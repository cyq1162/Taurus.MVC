﻿using System.Collections.Generic;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Rpc;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 登陆退出
    /// </summary>
    internal partial class AdminController
    {
        /// <summary>
        /// 代理 停止 客户端 微服务
        /// </summary>
        public void StopClientMicroService(string host)
        {
            string url = host + MsConfig.Server.RcPath + "/stop?mskey=" + MsConfig.Server.RcKey;
            Gateway.Proxy(Context, url);
            //RpcTask task = Rest.GetAsync(url);
            //Write(task.Result.Text);
        }

        /// <summary>
        /// 代理 退出 客户端 应用程序
        /// </summary>
        public void ExitClientAppliction(string host)
        {
            string url = host + MsConfig.Server.RcPath + "/exit?mskey=" + MsConfig.Server.RcKey;
            Gateway.Proxy(Context, url);
            //RpcTask task = Rest.GetAsync(url);
            //Write(task.Result.Text);
        }

        /// <summary>
        /// 重新检测Url状态
        /// </summary>
        /// <param name="host">主机</param>
        /// <param name="state">当前状态</param>
        public void CheckUrl(string host, int state)
        {
            var hostList = HostList;
            HostInfo hostInfo = null;
            foreach (var item in hostList)
            {
                string lowerKey = item.Key.ToLower();
                if (lowerKey == MsConst.RegistryCenter || lowerKey == MsConst.RegistryCenterOfSlave || lowerKey == MsConst.Gateway || lowerKey.Contains("."))
                {
                    continue;
                }
                if (hostInfo != null) { break; }
                foreach (var info in item.Value)
                {
                    if (info.State == state && info.Host == host)
                    {
                        hostInfo = info;
                        break;
                    }
                }
            }
            if (hostInfo != null)
            {
                Gateway.PreConnection(hostInfo);
                Write("Host address detection has been successfully initiated.", true);
            }
            else
            {
                Write("Unable to find information for this host", false);
            }
        }

        public void DownRpcProxy(string host, string name)
        {
            string url = host + MsConfig.Server.RcPath + "/downrpcproxy?mskey=" + MsConfig.Server.RcKey;
            Gateway.Proxy(Context, url);
        }
        public void ViewRpcProxy(string host, string name)
        {
            string url = host + MsConfig.Server.RcPath + "/viewrpcproxy?mskey=" + MsConfig.Server.RcKey;
            Gateway.Proxy(Context, url);
        }
    }
}
