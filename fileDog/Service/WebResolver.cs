using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace me.sibo.fileDog.Service
{
    public static class WebResolver
    {
        /// <summary>
        ///     解析对应的地址
        /// </summary>
        public static void ResolverUrl(string url = "")
        {
            if (string.IsNullOrEmpty(url))
            {
                url = Redis.PopUrl();
            }
            if (string.IsNullOrEmpty(url)) return;
            var client = new WebClient();
            var pageContent = client.DownloadString(new Uri(url));


            var fileList = new List<string>();
            var urlList = new List<string>();

            var imgSrcRegex = new Regex("src=('|\")([^\"']*)('|\")", RegexOptions.IgnoreCase);
            MatchCollection matches = imgSrcRegex.Matches(pageContent);
            foreach (Match match in matches)
            {
                fileList.Add(match.Groups[2].Value);
            }

            var hrefRegex = new Regex("href=('|\")([^\"']*)('|\")", RegexOptions.IgnoreCase);
            MatchCollection hrefMatches = hrefRegex.Matches(pageContent);
            foreach (Match match in hrefMatches)
            {
                urlList.Add(match.Groups[2].Value);
            }
            System.Console.WriteLine(fileList.Count);
            System.Console.WriteLine(urlList.Count);
            Redis.PushFileUrl(fileList.ToArray());
            Redis.PushUrl(urlList.ToArray());
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileUrl"></param>
        public static void DownloadFile(string fileUrl="")
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                fileUrl = Redis.PopFileUrl();
            }

            WebRequest webRequest = HttpWebRequest.Create(fileUrl);
            webRequest.Method = "HEAD";
            using (WebResponse response=webRequest.GetResponse())
            {
                var contentLength = response.Headers.Get("content-length");
            }
        }
    }
}