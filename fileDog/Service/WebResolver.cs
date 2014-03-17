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
        public static Result ResolveUrl(string url = "")
        {
            try
            {
                if (string.IsNullOrEmpty(url))
                {
                    url = Redis.PopUrl();
                }
                if (string.IsNullOrEmpty(url)) return new Result("url is empty");
                var client = new WebClient();
                var pageContent = client.DownloadString(new Uri(url));

                var config = TaskConfig.GetInstance();
                var imgSrcRegex = new Regex("src=('|\")([^\"']*)('|\")", RegexOptions.IgnoreCase);
                MatchCollection matches = imgSrcRegex.Matches(pageContent);

                var fileList = new List<string>();
                Uri baseUri=new Uri(url);
                Uri newUri;
                foreach (Match match in matches)
                {
                    newUri=new Uri(baseUri,match.Groups[2].Value);
                    if (config.FilePattern().IsMatch(newUri.AbsoluteUri))
                    {
                        fileList.Add(newUri.AbsoluteUri);   
                    }
                }

                var hrefRegex = new Regex("href=('|\")([^\"']*)('|\")", RegexOptions.IgnoreCase);
                MatchCollection hrefMatches = hrefRegex.Matches(pageContent);
                var urlList = new List<string>();
                foreach (Match match in hrefMatches)
                {
                    newUri = new Uri(baseUri, match.Groups[2].Value);
                    if (config.URLPattern().IsMatch(newUri.AbsoluteUri))
                    {
                        urlList.Add(newUri.AbsoluteUri);                        
                    }
                }
                Redis.PushFileUrl(fileList.ToArray());
                Redis.PushUrl(urlList.ToArray());
                return new Result(true,
                    "success:" + url + " fileCount:" + fileList.Count + " urlCount:" + urlList.Count);
            }
            catch (Exception e)
            {
                return new Result(e.Message);
            }
        }

        /// <summary>
        /// 下载文件
        /// </summary>
        public static Result DownloadFile()
        {
            try
            {
                var config = TaskConfig.GetInstance();
                string fileUrl = Redis.PopFileUrl();
                if (string.IsNullOrEmpty(fileUrl))
                {
                    return new Result("no file url");
                }

                WebRequest webRequest = HttpWebRequest.Create(fileUrl);
                webRequest.Method = "HEAD";
                using (WebResponse response = webRequest.GetResponse())
                {
                    var contentLength = response.Headers.Get("content-length");
                    int contentLengthByte;
                    if (Int32.TryParse(contentLength, out contentLengthByte))
                    {
                        if (contentLengthByte >= config.FileMinSize * 1024 && contentLengthByte <= config.FileMaxSize*1024)
                        {
                            var startURL = TaskConfig.GetInstance().StartURL;

                            var fileSaveDir = Path.Combine(Directory.GetCurrentDirectory(), new Uri(startURL).Host);
                            if (!Directory.Exists(fileSaveDir))
                            {
                                Directory.CreateDirectory(fileSaveDir);
                            }
                            var extension = Path.GetExtension(fileUrl).ToLower();
                            var fileName = Guid.NewGuid().ToString() + extension;
                            var filePath = Path.Combine(fileSaveDir, fileName);
                            using (var client = new WebClient())
                            {
                                client.DownloadFile(new Uri(fileUrl),filePath);
                                Redis.FileDownloaded(fileUrl);
                                return new Result(true,"download "+fileUrl+" successfully");
                            }
                        }
                    }
                    return new Result("not download:" + fileUrl);
                }
            }
            catch (Exception e)
            {
                return new Result(e.Message);
            }
        }
    }
}