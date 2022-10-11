using System.Net.Http;
using System.Collections.Generic;
using System.Net;
using System.Net.Http.Headers;
using System;
using System.IO;
using System.Threading.Tasks;

namespace Taurus.MicroService
{
    internal class RpcClient
    {
        private static bool isNet6 = Environment.Version.Major >= 6;
        WebClient wc;
        public RpcClient()
        {
            if (isNet6)
            {
                wc = new WebClient();
               // wc.Proxy = null;
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

        public byte[] DownloadData(string url)
        {
            if (wc != null)
            {
                return wc.DownloadData(url);
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
        }
        public byte[] UploadData(string url, string method, byte[] data)
        {
            return UploadData(new Uri(url), method, data);

        }
        public byte[] UploadData(Uri address, string method, byte[] data)
        {
            if (wc != null)
            {
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
            if (data != null)
            {
                request.Content = new StreamContent(new MemoryStream(data)) { }; ;
            }
            HttpClient httpClient = HttpClientPool.Create(address);
            Task<HttpResponseMessage> task = httpClient.SendAsync(request);
            return GetHttpClientBytes(task);
        }


        private byte[] GetHttpClientBytes(Task<HttpResponseMessage> task)
        {
            if (task.Status != TaskStatus.RanToCompletion)
            {
                task.Wait(30000);
            }
            if (ResponseHeaders != null)
            {
                ResponseHeaders.Clear();
            }
            else
            {
                ResponseHeaders =new WebHeaderCollection();
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