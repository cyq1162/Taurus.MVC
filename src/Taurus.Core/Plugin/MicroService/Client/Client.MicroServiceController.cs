using CYQ.Data;
using System.IO;
using System;
using System.Threading;
using Taurus.Mvc;
using Taurus.Mvc.Attr;
using Taurus.Plugin.MicroService.Proxy;
using CYQ.Data.Tool;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 微服务 - 客户端。
    /// </summary>
    internal partial class MicroServiceController : Controller
    {
        /// <summary>
        /// 停止微服务
        /// </summary>
        [MicroService]
        public void Stop()
        {
            if (MsConfig.IsClient && MsConfig.Client.IsAllowRemoteExit)
            {
                MsConfig.Client.IsEnable = false;
                Write("Remote stop success: microservice has stopped.", true);
            }
            else
            {
                Write("Remote stop fail : microservice remote exit is disabled.", false);
            }
        }
        /// <summary>
        /// 终止微服务程序，并退出应用程序
        /// </summary>
        [MicroService]
        public void Exit()
        {
            if (MsConfig.Client.IsEnable && MsConfig.IsClient && MsConfig.Client.IsAllowRemoteExit)
            {
                MsConfig.Client.IsExitApplication = true;//注销注册中心服务。
                new Thread(new ThreadStart(AppExit)).Start();
                Write("Remote exit success: wait for the registry center to remove the host (15s).", true);
            }
            else
            {
                Write("Remote exit fail : microservice remote exit is disabled.", false);
            }
        }

        private void AppExit()
        {
            MsLog.WriteDebugLine("Remote Exit : wait for the registry center to remove the host (15s).");
            for (int i = 15; i >= 0; i--)
            {
                MsLog.WriteDebugLine("Ready to stop : " + i + "s");
                Thread.Sleep(1000);
            }
            Environment.Exit(0);
        }

        /// <summary>
        /// 下载 RpcProxy
        /// </summary>
        [MicroService]
        public void DownRpcProxy(string name)
        {
            if (MsConfig.IsClient)
            {
                string folder = MvcConst.AppDataPath + "/microservice/";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                if (string.IsNullOrEmpty(name)) { name = "RpcProxy"; }
                else
                {
                    name = Path.GetFileNameWithoutExtension(name);
                }

                string savePath = folder + name + ".dll";
                if (RestProxyCreator.BuildAssembly(name, savePath))
                {
                    WriteFile(savePath);
                    IOHelper.Delete(savePath);
                    return;
                }

            }
        }
        /// <summary>
        /// 查看 RpcProxy 代码
        /// </summary>
        /// <param name="name">程序集名称</param>
        [MicroService]
        public void ViewRpcProxy(string name)
        {
            if (MsConfig.IsClient)
            {
                string code = RestProxyCoder.CreateCode(name);
                Write(code);
            }
        }
    }
}
