using CYQ.Data;
using System;
using System.Net;
using Taurus.Plugin.Rpc;

namespace System.Net.Http
{

    internal class HttpClient
    {
        HttpClientHandler config;
        public HttpClient(HttpClientHandler handler)
        {
            this.config = handler;
        }
        private TimeSpan _Timeout;
        /// <summary>
        /// 超时时间
        /// </summary>
        public TimeSpan Timeout
        {
            get
            {
                if (_Timeout.TotalSeconds == 0)
                {
                    _Timeout = TimeSpan.FromSeconds(AppConfig.IsDebugMode ? 120 : 10);
                }
                return _Timeout;
            }

            set { _Timeout = value; }
        }
        public RpcTaskResult Send(RpcTaskRequest request)
        {
            RpcTaskResult result = new RpcTaskResult();
            string url = request.Url.ToString();
            try
            {
                HttpWebClient httpClient = new HttpWebClient();
                httpClient.Timeout = this.Timeout;
                httpClient.Headers = request.Headers;
                string method = request.HttpMethod.ToUpper();
                switch (method)
                {
                    case "GET":
                        result.ResultByte = httpClient.DownloadData(url);
                        break;
                    case "HEAD":
                        httpClient.Head(url);
                        break;
                    default:
                        byte[] data = request.Data;
                        if (data == null)
                        {
                            data = new byte[0];
                        }
                        result.ResultByte = httpClient.UploadData(url, method, data);
                        break;
                }
                result.IsSuccess = true;
                result.Headers = httpClient.ResponseHeaders;
            }
            catch (Exception err)
            {
                result.IsSuccess = false;
                result.Error = err;
            }

            return result;
        }
    }
}
