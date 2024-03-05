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
            var ct = string.Empty;
            var cl = string.Empty;
            if (Headers.Count > 0)
            {
                foreach (string item in Headers.Keys)
                {
                    switch (item.ToLower())
                    {
                        case "content-type":
                            ct = Headers[item];
                            continue;
                        case "content-length":
                            cl = Headers[item];
                            continue;
                        default:
                            request.Headers.Add(item, Headers[item]);
                            break;
                    }
                }
            }
            if (Data != null && Data.Length > 0)
            {
                request.Content = new StreamContent(new MemoryStream(Data)) { };
                if (!string.IsNullOrEmpty(ct))
                {
                    request.Content.Headers.TryAddWithoutValidation("Content-Type", ct);
                }
                if (!string.IsNullOrEmpty(cl))
                {
                    request.Content.Headers.TryAddWithoutValidation("Content-Length", ct);
                }
            }
            return request;
        }
    }
}
