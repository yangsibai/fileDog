﻿namespace me.sibo.fileDog.Service
{
    public static class NetScheduler
    {
        private static int _netLimit = 20;

        private static readonly object Lock = new object();

        /// <summary>
        ///     网络数量
        /// </summary>
        public static int NetCount
        {
            get { return _netLimit; }
        }

        /// <summary>
        /// reset net count
        /// </summary>
        public static void Reset()
        {
            _netLimit = 10;
        }

        /// <summary>
        ///     借用
        /// </summary>
        /// <returns></returns>
        public static bool Borrow()
        {
            if (_netLimit > 0)
            {
                lock (Lock)
                {
                    _netLimit--;
                }
                return true;
            }
            return false;
        }

        /// <summary>
        ///     归还
        /// </summary>
        public static void Return()
        {
            lock (Lock)
            {
                _netLimit++;
            }
        }
    }
}