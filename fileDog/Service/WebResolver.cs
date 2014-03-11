using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;

namespace fileDog.Service
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
            client.DownloadStringCompleted += client_DownloadStringCompleted;
            client.DownloadStringAsync(new Uri(url));
        }

        /// <summary>
        ///     处理下载后的内容
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private static void client_DownloadStringCompleted(object sender, DownloadStringCompletedEventArgs e)
        {
            if (e.Error != null) return;

            string pageContent = e.Result;

            var imList = new List<string>();
            var regex = new Regex("src=('|\")([^\"']*)('|\")", RegexOptions.IgnoreCase);
            MatchCollection matches = regex.Matches(pageContent);
            foreach (Match match in matches)
            {
                imList.Add(match.Groups[2].Value);
            }

            var hrefList = new List<string>();
            var hrefRegex = new Regex("href=('|\")([^\"']*)('|\")", RegexOptions.IgnoreCase);
            MatchCollection hrefMatches = regex.Matches(pageContent);
            foreach (Match match in hrefMatches)
            {
                hrefList.Add(match.Groups[2].Value);
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileUrl"></param>
        public static void DownloadFile(string fileUrl)
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