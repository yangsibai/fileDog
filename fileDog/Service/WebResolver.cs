using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Web;
using me.sibo.fileDog.Model;

namespace me.sibo.fileDog.Service
{
    public static class WebResolver
    {
        /// <summary>
        ///     解析对应的地址
        /// </summary>
        public static Feedback ResolverUrl(string url = "")
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    url = Redis.PopUrl();
                }
                if (string.IsNullOrEmpty(url)) return new Feedback("url is empty");
                var client = new WebClient();
                var pageContent = client.DownloadString(new Uri(url));


                var fileList = new List<string>();
                var urlList = new List<string>();

                var imgSrcRegex = new Regex("src=('|\")([^\"']*)('|\")", RegexOptions.IgnoreCase);
                MatchCollection matches = imgSrcRegex.Matches(pageContent);

                Uri baseUri=new Uri(url);
                Uri newUri;
                foreach (Match match in matches)
                {
                    newUri=new Uri(baseUri,match.Groups[2].Value);
                    fileList.Add(newUri.AbsoluteUri);
                }

                var hrefRegex = new Regex("href=('|\")([^\"']*)('|\")", RegexOptions.IgnoreCase);
                MatchCollection hrefMatches = hrefRegex.Matches(pageContent);
                foreach (Match match in hrefMatches)
                {
                    newUri = new Uri(baseUri, match.Groups[2].Value);
                    urlList.Add(newUri.AbsoluteUri);
                }
                Redis.PushFileUrl(fileList.ToArray());
                Redis.PushUrl(urlList.ToArray());
                return new Feedback(true,
                    "success:" + url + " fileCount:" + fileList.Count + " urlCount:" + urlList.Count);
            }
            catch (Exception e)
            {
                return new Feedback(e.Message);
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        /// <param name="fileUrl"></param>
        public static Feedback DownloadFile(string fileUrl = "")
        {
            try
            {
                if (string.IsNullOrEmpty(fileUrl))
                {
                    fileUrl = Redis.PopFileUrl();
                }

                WebRequest webRequest = HttpWebRequest.Create(fileUrl);
                webRequest.Method = "HEAD";
                using (WebResponse response = webRequest.GetResponse())
                {
                    var contentLength = response.Headers.Get("content-length");
                    return new Feedback(true, " size:" + contentLength+" url:" + fileUrl);
                }
            }
            catch (Exception e)
            {
                return new Feedback(e.Message);
            }
        }
    }
}