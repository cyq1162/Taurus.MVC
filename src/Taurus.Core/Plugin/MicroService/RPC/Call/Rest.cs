using System.Collections.Generic;

namespace Taurus.Plugin.MicroService
{
    public static partial class Rpc
    {
        /// <summary>
        /// 用于客户端：服务间的RPC调用（REST API 调用方式）
        /// </summary>
        internal static partial class Rest
        {
            /// <summary>
            /// 对远程服务发起一个异步Get请求。
            /// </summary>
            /// <param name="msName">远程的微服务注册模块名</param>
            /// <param name="pathAndQuery">请求相对路径和参数</param>
            /// <param name="header">请求头</param>
            /// <returns></returns>
            public static RpcTask GetAsync(string msName, string pathAndQuery, Dictionary<string, string> header = null)
            {
                string host = GetHost(msName);
                if (string.IsNullOrEmpty(host))
                {
                    return new RpcTask() { Result = new RpcTaskResult() { IsSuccess = false, ErrorText = "Can't find the host by microservice name : " + msName } };
                }

                return CallAsync("GET", host + pathAndQuery, null, header);
            }

            /// <summary>
            /// 对远程服务发起一个异步Get请求。
            /// </summary>
            /// <param name="url">请求的完整【http或https】地址</param>
            /// <param name="header">可追加的请求头部分</param>
            /// <returns></returns>
            public static RpcTask GetAsync(string url, Dictionary<string, string> header = null)
            {
                return CallAsync("GET", url, null, header);
            }
            /// <summary>
            /// 对远程服务发起一个异步Post请求。
            /// </summary>
            /// <param name="msName">远程的微服务注册模块名</param>
            /// <param name="pathAndQuery">请求相对路径和参数</param>
            /// <param name="data">请求数据</param>
            /// <param name="header">请求头</param>
            /// <returns></returns>
            public static RpcTask PostAsync(string msName, string pathAndQuery, byte[] data, Dictionary<string, string> header = null)
            {
                string host = GetHost(msName);
                if (string.IsNullOrEmpty(host))
                {
                    return new RpcTask() { Result = new RpcTaskResult() { IsSuccess = false, ErrorText = "Can't find the host by microservice name : " + msName } };
                }
                return CallAsync("POST", host + pathAndQuery, data, header);
            }

            /// <summary>
            /// 对远程服务发起一个异步Post请求。
            /// </summary>
            /// <param name="url">请求的完整【http或https】地址</param>
            /// <param name="data">post的数据</param>
            /// <param name="header">可追加的请求头部分</param>
            /// <returns></returns>
            public static RpcTask PostAsync(string url, byte[] data, Dictionary<string, string> header = null)
            {
                return CallAsync("POST", url, data, header);
            }

            /// <summary>
            /// 对远程服务发起一个异步Put请求。
            /// </summary>
            /// <param name="msName">远程的微服务注册模块名</param>
            /// <param name="pathAndQuery">请求相对路径和参数</param>
            /// <param name="header">请求头</param>
            /// <returns></returns>
            public static RpcTask PutAsync(string msName, string pathAndQuery, byte[] data, Dictionary<string, string> header = null)
            {
                string host = GetHost(msName);
                if (string.IsNullOrEmpty(host))
                {
                    return new RpcTask() { Result = new RpcTaskResult() { IsSuccess = false, ErrorText = "Can't find the host by microservice name : " + msName } };
                }

                return CallAsync("PUT", host + pathAndQuery, data, header);
            }

            /// <summary>
            /// 对远程服务发起一个异步Put请求。
            /// </summary>
            /// <param name="url">请求的完整【http或https】地址</param>
            /// <param name="header">可追加的请求头部分</param>
            /// <returns></returns>
            public static RpcTask PutAsync(string url, byte[] data, Dictionary<string, string> header = null)
            {
                return CallAsync("PUT", url, data, header);
            }

            /// <summary>
            /// 对远程服务发起一个异步Head请求。
            /// </summary>
            /// <param name="msName">远程的微服务注册模块名</param>
            /// <param name="pathAndQuery">请求相对路径和参数</param>
            /// <param name="header">请求头</param>
            /// <returns></returns>
            public static RpcTask HeadAsync(string msName, string pathAndQuery, Dictionary<string, string> header = null)
            {
                string host = GetHost(msName);
                if (string.IsNullOrEmpty(host))
                {
                    return new RpcTask() { Result = new RpcTaskResult() { IsSuccess = false, ErrorText = "Can't find the host by microservice name : " + msName } };
                }

                return CallAsync("HEAD", host + pathAndQuery, null, header);
            }

            /// <summary>
            /// 对远程服务发起一个异步Head请求。
            /// </summary>
            /// <param name="url">请求的完整【http或https】地址</param>
            /// <param name="header">可追加的请求头部分</param>
            /// <returns></returns>
            public static RpcTask HeadAsync(string url, Dictionary<string, string> header = null)
            {
                return CallAsync("HEAD", url, null, header);
            }



            /// <summary>
            /// 对远程服务发起一个异步Delete请求。
            /// </summary>
            /// <param name="msName">远程的微服务注册模块名</param>
            /// <param name="pathAndQuery">请求相对路径和参数</param>
            /// <param name="header">请求头</param>
            /// <returns></returns>
            public static RpcTask DeleteAsync(string msName, string pathAndQuery, Dictionary<string, string> header = null)
            {
                string host = GetHost(msName);
                if (string.IsNullOrEmpty(host))
                {
                    return new RpcTask() { Result = new RpcTaskResult() { IsSuccess = false, ErrorText = "Can't find the host by microservice name : " + msName } };
                }

                return CallAsync("DELETE", host + pathAndQuery, null, header);
            }

            /// <summary>
            /// 对远程服务发起一个异步Delete请求。
            /// </summary>
            /// <param name="url">请求的完整【http或https】地址</param>
            /// <param name="header">可追加的请求头部分</param>
            /// <returns></returns>
            public static RpcTask DeleteAsync(string url, Dictionary<string, string> header = null)
            {
                return CallAsync("DELETE", url, null, header);
            }

            private static RpcTask CallAsync(string httpMethod, string url, byte[] data, Dictionary<string, string> header = null)
            {
                RpcTaskRequest request = new RpcTaskRequest();
                request.Method = httpMethod;
                request.Url = url;
                request.Data = data;
                request.Header = header;
                return StartTaskAsync(request);
            }
            public static RpcTask StartTaskAsync(RpcTaskRequest request)
            {
                RpcTask rpcTask = new RpcTask();
                rpcTask.Request = request;
                if (request == null || request.Uri == null)
                {
                    rpcTask.State = RpcTaskState.Complete;
                    rpcTask.Result = new RpcTaskResult() { ErrorText = "Uri is null." };
                }
                else
                {
                    RpcTaskWorker.ExeTaskAsync(rpcTask);
                }
                return rpcTask;
            }

        }
    }
}
