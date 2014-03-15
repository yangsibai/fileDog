using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;

namespace me.sibo.fileDog.Service
{
    /// <summary>
    /// redis操作
    /// </summary>
    public static class Redis
    {
        private const string RedisHost = "127.0.0.1";
        private const int RedisPort = 6379;
        private const int db = 0;

        /// <summary>
        /// 添加一个地址
        /// </summary>
        /// <param name="url"></param>
        public static void PushUrl(string url)
        {
            using (var conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                conn.Sets.Add(db, "urls", url);
            }
        }

        /// <summary>
        /// 添加多个地址
        /// </summary>
        /// <param name="urls"></param>
        public static void PushUrl(string[] urls)
        {
            using (var conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                conn.Sets.Add(db, "urls", urls);
            }
        }

        /// <summary>
        /// 获取一个地址
        /// </summary>
        /// <returns></returns>
        public static string PopUrl()
        {
            using (var conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                var task= conn.Sets.GetRandom(db, "urls");
                task.Wait();
                if (task.Result != null && task.Result.Any())
                {
                    return Encoding.Default.GetString(task.Result);
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// push文件地址
        /// </summary>
        /// <param name="fileUrl"></param>
        public static void PushFileUrl(string fileUrl)
        {
            using (var conn=RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                conn.Sets.Add(db, "fileUrls", fileUrl);
            }
        }

        /// <summary>
        /// push多个文件地址
        /// </summary>
        /// <param name="fileUrls"></param>
        public static void PushFileUrl(string[] fileUrls)
        {
            using (var conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                conn.Sets.Add(db, "fileUrls", fileUrls);
            }
        }

        /// <summary>
        /// pop文件地址
        /// </summary>
        /// <returns></returns>
        public static string PopFileUrl()
        {
            using (var conn=RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                var task = conn.Sets.GetRandom(db, "fileUrls");
                task.Wait();
                if (task.Result != null && task.Result.Any())
                {
                    return Encoding.Default.GetString(task.Result);
                }
                return string.Empty;
            }
        }
    }
}
