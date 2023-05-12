using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.MicroService
{
    /// <summary>
    /// 用于Rpc 任务 发起的请求
    /// </summary>
    public class RpcTaskRequest
    {
        /// <summary>
        /// Http Method 【Get、Post、Put、Head、Delete】
        /// </summary>
        public string Method { get; set; }
        /// <summary>
        /// 请求的Url（http或https）。【PS：主机地址部分可由：Rpc.GetHost(name)根据微服务注册模块名称获取】
        /// </summary>
        public string Url { get; set; }
        internal Uri Uri
        {
            get
            {
                if (!string.IsNullOrEmpty(Url) && Url.StartsWith("http"))
                {
                    return new Uri(Url);
                }
                return null;
            }
        }
        /// <summary>
        /// 当前请求的请求头
        /// </summary>
        public Dictionary<string, string> Header { get; set; }
        /// <summary>
        /// 当前请求的数据
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 超时：单位毫秒（ms）。
        /// </summary>
        public int Timeout { get; set; }
    }
}
