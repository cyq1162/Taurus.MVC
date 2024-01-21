using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Taurus.Mvc.Reflect;
using Taurus.Plugin.MicroService;

namespace Taurus.Mvc
{
    class MvcRun
    {
        static bool hasStart = false;
        /// <summary>
        /// 启用后仅启运行1次。
        /// </summary>
        public static void Start()
        {
            if (!hasStart)
            {
                hasStart = true;
                WriteDebugLine("--------------------------------------------------");
                WriteDebugLine("Current App Start Time    ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                WriteDebugLine("Current App Process ID    ：" + MvcConst.ProcessID);
                WriteDebugLine("Current Taurus Version    ：" + MvcConst.Version);
                WriteDebugLine("Current CYQ.Data Version  ：" + AppConfig.Version);
                WriteDebugLine("--------------------------------------------------");
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadStart), null);
            }
        }

        private static void ThreadStart(object p)
        {
            ControllerCollector.InitControllers();
            ViewEngine.InitStyles();
        }
        private static void WriteDebugLine(string msg)
        {
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }
}
