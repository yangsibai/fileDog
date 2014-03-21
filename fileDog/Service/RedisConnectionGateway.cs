using System;
using System.Net.Sockets;
using BookSleeve;

namespace me.sibo.fileDog.Service
{
    /// <summary>
    /// redis 连接管理
    /// </summary>
    public sealed class RedisConnectionGateway
    {
        private const string RedisConnectionFailed = "Redis connection failed.";
        private RedisConnection _connection;
        private static volatile RedisConnectionGateway _instance;

        private static object syncLock = new object();
        private static object syncConnectionLock = new object();

        public static RedisConnectionGateway Current
        {
            get
            {
                if (_instance == null)
                {
                    lock (syncLock)
                    {
                        if (_instance == null)
                        {
                            _instance = new RedisConnectionGateway();
                        }
                    }
                }

                return _instance;
            }
        }

        private RedisConnectionGateway()
        {
            _connection = GetNewConnection();
        }

        /// <summary>
        /// 创建连接，允许管理员操作
        /// </summary>
        /// <returns></returns>
        private static RedisConnection GetNewConnection()
        {
            return new RedisConnection("127.0.0.1" /* change with config value of course */, syncTimeout: 5000, ioTimeout: 5000,allowAdmin:true);
        }

        /// <summary>
        /// 获取连接
        /// </summary>
        /// <returns></returns>
        public RedisConnection GetConnection()
        {
            lock (syncConnectionLock)
            {
                if (_connection == null)
                    _connection = GetNewConnection();

                if (_connection.State == RedisConnectionBase.ConnectionState.Opening)
                    return _connection;

                if (_connection.State == RedisConnectionBase.ConnectionState.Closing || _connection.State == RedisConnectionBase.ConnectionState.Closed)
                {
                    try
                    {
                        _connection = GetNewConnection();
                    }
                    catch (Exception ex)
                    {
                        throw new Exception(RedisConnectionFailed, ex);
                    }
                }

                if (_connection.State == RedisConnectionBase.ConnectionState.New)
                {
                    try
                    {
                        var openAsync = _connection.Open();
                        _connection.Wait(openAsync);
                    }
                    catch (SocketException ex)
                    {
                        throw new Exception(RedisConnectionFailed, ex);
                    }
                }

                return _connection;
            }
        }
    }
}
