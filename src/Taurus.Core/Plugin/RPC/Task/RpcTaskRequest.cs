using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace Taurus.Plugin.Rpc
{
    /// <summary>
    /// 用于Rpc 任务 发起的请求
    /// </summary>
    public partial class RpcTaskRequest
    {
        private string _HttpMethod = "GET";
        /// <summary>
        /// Http Method 【GET、POST、PUT、HEAD、DELETE】
        /// </summary>
        public string HttpMethod { get { return _HttpMethod; } set { _HttpMethod = value; } }
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
                throw new Exception("Url must be start with http or https.");
            }
        }
        private WebHeaderCollection _Headers = new WebHeaderCollection();

        /// <summary>
        /// 当前请求的请求头
        /// </summary>
        public WebHeaderCollection Headers { get { return _Headers; } set { _Headers = value; } }
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
