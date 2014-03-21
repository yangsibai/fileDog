using System;
using me.sibo.fileDog.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class RedisTest
    {
        [TestMethod]
        public void FlushDbTest()
        {
            Redis.FlushDb();
        }
    }
}
