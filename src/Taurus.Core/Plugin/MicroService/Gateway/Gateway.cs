using System;
using System.Web;
using System.Collections.Generic;
using Taurus.Mvc;
using CYQ.Data;
using System.Threading;
using CYQ.Data.Tool;
using System.Net;
using Taurus.Plugin.Rpc;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 微服务的核心类：网关代理（请求转发）
    /// </summary>
    public static partial class Gateway
    {
        /// <summary>
        /// 根据微服务注册名称获取请求的主机地址【有多个时，由内部控制负载均衡，每次获取都会循环下一个】
        /// 【自动识别（先判断：是否客户端；再判断：是否服务端）】
        /// </summary>
        /// <param name="name">微服务注册名称</param>
        /// <returns></returns>
        public static string GetHost(string name)
        {
            if (MsConfig.IsClient)
            {
                return Client.GetHost(name);
            }
            if (MsConfig.IsServer)
            {
                return Server.GetHost(name);
            }
            return string.Empty;
        }
        /// <summary>
        /// 根据微服务注册名称获取请求的主机地址【有多个时，由内部控制负载均衡，每次获取都会循环下一个】
        /// </summary>
        /// <param name="name">微服务注册名称</param>
        /// <param name="isClient">指定查询：true（客户端）：false（服务端）</param>
        /// <returns></returns>
        public static string GetHost(string name, bool isClient)
        {
            if (isClient && MsConfig.IsClient)
            {
                return Client.GetHost(name);
            }
            else if (!isClient && MsConfig.IsServer)
            {
                return Server.GetHost(name);
            }
            return string.Empty;
        }
    }

}
