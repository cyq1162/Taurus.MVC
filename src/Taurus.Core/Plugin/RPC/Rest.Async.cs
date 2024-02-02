using System.Collections.Generic;

namespace Taurus.Plugin.Rpc
{
    /// <summary>
    /// 发起 RpcTask 请求：REST API 调用方式
    /// </summary>
    public static partial class Rest
    {
        /// <summary>
        /// 异步：对远程服务发起一个 Get 请求。
        /// </summary>
        /// <param name="url">请求的完整【http或https】地址</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTask GetAsync(string url, Dictionary<string, string> header = null)
        {
            return CallAsync("GET", url, null, header);
        }

        /// <summary>
        /// 异步：对远程服务发起一个 Post 请求。
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
        /// 异步：对远程服务发起一个 Put 请求。
        /// </summary>
        /// <param name="url">请求的完整【http或https】地址</param>
        /// <param name="data">请求数据</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTask PutAsync(string url, byte[] data, Dictionary<string, string> header = null)
        {
            return CallAsync("PUT", url, data, header);
        }


        /// <summary>
        /// 异步：对远程服务发起一个 Head 请求。
        /// </summary>
        /// <param name="url">请求的完整【http或https】地址</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTask HeadAsync(string url, Dictionary<string, string> header = null)
        {
            return CallAsync("HEAD", url, null, header);
        }


        /// <summary>
        /// 异步：对远程服务发起一个 Delete 请求。
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
            request.HttpMethod = httpMethod;
            request.Url = url;
            request.Data = data;
            if (header != null)
            {
                foreach (var item in header)
                {
                    request.Headers.Add(item.Key, item.Value);
                }
            }
            return StartTaskAsync(request);
        }
        /// <summary>
        /// 异步：发起一个【通用】请求
        /// </summary>
        /// <param name="request">请求信息</param>
        public static RpcTask StartTaskAsync(RpcTaskRequest request)
        {
           return RpcTaskWorker.ExecuteTaskAsync(request);
        }
    }
}
