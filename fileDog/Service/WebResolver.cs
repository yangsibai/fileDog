using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using me.sibo.fileDog.Model;
using me.sibo.fileDog.Utils;

namespace me.sibo.fileDog.Service
{
    public static class WebResolver
    {
        /// <summary>
        ///     解析对应的地址
        /// </summary>
        public static Task<MyResult> ResolveUrl(string url)
        {
            if (string.IsNullOrEmpty(url))
            {
                return new Task<MyResult>(() => new MyResult("url is null or empty"));
            }
            try
            {
                if (string.IsNullOrEmpty(url)) return new Task<MyResult>(() => new MyResult("no url"));

                var client = new MyWebClient();

                string pageContent = client.DownloadString(new Uri(url));
                return new Task<MyResult>(() =>
                {
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
                    return new MyResult(true,
                        "success:" + url + " fileCount:" + fileList.Count + " urlCount:" + urlList.Count);
                });
            }
            catch (Exception e)
            {
                return new Task<MyResult>(() => new MyResult("resolve:" + url + " " + e.Message));
            }
        }

        /// <summary>
        ///     下载文件
        /// </summary>
        public static MyResult DownloadFile(string fileUrl)
        {
            if (string.IsNullOrEmpty(fileUrl))
            {
                return new MyResult("no file url"); 
            }
            try
            {
                TaskConfig config = TaskConfig.GetInstance();

                WebRequest webRequest = WebRequest.Create(fileUrl);
                if (config.EnableProxy)
                {
                    webRequest.Proxy = new WebProxy(config.ProxyHost, config.ProxyPort);
                }
                else
                {
                    webRequest.Proxy = null;
                }
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

                            string fileName;
                            if (TaskConfig.GetInstance().RenameFile)
                            {
                                fileName = Guid.NewGuid() + extension;
                            }
                            else
                            {
                                fileName = Path.GetFileName(fileUrl);
                            }
                            string filePath = Path.Combine(fileSaveDir, fileName);
                            using (var client = new MyWebClient())
                            {
                                client.DownloadFile(new Uri(fileUrl), filePath);
                                Redis.FileDownloaded(fileUrl);
                                return new MyResult(true, "download " + fileUrl + " successfully");
                            }
                        }
                    }
                    return new MyResult("not download:" + fileUrl);
                }
            }
            catch (Exception e)
            {
                return new MyResult("download: " + fileUrl + " error:" + e.Message);
            }
        }
    }
}