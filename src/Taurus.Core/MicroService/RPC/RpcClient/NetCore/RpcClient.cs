using System.Net.Http;
using System.Net;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Taurus.MicroService
{

    internal class RpcClient
    {
        static RpcClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | (SecurityProtocolType)12288;
        }
        private int _ResponseStatusCode = 0;
        /// <summary>
        /// 请求返回的状态码。
        /// </summary>
        public int ResponseStatusCode
        {
            get
            {
                if (wc != null)
                {
                    return wc.ResponseStatusCode;
                }
                return _ResponseStatusCode;
            }
        }
        private int _Timeout = 0;
        /// <summary>
        /// 请求超时，单位：毫秒（ms）。
        /// </summary>
        public int Timeout
        {
            get
            {
                if (wc != null)
                {
                    return wc.Timeout;
                }
                if (_Timeout <= 0)
                {
                    _Timeout = MsConfig.Server.GatewayTimeout * 1000;
                }
                return _Timeout;
            }
            set
            {
                if (wc != null)
                {
                    wc.Timeout = value;
                }
                else
                {
                    _Timeout = value;
                }
            }
        }
        private static bool isNet6 = Environment.Version.Major >= 6;
        MyWebClient wc;
        public RpcClient()
        {
            if (isNet6)
            {
                wc = new MyWebClient();
            }
        }

        private WebHeaderCollection _Headers = null;
        public WebHeaderCollection Headers
        {
            get
            {
                if (wc != null)
                {
                    return wc.Headers;
                }
                if (_Headers == null)
                {
                    _Headers = new WebHeaderCollection();
                }
                return _Headers;
            }
        }
        private WebHeaderCollection _ResponseHeaders = null;
        public WebHeaderCollection ResponseHeaders
        {
            get
            {
                if (wc != null)
                {
                    return wc.ResponseHeaders;
                }
                return _ResponseHeaders;
            }
            set
            {
                _ResponseHeaders = value;
            }
        }
        //临时用于预加载的链接建立
        public void DownloadDataAsync(Uri uri)
        {
            wc.DownloadDataTaskAsync(uri.AbsoluteUri);
        }
        public byte[] DownloadData(string url)
        {
            //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
            //sw.Start();
            //try
            //{
            if (wc != null)
            {
                //Random rnd = new Random();
                //if (rnd.Next() % 2 == 0)
                //{
                //    
                //}
                //return wc.DownloadDataTaskAsync(url).Result;
                return wc.DownloadData(url);
                //      
                //WebClient wc = new WebClient();
                //if (Headers.Count > 0)
                //{
                //    foreach (var item in Headers)
                //    {
                //        wc.Headers.Add(item.Key, item.Value);
                //    }
                //}
                //byte[] result = wc.DownloadData(url);
                //SetWebClientHeader(wc);
                //return result;
            }
            return ExeTask("GET", new Uri(url), null);
            //}
            //finally
            //{
            //    sw.Stop();
            //    if (sw.ElapsedMilliseconds > 1000)
            //    {
            //        Log.WriteLogToTxt("Proxy DownloadData: " + url + " " + sw.ElapsedMilliseconds, "DownloadData");
            //    }
            //    sw.Start();
            //}
        }
        public byte[] UploadData(string url, string method, byte[] data)
        {
            return UploadData(new Uri(url), method, data);

        }
        public byte[] UploadData(Uri address, string method, byte[] data)
        {
            if (wc != null)
            {
                //    return wc.UploadDataTaskAsync(address, method, data).Result;
                return wc.UploadData(address, method, data);
                //WebClient wc = new WebClient();
                //if (Headers.Count > 0)
                //{
                //    foreach (var item in Headers)
                //    {
                //        wc.Headers.Add(item.Key, item.Value);
                //    }
                //}
                //byte[] result = wc.UploadData(address, method, data);
                //SetWebClientHeader(wc);
                //return result;
            }
            return ExeTask(method, address, data);
        }

        public void Head(string url)
        {
            if (wc != null)
            {
                wc.Head(url);
            }
            else
            {
                ExeTask("HEAD", new Uri(url), null);
            }
        }

        private byte[] ExeTask(string method, Uri address, byte[] data)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), address.AbsoluteUri);
            if (Headers.Count > 0)
            {
                foreach (string item in Headers.Keys)
                {
                    request.Headers.Add(item, Headers[item]);
                }
            }
            if (data != null && data.Length > 0)
            {
                request.Content = new StreamContent(new MemoryStream(data)) { };
            }
            HttpClient httpClient = HttpClientPool.Create(address, Timeout);
            Task<HttpResponseMessage> task = httpClient.SendAsync(request);
            return GetHttpClientBytes(task);
        }


        private byte[] GetHttpClientBytes(Task<HttpResponseMessage> task)
        {
            if (task.Status != TaskStatus.RanToCompletion)
            {
                task.Wait(Timeout);
            }
            _ResponseStatusCode = (int)task.Result.StatusCode;
            if (ResponseHeaders != null)
            {
                ResponseHeaders.Clear();
            }
            else
            {
                ResponseHeaders = new WebHeaderCollection();
            }
            foreach (var item in task.Result.Headers)
            {
                string value = string.Empty;
                foreach (var v in item.Value)
                {
                    value = v;
                    break;
                }
                ResponseHeaders.Add(item.Key, value);
            }
            return task.Result.Content.ReadAsByteArrayAsync().Result;
        }

        //private void SetWebClientHeader(WebClient wc)
        //{
        //    if (ResponseHeaders != null)
        //    {
        //        ResponseHeaders.Clear();
        //    }
        //    else
        //    {
        //        ResponseHeaders = new Dictionary<string, string>();
        //    }
        //    try
        //    {
        //        foreach (string key in wc.ResponseHeaders.Keys)
        //        {
        //            ResponseHeaders.Add(key, wc.ResponseHeaders[key]);
        //        }
        //    }
        //    catch (Exception ex)
        //    {

        //    }
        //}
    }
}