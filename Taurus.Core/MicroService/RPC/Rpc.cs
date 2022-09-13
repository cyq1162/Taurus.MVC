﻿
using System.Collections.Generic;
using System.Net;
using System.Threading;

namespace Taurus.MicroService
{
    /// <summary>
    /// 一般用于客户端：服务间的RPC调用
    /// </summary>
    public static partial class Rpc
    {
        static Rpc()
        {
            if (ServicePointManager.DefaultConnectionLimit == 2)
            {
                ServicePointManager.DefaultConnectionLimit = 1024;//对.net framework有效。
                ThreadPool.SetMinThreads(10, 10);
            }
        }
        /// <summary>
        /// 根据微服务注册名称获取请求的主机地址【有多个时，由内部控制负载均衡，每次获取都会循环下一个】
        /// 【自动识别（先判断：是否客户端；再判断：是否服务端）】
        /// </summary>
        /// <param name="name">微服务注册名称</param>
        /// <returns></returns>
        public static string GetHost(string name)
        {
            if (MSConfig.IsClient)
            {
                return Client.GetHost(name);
            }
            if (MSConfig.IsServer)
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
            if (isClient && MSConfig.IsClient)
            {
                return Client.GetHost(name);
            }
            else if (!isClient && MSConfig.IsServer)
            {
                return Server.GetHost(name);
            }
            return string.Empty;
        }

        /// <summary>
        /// 执行一个异步的【通用】请求任务。
        /// </summary>
        /// <param name="request">任务请求</param>
        /// <returns></returns>
        public static RpcTask StartTaskAsync(RpcTaskRequest request)
        {
            return Rest.StartTaskAsync(request);
        }

        /// <summary>
        /// 对远程服务发起一个异步Get请求。
        /// </summary>
        /// <param name="name">远程的注册模块名</param>
        /// <param name="pathAndQuery">请求路径和参数</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public static RpcTask StartGetAsync(string name, string pathAndQuery, Dictionary<string, string> header = null)
        {
            return Rest.GetAsync(name, pathAndQuery, header);
        }

        /// <summary>
        /// 对远程服务发起一个异步Get请求。
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTask StartGetAsync(string url, Dictionary<string, string> header = null)
        {
            return Rest.GetAsync(url, header);
        }
        /// <summary>
        ///  对远程服务发起一个异步Post请求。
        /// </summary>
        /// <param name="name">微服务名称</param>
        /// <param name="pathAndQuery">请求路径</param>
        /// <param name="data">请求数据</param>
        /// <param name="header">请求头</param>
        /// <returns></returns>
        public static RpcTask StartPostAsync(string name, string pathAndQuery, byte[] data, Dictionary<string, string> header = null)
        {
            return Rest.PostAsync(name, pathAndQuery, data, header);
        }
        /// <summary>
        /// 对远程服务发起一个异步Post请求。
        /// </summary>
        /// <param name="url">请求的地址</param>
        /// <param name="data">post的数据</param>
        /// <param name="header">可追加的请求头部分</param>
        /// <returns></returns>
        public static RpcTask StartPostAsync(string url, byte[] data, Dictionary<string, string> header = null)
        {
            return Rest.PostAsync(url, data, header);
        }
    }
}