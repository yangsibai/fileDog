using System;
using System.Linq;
using System.Reflection;
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
        private const int DefaultDb = 0;

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
            try
            {
                if (urls == null || !urls.Any())
                {
                    return;
                }
                using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
                {
                    conn.Open().Wait();
                    string guid = Guid.NewGuid().ToString();
                    conn.Sets.Add(DefaultDb, guid, urls).Wait();
                    string[] keys = {guid, UrlChecked};
                    Task<byte[][]> task = conn.Sets.Difference(DefaultDb, keys);
                    task.Wait();
                    conn.Sets.Add(DefaultDb, UrlsCacheKey, task.Result).Wait();
                    conn.Keys.Remove(DefaultDb, guid); //remove guid cache
                }
            }
            catch (Exception e)
            {
                Logger.Error(MethodBase.GetCurrentMethod(),e);
            }
        }

        /// <summary>
        ///     获取一个地址
        /// </summary>
        /// <returns></returns>
        public static string PopUrl()
        {
            try
            {
                using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
                {
                    conn.Open().Wait();
                    Task<byte[]> task = conn.Sets.RemoveRandom(DefaultDb, UrlsCacheKey);
                    task.Wait();
                    if (task.Result != null && task.Result.Any())
                    {
                        conn.Sets.Add(DefaultDb, UrlChecked, task.Result);
                        return Encoding.Default.GetString(task.Result);
                    }
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                Logger.Error(MethodBase.GetCurrentMethod(), e);
                throw;
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
            try
            {
                using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
                {
                    conn.Open().Wait();
                    string guid = Guid.NewGuid().ToString();
                    conn.Sets.Add(DefaultDb, guid, fileUrls).Wait();
                    string[] keys = {guid, FilesUrlCachekey};
                    Task<byte[][]> task = conn.Sets.Difference(DefaultDb, keys);
                    task.Wait();
                    conn.Sets.Add(DefaultDb, FilesUrlCachekey, task.Result).Wait();
                    conn.Keys.Remove(DefaultDb, guid); //remove guid cache
                }
            }
            catch (Exception e)
            {
                Logger.Error(MethodBase.GetCurrentMethod(),e);
            }
        }

        /// <summary>
        ///     pop文件地址
        /// </summary>
        /// <returns></returns>
        public static string PopFileUrl()
        {
            try
            {
                using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
                {
                    conn.Open().Wait();
                    Task<byte[]> task = conn.Sets.RemoveRandom(DefaultDb, FilesUrlCachekey);
                    task.Wait();
                    if (task.Result != null && task.Result.Any())
                    {
                        conn.Sets.Add(DefaultDb, FileUrlChecked, task.Result);
                        return Encoding.Default.GetString(task.Result);
                    }
                    return string.Empty;
                }
            }
            catch (Exception e)
            {
                Logger.Error(MethodBase.GetCurrentMethod(),e);
                throw;
            }
        }

        /// <summary>
        /// 清除所有数据
        /// </summary>
        public static void FlushDb()
        {
            try
            {
                using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
                {
                    conn.Open().Wait();
                    conn.Server.FlushDb(DefaultDb).Wait();
                }
            }
            catch (Exception e)
            {
                Logger.Error(MethodBase.GetCurrentMethod(),e);
            }
        }

        /// <summary>
        ///     添加下载完毕的url
        /// </summary>
        /// <param name="url"></param>
        /// <returns></returns>
        public static void FileDownloaded(string url)
        {
            try
            {
                using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
                {
                    conn.Open().Wait();
                    conn.Sets.Add(DefaultDb, FileDownloadedCachekey, url);
                }
            }
            catch (Exception e)
            {
                Logger.Error(MethodBase.GetCurrentMethod(),e);
            }
        }

        /// <summary>
        /// 获取任务状态
        /// </summary>
        /// <returns></returns>
        public static TaskInfo GetTaskStatus()
        {
            try
            {
                using (RedisConnection conn = RedisConnectionGateway.Current.GetConnection())
                {
                    conn.Open().Wait();
                    var taskInfo = new TaskInfo();
                    Task[] tasks =
                    {
                        conn.Sets.GetLength(DefaultDb, UrlsCacheKey).ContinueWith(cont => { taskInfo.UrlCount = cont.Result; }),
                        conn.Sets.GetLength(DefaultDb, UrlChecked)
                            .ContinueWith(cont => { taskInfo.UrlCheckedCount = cont.Result; }),
                        conn.Sets.GetLength(DefaultDb, FilesUrlCachekey)
                            .ContinueWith(cont => { taskInfo.FileUrlCount = cont.Result; }),
                        conn.Sets.GetLength(DefaultDb, FileUrlChecked)
                            .ContinueWith(cont => { taskInfo.FileCheckedCount = cont.Result; }),
                        conn.Sets.GetLength(DefaultDb, FileDownloadedCachekey)
                            .ContinueWith(cont => { taskInfo.DownloadFileCount = cont.Result; })
                    };

                    Task.WaitAll(tasks);
                    return taskInfo;
                }
            }
            catch (Exception e)
            {
                Logger.Error(MethodBase.GetCurrentMethod(),e);
            }
            return null;
        }
    }
}