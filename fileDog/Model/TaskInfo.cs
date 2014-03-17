using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace me.sibo.fileDog.Model
{
    public class TaskInfo
    {
        /// <summary>
        /// 待解析url数量
        /// </summary>
        public long UrlCount { get; set; }

        /// <summary>
        /// 检查过的url数量
        /// </summary>
        public long UrlCheckedCount { get; set; }

        /// <summary>
        /// 文件url数量
        /// </summary>
        public long FileUrlCount { get; set; }

        /// <summary>
        /// 已经检查过的文件数量
        /// </summary>
        public long FileCheckedCount { get; set; }

        /// <summary>
        /// 已经下载过的文件数量
        /// </summary>
        public long DownloadFileCount { get; set; }
    }
}
