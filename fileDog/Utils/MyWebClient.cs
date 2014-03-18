using System.Net;

namespace me.sibo.fileDog.Utils
{
    public class MyWebClient : WebClient
    {
        public MyWebClient()
        {
            Proxy = new WebProxy("127.0.0.1", 8087);
            Headers.Add("user-agent",
                "Mozilla/5.0 (Windows NT 6.1; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/33.0.1750.154 Safari/537.36");
        }
    }
}