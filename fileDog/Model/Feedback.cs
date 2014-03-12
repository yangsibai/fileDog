using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace me.sibo.fileDog.Model
{
    /// <summary>
    /// 反馈
    /// </summary>
    public class Feedback
    {
        public Feedback(string errorMessage)
        {
            Success = false;
            Message = errorMessage;
        }

        public Feedback(bool success, string message)
        {
            Success = success;
            Message = message;
        }

        /// <summary>
        /// 是否成功
        /// </summary>
        public bool Success { get; set; }

        /// <summary>
        /// 信息
        /// </summary>
        public string Message { get; set; }
    }
}
