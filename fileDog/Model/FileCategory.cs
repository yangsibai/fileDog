using System.Collections.Generic;
using System.ComponentModel;
using fileDog.Annotations;

namespace me.sibo.fileDog.Model
{
    /// <summary>
    ///     文件分类
    /// </summary>
    public class FileCategory:INotifyPropertyChanged
    {
        public FileCategory()
        {
            this.IsChecked = false;
        }

        private bool? _isChecked = false;

        public bool? IsChecked
        {
            get { return _isChecked; }
            set
            {
                if (value == _isChecked) return;

                _isChecked = value;
                if (_isChecked.HasValue)
                {
                    FileTypes.ForEach(f => f.SetCheckedByParent(_isChecked.Value));
                }
                UpdateState();
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

        public void UpdateState()
        {
            bool? state = null;
            for (var i = 0; i < FileTypes.Count; i++)
            {
                var current = FileTypes[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            _isChecked = state;
            this.OnPropertyChanged("IsChecked");
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}