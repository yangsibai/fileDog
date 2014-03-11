using System.Collections.Generic;

namespace me.sibo.fileDog.Model
{
    /// <summary>
    /// 文件分类
    /// </summary>
    public class FileCategory
    {
        /// <summary>
        /// 分类名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件类型列表
        /// </summary>
        public IEnumerable<FileType> FileTypes { get; set; } 
    }
}
