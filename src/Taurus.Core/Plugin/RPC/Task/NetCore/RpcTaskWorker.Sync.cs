using Microsoft.AspNetCore.Http.Headers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Reflection.PortableExecutable;
using System.Threading.Tasks;
using System.Threading;
using System.Web;

namespace Taurus.Plugin.Rpc
{
    /// <summary>
    /// 执行同步方法
    /// </summary>
    internal static partial class RpcTaskWorker
    {
        static RpcTaskWorker()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Tls11 | SecurityProtocolType.Tls12 | (SecurityProtocolType)12288;
        }
        public static RpcTaskResult ExecuteTask(RpcTaskRequest rpcRequest)
        {
            HttpRequestMessage request = rpcRequest.ToHttpRequestMessage();
            if (HttpContext.Current != null && !request.Headers.Contains("X-Request-ID"))
            {
                //分布式请求追踪ID。
                request.Headers.Add("X-Request-ID", HttpContext.Current.GetTraceID());
            }
            return SendAsync(request, rpcRequest.Timeout).GetAwaiter().GetResult();
        }

        private static async Task<RpcTaskResult> SendAsync(HttpRequestMessage request, int timeout)
        {
            try
            {
                HttpClient httpClient = HttpClientFactory.Get(request.RequestUri, timeout);
                return await GetRpcTaskResult(httpClient.SendAsync(request));
            }
            catch (Exception err)
            {
                RpcTaskResult result = new RpcTaskResult();
                result.Error = err;
                result.IsSuccess = false;
                return result;
            }
        }

        /// <summary>
        /// 获取异步请求结果
        /// </summary>
        /// <param name="task">异步任务</param>
        /// <returns></returns>
        internal static async Task<RpcTaskResult> GetRpcTaskResult(Task<HttpResponseMessage> task)
        {
            HttpResponseMessage responseMessage = await task;
            return await GetRpcTaskResult(responseMessage);
        }


        private static async Task<RpcTaskResult> GetRpcTaskResult(HttpResponseMessage responseMessage)
        {
            RpcTaskResult result = new RpcTaskResult();
            result.IsSuccess = true;
            result.StatusCode = (int)responseMessage.StatusCode;
            foreach (var item in responseMessage.Headers)
            {
                string value = string.Join(" ", item.Value);
                //string value = string.Empty;
                //foreach (var v in item.Value)
                //{
                //    value = v;
                //    break;
                //}
                result.Headers.Add(item.Key, value);
            }
            byte[] bytes = await responseMessage.Content.ReadAsByteArrayAsync();
            if (bytes != null && bytes.Length > 0)
            {
                result.ResultByte = bytes;
            }
            return result;
        }
    }

}
