using System;
using me.sibo.fileDog.Service;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTest
{
    [TestClass]
    public class WebResolverTest
    {
        [TestMethod]
        public void ResolveUrl()
        {
            WebResolver.ResolverUrl("http://www.baidu.com");
        }

        [TestMethod]
        public void DownloadTest()
        {
            var path = "http://www.ireadhome.com/Content/Images/Home/iNote.png";
            WebResolver.DownloadFile(path);;
        }

        [TestMethod]
        public void DownloadTest2()
        {
            WebResolver.DownloadFile(); ;
        }
    }
}
