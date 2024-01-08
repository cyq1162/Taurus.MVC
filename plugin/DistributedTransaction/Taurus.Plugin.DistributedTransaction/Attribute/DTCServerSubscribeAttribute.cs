using System;
using System.Data;

namespace Taurus.Plugin.DistributedTransaction
{
    /// <summary>
    /// 用于分布式事务服务端【即提供端】回调订阅，SubKey 区分大小写。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class DTCServerSubscribeAttribute : Attribute
    {
        /// <summary>
        /// 获取设置的监听名称。
        /// </summary>
        public string SubKey { get; set; }

        /// <summary>
        /// 用于分布式事务回调订阅。
        /// </summary>
        /// <param name="subKey">监听的名称</param>
        public DTCServerSubscribeAttribute(string subKey)
        {
            this.SubKey = subKey;
        }

    }
}
