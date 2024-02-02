using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Web;

namespace Taurus.Plugin.Rpc
{
    internal static partial class RpcTaskWorker
    {
        static RpcTaskWorker()
        {
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (ServicePointManager.DefaultConnectionLimit <= 1024)
            {
                ServicePointManager.DefaultConnectionLimit = 2048;//对.net framework有效。
            }
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | (SecurityProtocolType)12288;
        }

        public static RpcTaskResult ExecuteTask(RpcTaskRequest request)
        {
            if (HttpContext.Current != null && request.Headers["X-Request-ID"] == null)
            {
                //分布式请求追踪ID。
                request.Headers.Add("X-Request-ID", HttpContext.Current.GetTraceID());
            }
            HttpClient httpClient = HttpClientFactory.Get(request.Uri, request.Timeout);
            RpcTaskResult result = httpClient.Send(request);
            return result;
        }
    }
}
