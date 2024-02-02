using System.Collections.Generic;
using System.Net.Http;
using System.Web;

namespace Taurus.Plugin.Rpc
{
    /// <summary>
    /// 用于客户端：REST API 调用方式
    /// </summary>
    public static partial class Rest
    {

        /// <summary>
        /// 对远程服务发起一个同步Get请求。
        /// </summary>
        /// <param name="url">请求的完整【http或https】地址</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTaskResult Get(string url, Dictionary<string, string> header = null)
        {
            return Call("GET", url, null, header);
        }

        /// <summary>
        /// 对远程服务发起一个同步Post请求。
        /// </summary>
        /// <param name="url">请求的完整【http或https】地址</param>
        /// <param name="data">post的数据</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTaskResult Post(string url, byte[] data, Dictionary<string, string> header = null)
        {
            return Call("POST", url, data, header);
        }


        /// <summary>
        /// 对远程服务发起一个同步Put请求。
        /// </summary>
        /// <param name="url">请求的完整【http或https】地址</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTaskResult Put(string url, byte[] data, Dictionary<string, string> header = null)
        {
            return Call("PUT", url, data, header);
        }


        /// <summary>
        /// 对远程服务发起一个同步Head请求。
        /// </summary>
        /// <param name="url">请求的完整【http或https】地址</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTaskResult Head(string url, Dictionary<string, string> header = null)
        {
            return Call("HEAD", url, null, header);
        }


        /// <summary>
        /// 对远程服务发起一个同步Delete请求。
        /// </summary>
        /// <param name="url">请求的完整【http或https】地址</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTaskResult Delete(string url, Dictionary<string, string> header = null)
        {
            return Call("DELETE", url, null, header);
        }

        private static RpcTaskResult Call(string httpMethod, string url, byte[] data, Dictionary<string, string> header = null)
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
            return StartTask(request);
        }
        /// <summary>
        /// 同步：发起一个通用请求
        /// </summary>
        /// <param name="request">请求信息</param>
        public static RpcTaskResult StartTask(RpcTaskRequest request)
        {
           return RpcTaskWorker.ExecuteTask(request);
        }
    }

}
