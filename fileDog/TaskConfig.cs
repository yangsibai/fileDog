using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using fileDog.Annotations;

namespace fileDog
{
    /// <summary>
    /// 任务设置
    /// </summary>
    public class TaskConfig
    {
        /// <summary>
        /// 开始地址
        /// </summary>
        public String StartURL { get; set; }

        /// <summary>
        /// min size
        /// </summary>
        public int FileMinSize { get; set; }

        /// <summary>
        /// max size
        /// </summary>
        public int FileMaxSize { get; set; }

        /// <summary>
        /// 重命名文件
        /// </summary>
        public bool RenameFile { get; set; }

        /// <summary>
        /// 启用代理
        /// </summary>
        public bool EnableProxy { get; set; }
    }
}
