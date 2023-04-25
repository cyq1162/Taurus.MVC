using System;
using System.Net;
using System.Threading;
using CYQ.Data.Tool;
using Taurus.Mvc;

namespace Taurus.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {
        private static bool isStart = false;
        internal static void Start(Uri uri)
        {
            if (!isStart)
            {
                string urlAbs = uri.AbsoluteUri;
                string urlPath = uri.PathAndQuery;
                string host = urlAbs.Substring(0, urlAbs.Length - urlPath.Length);
                Start(host);
            }
        }
        /// <summary>
        /// 微服务客户端启动
        /// </summary>
        internal static void Start(string host)
        {
            if (string.IsNullOrEmpty(MvcConfig.RunUrl))
            {
                MvcConfig.RunUrl = host.ToLower().TrimEnd('/');//设置当前程序运行的请求网址。
            }
            if (!isStart)
            {
                MsLog.WriteDebugLine("--------------------------------------------------");
                MsLog.WriteDebugLine("Current App Process ID    ：" + MvcConst.ProcessID);
                MsLog.WriteDebugLine("Current Taurus Version    ：" + MvcConst.Version);
                isStart = true;
                if (MsConfig.IsServer)
                {
                    ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
                    if (ServicePointManager.DefaultConnectionLimit == 2)
                    {
                        ServicePointManager.DefaultConnectionLimit = 2048;//对.net framework有效。
                        ThreadPool.SetMinThreads(30, 50);
                    }
                }
                if (MsConfig.IsRegCenterOfMaster)
                {
                    MsLog.WriteDebugLine("Current MicroService Type ：RegCenter of Master");
                    Thread thread = new Thread(new ThreadStart(ClearExpireHost));
                    thread.Start();
                }
                if (!string.IsNullOrEmpty(MsConfig.Server.Name) && !string.IsNullOrEmpty(MsConfig.Server.RcUrl) && MsConfig.Server.RcUrl != MvcConfig.RunUrl)
                {
                    if (MsConfig.IsRegCenter || MsConfig.IsGateway)
                    {
                        if (MsConfig.IsRegCenter)
                        {
                            MsLog.WriteDebugLine("Current MicroService Type ：RegCenter of Slave");
                        }
                        else
                        {
                            MsLog.WriteDebugLine("Current MicroService Type ：Gateway");
                        }
                        MsLog.WriteDebugLine("Current RegisterCenter Url：" + MsConfig.Server.RcUrl);
                        ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(RunLoopOfServer));
                    }
                }
                if (!string.IsNullOrEmpty(MsConfig.Client.Name) && !string.IsNullOrEmpty(MsConfig.Client.RcUrl) && MsConfig.Client.RcUrl != MvcConfig.RunUrl)
                {
                    MsLog.WriteDebugLine("Current MicroService Type ：Client of 【" + MsConfig.Client.Name + "】");
                    MsLog.WriteDebugLine("Current RegisterCenter Url：" + MsConfig.Client.RcUrl);
                    ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(RunLoopOfClient));
                }
                MsLog.WriteDebugLine("--------------------------------------------------");
            }
        }
    }

}
