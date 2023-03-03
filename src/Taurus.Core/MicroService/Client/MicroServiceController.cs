using System;
using Taurus.Mvc;
using Taurus.Mvc.Attr;

namespace Taurus.MicroService
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
            if (!MsConfig.IsDisableExit)
            {
                Write("Environment exit.", true);
                Environment.Exit(0);
            }
            else
            {
                Write("Method is disabled.", false);
            }
        }
    }
}
