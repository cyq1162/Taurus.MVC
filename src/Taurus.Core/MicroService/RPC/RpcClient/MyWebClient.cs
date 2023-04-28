using System;
using System.Net;

namespace Taurus.MicroService
{
    internal class MyWebClient : WebClient
    {
        private int _ResponseStatusCode = 0;
        public int ResponseStatusCode
        {
            get
            {
                return _ResponseStatusCode;
            }
            set
            {
                _ResponseStatusCode = value;
            }
        }
        bool isHeadRequest = false;
        protected override WebRequest GetWebRequest(Uri address)
        {
            try
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
                //if (date != null)
                //{
                //    Headers.Remove("Date");
                //}
                #endregion

                HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);

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
                //if (date != null)
                //{
                //    request.SendChunked
                //}
                #endregion

                request.Proxy = null;
                request.AllowAutoRedirect = false;
                if (isHeadRequest)
                {
                    request.Method = "HEAD";
                    isHeadRequest = false;
                }
                return request;
            }
            catch (Exception err)
            {
                CYQ.Data.Log.Write(err);
                throw;
            }
        }
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)base.GetWebResponse(request);

                return response;
            }
            catch (WebException err)
            {
                if (err.Response == null)
                {
                    throw;
                }
                response = (HttpWebResponse)err.Response;
            }
            finally
            {
                if (response != null)
                {
                    _ResponseStatusCode = (int)response.StatusCode;
                }

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
