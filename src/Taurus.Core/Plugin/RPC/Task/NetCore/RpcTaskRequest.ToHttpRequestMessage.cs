using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Text;

namespace Taurus.Plugin.Rpc
{
    /// <summary>
    /// 用于Rpc 任务 发起的请求
    /// </summary>
    public partial class RpcTaskRequest
    {
        /// <summary>
        /// Rpc 请求 转 HttpClient 请求
        /// </summary>
        /// <returns></returns>
        public HttpRequestMessage ToHttpRequestMessage()
        {
            HttpMethod method = new HttpMethod(HttpMethod);
            HttpRequestMessage request = new HttpRequestMessage(method, Uri.AbsoluteUri);
            if (Headers.Count > 0)
            {
                foreach (string item in Headers.Keys)
                {
                    request.Headers.Add(item, Headers[item]);
                }
            }
            if (Data != null && Data.Length > 0)
            {
                request.Content = new StreamContent(new MemoryStream(Data)) { };
            }
            return request;
        }
    }
}
