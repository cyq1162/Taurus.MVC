using CYQ.Data;
using System;
using System.Collections;
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
        static bool hasStartThread = false;
        /// <summary>
        /// ASP.NET 启动。
        /// </summary>
        public static void ASPNET_Start()
        {
            if (!hasStartThread)
            {
                hasStartThread = true;
                new Thread(new ThreadStart(Start), 512).Start();
            }
        }

        static bool hasStart = false;
        /// <summary>
        ///  ASP.NET Core 启动。
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
                ControllerCollector.InitControllers();
                ViewEngine.InitStyles();
            }
        }
        private static void WriteDebugLine(string msg)
        {
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }
}
