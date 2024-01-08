using System;
using System.Data;

namespace Taurus.Plugin.DistributedTransaction
{
    /// <summary>
    /// 用于分布式事务客户端【即调用端】回调订阅。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DTCClientCallBackAttribute : Attribute
    {
        /// <summary>
        /// 获取设置的监听名称。
        /// </summary>
        public string CallBackKey { get; set; }


        /// <summary>
        /// 用于分布式事务回调订阅。
        /// </summary>
        /// <param name="callBackKey">监听的名称</param>
        public DTCClientCallBackAttribute(string callBackKey)
        {
            this.CallBackKey = callBackKey;
        }
    }
}
