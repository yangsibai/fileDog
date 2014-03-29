using System;
using System.IO;
using System.Reflection;

namespace me.sibo.fileDog.Service
{
    /// <summary>
    /// 记录日志
    /// </summary>
    public static class Logger
    {
        private static readonly StreamWriter sw;

        static Logger()
        {
            try
            {
                string dir = Path.Combine(Directory.GetCurrentDirectory(), "Log");
                if (!Directory.Exists(dir))
                {
                    Directory.CreateDirectory(dir);
                }
                string logFileName = TaskConfig.GetInstance().GetTaskHost() + "_" +
                                     DateTime.Now.ToString("yy-MM-dd HH_mm") +
                                     ".log";
                sw = File.AppendText(Path.Combine(dir, logFileName));
                sw.AutoFlush = true;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        /// <summary>
        /// 记录日志信息
        /// </summary>
        /// <param name="message"></param>
        public static void AppendLog(string message)
        {
            if (sw != null)
            {
                sw.WriteLine(message);
            }
        }

        /// <summary>
        /// 记录异常
        /// </summary>
        /// <param name="method">方法信息</param>
        /// <param name="e"></param>
        public static void Error(MethodBase method,Exception e)
        {
            if (sw != null)
            {
                sw.WriteLine(DateTime.Now.ToString("MM-dd HH:mm:ss => ")+"error in method:"+method.Name+" "+e.Message);
            }
        }
    }
}