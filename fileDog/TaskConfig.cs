using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using fileDog.Annotations;
using me.sibo.fileDog.Model;
using Newtonsoft.Json;

namespace me.sibo.fileDog
{
    /// <summary>
    ///     任务设置
    /// </summary>
    public sealed class TaskConfig : INotifyPropertyChanged
    {
        private static readonly object Lock = new object();
        private static TaskConfig _instance;

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
                        foreach (var category in _instance.FileCategories)
                        {
                            category.UpdateState();
                            foreach (var fileType in category.FileTypes)
                            {
                                fileType.Parent=category;
                            }
                        }
                    }
                }
            }
            return _instance;
        }

        private string _startURL;

        public string StartURL
        {
            get
            {
                return _startURL;
            }
            set
            {
                _startURL = value;
                try
                {
                    MatchURL = new Uri(_startURL).Host.Replace("www.","");
                }
                catch (Exception e)
                {
                    MatchURL = "Start URL is invalid";
                }
                this.OnPropertyChanged("MatchURL");
            }
        }

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
        /// 代理地址
        /// </summary>
        public string ProxyHost { get; set; }

        /// <summary>
        /// 代理端口
        /// </summary>
        public int ProxyPort { get; set; }

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
                    where fileType.IsChecked.HasValue && fileType.IsChecked.Value
                    select fileType.Extension);
            }
            var str="("+ String.Join("|", checkedFiles).Replace(".", "\\.")+")$";
            return new Regex(str,RegexOptions.IgnoreCase);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        private void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}