using System;
using me.sibo.fileDog.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class RedisTest
    {
        [TestMethod]
        public void PushUrlTest()
        {
            Redis.PushUrl("http://www.baidu.com");
            var result = Redis.PopUrl();
            Assert.IsTrue(!string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void StartRedisServer()
        {
            Redis.StartRedisServer();
        }
    }
}
