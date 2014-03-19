using System;
using System.Collections.Generic;
using System.ComponentModel;
using fileDog.Annotations;

namespace me.sibo.fileDog.Model
{
    /// <summary>
    ///     文件类型
    /// </summary>
    public class FileType:INotifyPropertyChanged
    {
        public FileType(FileCategory parent)
        {
            _parent = parent;
        }

        private bool? _isChecked = false;
        private FileCategory _parent;

        public FileCategory Parent
        {
            set
            {
                _parent = value;
            }
        }

        #region IsChecked

        public bool? IsChecked
        {
            get { return _isChecked; }

            set
            {
                _isChecked = value;
                if (_parent != null)
                {
                    _parent.UpdateState();                    
                }
                this.OnPropertyChanged("IsChecked");
            }
        }

        /// <summary>
        /// 父元素直接设置状态，不需要再通知父元素更新状态了
        /// </summary>
        /// <param name="value"></param>
        public void SetCheckedByParent(bool value)
        {
            this._isChecked = value;
            this.OnPropertyChanged("IsChecked");
        }

        #endregion
        /// <summary>
        ///     文件介绍
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        ///     文件扩展名
        /// </summary>
        public string Extension { get; set; }

        /// <summary>
        ///     简单介绍
        /// </summary>
        public string Description { get; set; }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}