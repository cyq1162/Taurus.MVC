using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace System.Net.Http
{
    /// <summary>
    /// 只要不关闭链接，链接即时复用。
    /// </summary>
    internal class HttpWebClient : WebClient
    {
        /// <summary>
        /// 是否允许重定向
        /// </summary>
        public bool AllowAutoRedirect { get; set; }
        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout { get; set; }

        bool isHeadRequest = false;
        private WebRequest GetHttpWebRequestByNet(Uri address)
        {

            //必须使用适当的属性或方法修改“Content-Length”标头。
            var accept = Headers["Accept"];
            var connection = Headers["Connection"];
            var proxyConnection = Headers["Proxy-Connection"];
            var length = Headers["Content-Length"];
            var contentType = Headers["Content-Type"];
            var referer = Headers["Referer"];
            var userAgent = Headers["User-Agent"];
            var range = Headers["Range"];
            var ifModifiedSince = Headers["If-Modified-Since"];
            var expect = Headers["Expect"];
            //var date = Headers["Date"];
            #region MyRegion

            if (accept != null)
            {
                Headers.Remove("Accept");
            }
            if (connection != null)
            {
                Headers.Remove("Connection");
            }
            if (proxyConnection != null)
            {
                Headers.Remove("Proxy-Connection");
            }
            if (length != null)
            {
                Headers.Remove("Content-Length");
            }
            if (contentType != null)
            {
                Headers.Remove("Content-Type");
            }
            if (referer != null)
            {
                Headers.Remove("Referer");
            }
            if (userAgent != null)
            {
                Headers.Remove("User-Agent");
            }
            if (range != null)
            {
                Headers.Remove("Range");
            }
            if (ifModifiedSince != null)
            {
                Headers.Remove("If-Modified-Since");
            }
            if (expect != null)
            {
                Headers.Remove("Expect");
            }

            #endregion

            WebRequest webRequest = base.GetWebRequest(address);
            if (webRequest is HttpWebRequest)
            {
                HttpWebRequest request = (HttpWebRequest)webRequest;
                #region MyRegion

                if (accept != null)
                {
                    request.Accept = accept;
                }
                if (connection != null)
                {
                    //request.Connection = connection;
                    request.KeepAlive = connection == "keep-alive";
                }
                else if (proxyConnection != null)
                {
                    //request.Connection = proxyConnection;
                    request.KeepAlive = proxyConnection == "keep-alive";
                }
                if (length != null)
                {
                    request.ContentLength = long.Parse(length);
                }
                if (contentType != null)
                {
                    request.ContentType = contentType;
                }
                if (referer != null)
                {
                    request.Referer = referer;
                }
                if (userAgent != null)
                {
                    request.UserAgent = userAgent;
                }
                if (range != null)
                {
                    //Range: bytes=10- ：第10个字节及最后个字节的数据
                    //Range: bytes=40-100 ：第40个字节到第100个字节之间的数据.
                    string[] items = range.Split('=')[0].Split('-');
                    int start = 0;
                    int end = 0;
                    int.TryParse(items[0], out start);
                    int.TryParse(items[1], out end);
                    if (end > start)
                    {
                        request.AddRange(start, end);
                    }
                    else
                    {
                        request.AddRange(start);
                    }
                }
                if (ifModifiedSince != null)
                {
                    DateTime dt;
                    if (DateTime.TryParse(ifModifiedSince, out dt))
                    {
                        request.IfModifiedSince = dt;
                    }
                }
                if (expect != null)
                {
                    request.Expect = expect;
                }
                #endregion
                return request;
            }
            else
            {
                //if (webRequest is FileWebRequest)
                //{
                //    FileWebRequest request = (FileWebRequest)webRequest;
                //    if (length != null)
                //    {
                //        request.ContentLength = long.Parse(length);
                //    }
                //    request.
                //}
                //FileWebRequest
                #region MyRegion
                if (accept != null)
                {
                    Headers.Add("Accept", accept);
                }
                if (connection != null)
                {
                    Headers.Add("Connection", connection);
                }
                if (proxyConnection != null)
                {
                    Headers.Add("Proxy-Connection", proxyConnection);
                }
                if (length != null)
                {
                    Headers.Add("Content-Length", length);
                }
                if (contentType != null)
                {
                    Headers.Add("Content-Type", contentType);
                }
                if (referer != null)
                {
                    Headers.Add("Referer", referer);
                }
                if (userAgent != null)
                {
                    Headers.Add("User-Agent", userAgent);
                }
                if (range != null)
                {
                    Headers.Add("Range", range);
                }
                if (ifModifiedSince != null)
                {
                    Headers.Add("If-Modified-Since", ifModifiedSince);
                }
                if (expect != null)
                {
                    Headers.Add("Expect", expect);
                }
                #endregion
            }
            return webRequest;
        }
        protected override WebRequest GetWebRequest(Uri address)
        {
            try
            {
                WebRequest webRequest = null;
                if (AppConst.IsNetCore)
                {
                    webRequest = base.GetWebRequest(address);
                }
                else
                {
                    webRequest = GetHttpWebRequestByNet(address);
                }
                if (webRequest is HttpWebRequest)
                {
                    HttpWebRequest httpWebRequest = webRequest as HttpWebRequest;
                    httpWebRequest.AllowAutoRedirect = this.AllowAutoRedirect;
                    httpWebRequest.Proxy = this.Proxy;
                    httpWebRequest.KeepAlive = true;
                }
                if (isHeadRequest)
                {
                    webRequest.Method = "HEAD";
                    isHeadRequest = false;
                }
                if (Timeout.TotalMilliseconds > 0)
                {
                    webRequest.Timeout = (int)Timeout.TotalMilliseconds;
                }
                webRequest.Proxy = null;
                return webRequest;
            }
            catch (Exception err)
            {
                CYQ.Data.Log.Write(err, LogType.Taurus);
                throw;
            }
        }
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            WebResponse response = null;
            try
            {
                response = base.GetWebResponse(request);
            }
            catch (WebException err)
            {
                if (err.Response == null)
                {
                    throw;
                }
                response = err.Response;
            }
            return response;
        }
        public void Head(string url)
        {
            isHeadRequest = true;
            DownloadData(url);
        }
    }
}
