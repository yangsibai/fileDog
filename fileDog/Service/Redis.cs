using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BookSleeve;
using me.sibo.fileDog.Model;

namespace me.sibo.fileDog.Service
{
    /// <summary>
    ///     redis操作
    /// </summary>
    public static class Redis
    {
        private const int db = 0;

        private static readonly string UrlsCacheKey = TaskConfig.GetInstance().GetTaskHost() + "_urls";
        private static readonly string UrlChecked = TaskConfig.GetInstance().GetTaskHost() + "_urlChecked";
        private static readonly string FilesUrlCachekey = TaskConfig.GetInstance().GetTaskHost() + "_fileUrls";
        private static readonly string FileUrlChecked = TaskConfig.GetInstance().GetTaskHost() + "_fileUrlsChecked";
        private static readonly string FileDownloadedCachekey = TaskConfig.GetInstance().GetTaskHost() +"_fileDownloaded";

        /// <summary>
        ///     添加多个地址
        /// </summary>
        /// <param name="urls"></param>
        public static void PushUrl(string[] urls)
        {
            if (urls == null || !urls.Any())
            {
                return;
            }
            using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                string guid = Guid.NewGuid().ToString();
                conn.Sets.Add(db, guid, urls).Wait();
                string[] keys = {guid, UrlChecked};
                Task<byte[][]> task = conn.Sets.Difference(db, keys);
                task.Wait();
                conn.Sets.Add(db, UrlsCacheKey, task.Result).Wait();
                conn.Keys.Remove(db, guid); //remove guid cache
            }
        }

        /// <summary>
        ///     获取一个地址
        /// </summary>
        /// <returns></returns>
        public static string PopUrl()
        {
            using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                Task<byte[]> task = conn.Sets.RemoveRandom(db, UrlsCacheKey);
                task.Wait();
                if (task.Result != null && task.Result.Any())
                {
                    conn.Sets.Add(db, UrlChecked, task.Result);
                    return Encoding.Default.GetString(task.Result);
                }
                return string.Empty;
            }
        }

        /// <summary>
        ///     push多个文件地址
        /// </summary>
        /// <param name="fileUrls"></param>
        public static void PushFileUrl(string[] fileUrls)
        {
            if (fileUrls == null || !fileUrls.Any())
            {
                return;
            }
            using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                string guid = Guid.NewGuid().ToString();
                conn.Sets.Add(db, guid, fileUrls).Wait();
                string[] keys = {guid, FilesUrlCachekey};
                Task<byte[][]> task = conn.Sets.Difference(db, keys);
                task.Wait();
                conn.Sets.Add(db, FilesUrlCachekey, task.Result).Wait();
                conn.Keys.Remove(db, guid); //remove guid cache
            }
        }

        /// <summary>
        ///     pop文件地址
        /// </summary>
        /// <returns></returns>
        public static string PopFileUrl()
        {
            using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                Task<byte[]> task = conn.Sets.RemoveRandom(db, FilesUrlCachekey);
                task.Wait();
                if (task.Result != null && task.Result.Any())
                {
                    conn.Sets.Add(db, FileUrlChecked, task.Result);
                    return Encoding.Default.GetString(task.Result);
                }
                return string.Empty;
            }
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public static void FlushDb()
        {
            using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                conn.Server.FlushAll();
            }
        }

        /// <summary>
        ///     添加下载完毕的url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static void FileDownloaded(string url)
        {
            using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                conn.Sets.Add(db, FileDownloadedCachekey, url);
            }
        }

        public static TaskInfo GeTaskInfo()
        {
            using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
            {
                conn.Open().Wait();
                var taskInfo = new TaskInfo();
                Task[] tasks =
                {
                    conn.Sets.GetLength(db, UrlsCacheKey).ContinueWith(cont => { taskInfo.UrlCount = cont.Result; }),
                    conn.Sets.GetLength(db, UrlChecked)
                        .ContinueWith(cont => { taskInfo.UrlCheckedCount = cont.Result; }),
                    conn.Sets.GetLength(db, FilesUrlCachekey)
                        .ContinueWith(cont => { taskInfo.FileUrlCount = cont.Result; }),
                    conn.Sets.GetLength(db, FileUrlChecked)
                        .ContinueWith(cont => { taskInfo.FileCheckedCount = cont.Result; }),
                    conn.Sets.GetLength(db, FileDownloadedCachekey)
                        .ContinueWith(cont => { taskInfo.DownloadFileCount = cont.Result; })
                };

                Task.WaitAll(tasks);
                return taskInfo;
            }
        }
    }
}