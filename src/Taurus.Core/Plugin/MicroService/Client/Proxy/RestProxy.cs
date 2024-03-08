using CYQ.Data.Json;
using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Plugin.Rpc;

namespace Taurus.Plugin.MicroService.Proxy
{
    /// <summary>
    /// 由微服务代理产生的代码内部调用
    /// </summary>
    public static partial class RestProxy
    {
        //public class APIHelloPara : RestParaBase<APIHelloPara>
        //{
        //    public int ID { get; set; }
        //    public string Name { get; set; }
        //}
        //private static RpcTask Hello(APIHelloPara para)
        //{
        //    return RestProxyAsync<APIHelloPara>("", "", "GET", para);
        //}

        /// <summary>
        /// 由 RestProxy 内部调用。
        /// </summary>
        public static RpcTask CallAsync(string msName, string localPath, string httpMethod, RestParaBase restPara)
        {
            RpcTask rpcTask;
            try
            {
                if (string.IsNullOrEmpty(msName) || string.IsNullOrEmpty(localPath) || string.IsNullOrEmpty(httpMethod))
                {
                    rpcTask = new RpcTask();
                    rpcTask.Result = new RpcTaskResult() { Error = new Exception("para can't be empty.") };
                    return rpcTask;
                }

                string host = Gateway.GetHost(msName);
                if (string.IsNullOrEmpty(host))
                {
                    rpcTask = new RpcTask();
                    rpcTask.Result = new RpcTaskResult() { Error = new Exception("can't find the host by msName.") };
                    return rpcTask;
                }
                string url = host.TrimEnd('/') + localPath.ToLower();

                RpcTaskRequest request = new RpcTaskRequest();
                request.Url = url;
                request.HttpMethod = httpMethod;
                if (restPara != null)
                {
                    request.Headers = restPara.Headers;
                    switch (httpMethod)
                    {
                        case "GET":
                        case "HEAD":
                        case "DELETE":
                            request.Url += restPara.GetQueryString();
                            break;
                        case "POST":
                        case "PUT":
                            request.Data = restPara.GetBytes();
                            break;
                    }
                }
                return Rest.StartTaskAsync(request);
            }
            catch (Exception err)
            {
                rpcTask = new RpcTask();
                rpcTask.Result = new RpcTaskResult() { Error = err };
                return rpcTask;
            }
        }


    }
}
