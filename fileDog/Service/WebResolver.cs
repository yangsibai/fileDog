using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
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
            if (string.IsNullOrEmpty(url))
            {
                url = Redis.PopUrl();
            }
            try
            {
                if (string.IsNullOrEmpty(url)) return new Result("url is empty");
                var client = new WebClient();
                string pageContent = client.DownloadString(new Uri(url));

                TaskConfig config = TaskConfig.GetInstance();
                var imgSrcRegex = new Regex("src=('|\")([^\"']*)('|\")", RegexOptions.IgnoreCase);
                MatchCollection matches = imgSrcRegex.Matches(pageContent);

                var fileList = new List<string>();
                var baseUri = new Uri(url);
                Uri newUri;
                foreach (Match match in matches)
                {
                    newUri = new Uri(baseUri, match.Groups[2].Value);
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
                    if (config.FilePattern().IsMatch(newUri.AbsoluteUri))
                    {
                        fileList.Add(newUri.AbsoluteUri);
                    }
                    else if (config.URLPattern().IsMatch(newUri.AbsoluteUri))
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
                return new Result("resolve:" + url + " " + e.Message);
            }
        }

        /// <summary>
        ///     下载文件
        /// </summary>
        public static Result DownloadFile()
        {
            string fileUrl = Redis.PopFileUrl();
            if (string.IsNullOrEmpty(fileUrl))
            {
                return new Result("no file url");
            }
            try
            {
                TaskConfig config = TaskConfig.GetInstance();

                WebRequest webRequest = WebRequest.Create(fileUrl);
                webRequest.Method = "HEAD";
                using (WebResponse response = webRequest.GetResponse())
                {
                    string contentLength = response.Headers.Get("content-length");
                    int contentLengthByte;
                    if (Int32.TryParse(contentLength, out contentLengthByte))
                    {
                        if (contentLengthByte >= config.FileMinSize*1024 && contentLengthByte <= config.FileMaxSize*1024)
                        {
                            string startURL = TaskConfig.GetInstance().StartURL;

                            string fileSaveDir = Path.Combine(Directory.GetCurrentDirectory(), new Uri(startURL).Host);
                            if (!Directory.Exists(fileSaveDir))
                            {
                                Directory.CreateDirectory(fileSaveDir);
                            }
                            string extension = Path.GetExtension(fileUrl).ToLower();
                            string fileName = Guid.NewGuid() + extension;
                            string filePath = Path.Combine(fileSaveDir, fileName);
                            using (var client = new WebClient())
                            {
                                client.DownloadFile(new Uri(fileUrl), filePath);
                                Redis.FileDownloaded(fileUrl);
                                return new Result(true, "download " + fileUrl + " successfully");
                            }
                        }
                    }
                    return new Result("not download:" + fileUrl);
                }
            }
            catch (Exception e)
            {
                return new Result("download: " + fileUrl + " error:" + e.Message);
            }
        }
    }
}