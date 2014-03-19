using System.Collections.Generic;

namespace me.sibo.fileDog.Model
{
    /// <summary>
    ///     文件分类
    /// </summary>
    public class FileCategory
    {
        private bool? _isCheck = false;

        public bool? IsCheck
        {
            get { return _isCheck; }
            set
            {
                if (value == _isCheck) return;

                _isCheck = value;
                if (_isCheck.HasValue)
                {
                    FileTypes.ForEach(f => f.IsCheck = true);
                }
            }
        }

        /// <summary>
        ///     分类名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     文件类型列表
        /// </summary>
        public List<FileType> FileTypes { get; set; }
    }
}