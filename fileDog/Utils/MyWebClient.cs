using System.Net;

namespace me.sibo.fileDog.Utils
{
    public class MyWebClient : WebClient
    {
        public MyWebClient()
        {
            var config = TaskConfig.GetInstance();
            if (config.EnableProxy)
            {
                Proxy = new WebProxy(config.ProxyHost, config.ProxyPort);
            }
            else
            {
                Proxy = null;
            }
            Headers.Add("user-agent",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.154 Safari/537.36");
        }
    }
}