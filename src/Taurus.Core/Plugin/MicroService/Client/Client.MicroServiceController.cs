using System;
using System.Threading;
using Taurus.Mvc;
using Taurus.Mvc.Attr;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 微服务 - 注册中心。
    /// </summary>
    internal partial class MicroServiceController : Controller
    {
        /// <summary>
        /// 停止微服务
        /// </summary>
        [MicroService]
        public void Stop()
        {
            if (MsConfig.IsClient && MsConfig.Client.RemoteExit)
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
            if (MsConfig.Client.IsEnable && MsConfig.IsClient && MsConfig.Client.RemoteExit)
            {
                MsConfig.Client.IsApplicationExit = true;//注销注册中心服务。
                new Thread(new ThreadStart(AppExit)).Start();
                Write("Remote exit success: wait for the register center to unregister the host (15s).", true);
            }
            else
            {
                Write("Remote exit fail : microservice remote exit is disabled.", false);
            }
        }

        private void AppExit()
        {
            MsLog.WriteDebugLine("Remote Exit : wait for the register center to unregister the host (15s).");
            for (int i = 15; i >= 0; i--)
            {
                MsLog.WriteDebugLine("Ready to stop : " + i + "s");
                Thread.Sleep(1000);
            }
            Environment.Exit(0);
        }
    }
}
