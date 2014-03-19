namespace me.sibo.fileDog.Model
{
    /// <summary>
    /// 文件类型
    /// </summary>
    public class FileType
    {
        private FileCategory _parent;

        /// <summary>
        /// 文件介绍
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 文件扩展名
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        /// 简单介绍
        /// </summary>
        public string Description { get; set; }


        private bool _isCheck = false;

        public bool IsCheck
        {
            get
            {
                return _isCheck;
            }
            set
            {
                _isCheck = value;
            }
        }
    }
}
