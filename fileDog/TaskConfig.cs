using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using fileDog.Annotations;
using me.sibo.fileDog.Model;

namespace me.sibo.fileDog
{
    /// <summary>
    ///     任务设置
    /// </summary>
    public class TaskConfig
    {
        /// <summary>
        ///     配置保存
        /// </summary>
        public static TaskConfig Config = new TaskConfig
        {
            EnableProxy = false,
            FileMaxSize = 1024,
            FileMinSize = 20,
            RenameFile = false,
            StartURL = "http://tieba.baidu.com"
        };

        /// <summary>
        ///     开始地址
        /// </summary>
        public String StartURL { get; set; }

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
        /// 获取所有选中的文件的后缀名
        /// </summary>
        /// <returns></returns>
        public static string GetFileExtensions()
        {
            string str = "";
            if (Config != null)
            {
                str = Config.FileCategories.Aggregate(str, (current1, cat) => cat.FileTypes.Where(fileType => fileType.Check).Aggregate(current1, (current, fileType) => current + fileType.Extension));
            }
            return str.Replace(".","\\.");
        }
    }
}