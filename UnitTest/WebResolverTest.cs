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
            WebResolver.ResolveUrl("http://www.baidu.com");
        }

        [TestMethod]
        public void DownloadTest2()
        {
            WebResolver.DownloadFile("http://file.mockup-b.com/software/MockupBSetup.air"); ;
        }
    }
}
