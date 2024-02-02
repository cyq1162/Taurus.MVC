using System.Net.Http;
using System.Web;
using System.IO;
using System;
using CYQ.Data;

namespace Taurus.Plugin.Rpc
{
    /// <summary>
    /// Net Core 版本
    /// </summary>
    internal partial class RpcTaskWorker
    {

        public static RpcTask ExecuteTaskAsync(RpcTaskRequest request)
        {
            RpcTask task = new RpcTask();

            task.Request = request;
            if (request == null || request.Uri == null)
            {
                task.State = RpcTaskState.Complete;
                task.Result = new RpcTaskResult() { ErrorText = "Uri is null." };
                return task;
            }

            try
            {
                task.State = RpcTaskState.Running;
                HttpRequestMessage httpRequest = task.Request.ToHttpRequestMessage();
                if (HttpContext.Current != null && !httpRequest.Headers.Contains("X-Request-ID"))
                {
                    //分布式请求追踪ID。
                    httpRequest.Headers.Add("X-Request-ID", HttpContext.Current.GetTraceID());
                }
                HttpClient httpClient = HttpClientFactory.Get(task.Request.Uri, task.Request.Timeout);
                task.task = httpClient.SendAsync(httpRequest);
            }
            catch (Exception err)
            {
                task.State = RpcTaskState.Complete;
                task.Result = new RpcTaskResult() { Error = err };
                Log.Write(err);
            }
            return task;
        }
    }
}