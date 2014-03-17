using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using me.sibo.fileDog.Model;
using Newtonsoft.Json;

namespace me.sibo.fileDog
{
    /// <summary>
    ///     任务设置
    /// </summary>
    public sealed class TaskConfig
    {
        private static readonly object Lock = new object();
        private static TaskConfig _instance;

        private TaskConfig()
        {
            
        }

        public static TaskConfig GetInstance()
        {
            if (_instance == null)
            {
                lock (Lock)
                {
                    if (_instance == null)
                    {
                        var configFilePath = Path.Combine(Directory.GetCurrentDirectory(), "Source", "config.json");
                        string configJson = File.ReadAllText(configFilePath);
                        _instance = JsonConvert.DeserializeObject<TaskConfig>(configJson);
                    }
                }
            }
            return _instance;
        }

        /// <summary>
        ///     开始地址
        /// </summary>
        public String StartURL { get; set; }

        /// <summary>
        /// 匹配地址
        /// </summary>
        public string MatchURL { get; set; }

        /// <summary>
        ///     min size
        /// </summary>
        public int FileMinSize { get; set; }

        /// <summary>
        ///     max size
        /// </summary>
        public int FileMaxSize { get; set; }

        /// <summary>
        ///     重命名文件
        /// </summary>
        public bool RenameFile { get; set; }

        /// <summary>
        ///     启用代理
        /// </summary>
        public bool EnableProxy { get; set; }

        /// <summary>
        ///     文件列表
        /// </summary>
        public List<FileCategory> FileCategories { get; set; }

        /// <summary>
        /// 获取当前任务的hostname
        /// </summary>
        /// <returns></returns>
        public string GetTaskHost()
        {
            try
            {
                var host = new Uri(StartURL).Host;
                if (!string.IsNullOrEmpty(host))
                {
                    return host;
                }
            }
            catch
            {

            }
            return "unknown";
        }

        /// <summary>
        /// 获取匹配地址
        /// </summary>
        /// <returns></returns>
        public Regex URLPattern()
        {
            return new Regex(MatchURL.Replace("\n","|"),RegexOptions.IgnoreCase);
        }

        /// <summary>
        /// 获取文件路径匹配
        /// </summary>
        /// <returns></returns>
        public Regex FilePattern()
        {
            var checkedFiles = new List<string>();
            foreach (FileCategory fileCategory in _instance.FileCategories)
            {
                checkedFiles.AddRange(from fileType in fileCategory.FileTypes
                    where fileType.Check
                    select fileType.Extension);
            }
            var str="("+ String.Join("|", checkedFiles).Replace(".", "\\.")+")$";
            return new Regex(str,RegexOptions.IgnoreCase);
        }
    }
}