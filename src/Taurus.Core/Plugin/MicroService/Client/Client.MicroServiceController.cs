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
        /// 应用程序退出
        /// </summary>
        [MicroService]
        public void Exit()
        {
            if (MsConfig.IsClient && MsConfig.Client.RemoteExit)
            {
                MsConfig.IsApplicationExit = true;//注销注册中心服务。
                new Thread(new ThreadStart(AppExit)).Start();
                Write("Remote Exit Success: wait for the registry to unregister the host (10s).", true);
            }
            else
            {
                Write("MicroService remote exit is disabled.", false);
            }
        }

        private void AppExit()
        {
            MsLog.WriteDebugLine("Remote Exit : wait for the registry to unregister the host (15s).");
            for (int i = 15; i >= 0; i--)
            {
                MsLog.WriteDebugLine("Ready to stop : " + i + "s");
                Thread.Sleep(1000);
            }
            Environment.Exit(0);
        }
    }
}
