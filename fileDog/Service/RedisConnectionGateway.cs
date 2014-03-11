using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using BookSleeve;

namespace fileDog.Service
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

        private static RedisConnection GetNewConnection()
        {
            return new RedisConnection("127.0.0.1" /* change with config value of course */, syncTimeout: 5000, ioTimeout: 5000);
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
