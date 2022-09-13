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
        private bool isNet6 = Environment.Version.Major >= 6;
        public object Proxy { get; set; }

        private Dictionary<string, string> _Headers = new Dictionary<string, string>();
        public Dictionary<string, string> Headers
        {
            get
            {
                return _Headers;
            }
        }

        public Dictionary<string, string> ResponseHeaders { get; internal set; }

        public byte[] DownloadData(string url)
        {
            if (isNet6)
            {
                WebClient wc = new WebClient();
                if (Headers.Count > 0)
                {
                    foreach (var item in Headers)
                    {
                        wc.Headers.Add(item.Key, item.Value);
                    }
                }
                byte[] result = wc.DownloadData(url);
                SetWebClientHeader(wc);
                return result;
            }
            return ExeTask("GET", new Uri(url), null);
        }
        public byte[] UploadData(string url, string method, byte[] data)
        {
            return UploadData(new Uri(url), method, data);

        }
        public byte[] UploadData(Uri address, string method, byte[] data)
        {
            if (isNet6)
            {
                WebClient wc = new WebClient();
                if (Headers.Count > 0)
                {
                    foreach (var item in Headers)
                    {
                        wc.Headers.Add(item.Key, item.Value);
                    }
                }
                byte[] result = wc.UploadData(address, method, data);
                SetWebClientHeader(wc);
                return result;
            }
            return ExeTask(method, address, data);
        }

        private byte[] ExeTask(string method, Uri address, byte[] data)
        {
            HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(method), address.AbsoluteUri);
            if (Headers.Count > 0)
            {
                foreach (var item in Headers)
                {
                    request.Headers.Add(item.Key, item.Value);
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
                ResponseHeaders = new Dictionary<string, string>();
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

        private void SetWebClientHeader(WebClient wc)
        {
            if (ResponseHeaders != null)
            {
                ResponseHeaders.Clear();
            }
            else
            {
                ResponseHeaders = new Dictionary<string, string>();
            }
            try
            {
                foreach (string key in wc.ResponseHeaders.Keys)
                {
                    ResponseHeaders.Add(key, wc.ResponseHeaders[key]);
                }
            }
            catch (Exception ex)
            {

            }
        }
    }
}