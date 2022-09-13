using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;
using Taurus.Mvc;
using System.Web;
namespace Taurus.MicroService
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
            /// <param name="name">远程的注册模块名</param>
            /// <param name="pathAndQuery">请求路径和参数</param>
            /// <param name="header">请求头</param>
            /// <returns></returns>
            public static RpcTask GetAsync(string name, string pathAndQuery, Dictionary<string, string> header = null)
            {
                string host = GetHost(name);
                if (string.IsNullOrEmpty(host))
                {
                    return new RpcTask() { Result = new RpcTaskResult() { IsSuccess = false, ErrorText = "Can't find the host by " + name } };
                }

                return CallAsync("GET", host + pathAndQuery, null, header);
            }

            /// <summary>
            /// 对远程服务发起一个异步Get请求。
            /// </summary>
            /// <param name="url">请求的地址</param>
            /// <param name="header">可追加的请求头部分</param>
            /// <returns></returns>
            public static RpcTask GetAsync(string url, Dictionary<string, string> header = null)
            {
                return CallAsync("GET", url, null, header);
            }
            public static RpcTask PostAsync(string name, string pathAndQuery, byte[] data, Dictionary<string, string> header = null)
            {
                string host = GetHost(name);
                if (string.IsNullOrEmpty(host))
                {
                    return new RpcTask() { Result = new RpcTaskResult() { IsSuccess = false, ErrorText = "Can't find the host by " + name } };
                }
                return CallAsync("POST", host + pathAndQuery, data, header);
            }

            /// <summary>
            /// 对远程服务发起一个异步Post请求。
            /// </summary>
            /// <param name="url">请求的地址</param>
            /// <param name="data">post的数据</param>
            /// <param name="header">可追加的请求头部分</param>
            /// <returns></returns>
            public static RpcTask PostAsync(string url, byte[] data, Dictionary<string, string> header = null)
            {
                return CallAsync("POST", url, data, header);
            }
            //public static RpcTask PostAsync(string url, Dictionary<string, string> data, Dictionary<string, string> header = null)
            //{
            //    return CallAsync("POST", url, data, header);
            //}
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
                RpcTaskWorker.ExeTaskAsync(rpcTask);
                return rpcTask;
            }

        }
    }
}
