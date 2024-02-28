using CYQ.Data;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;
using Taurus.Mvc.Reflect;
using Taurus.Plugin.Doc;
using Taurus.Plugin.MicroService;

namespace Taurus.Mvc
{
    class MvcRun
    {
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
                new Thread(new ThreadStart(OnInit)).Start();
            }
        }
        /// <summary>
        /// ASP.NET 启动。
        /// </summary>
        private static void OnInit()
        {
            ControllerCollector.InitControllers();
            ViewEngine.InitViews();//View 预热加载
            //DocConfig.Init();//初始化 Doc 模块的 Xml 预加载，【考量到 Doc 一般并不进行对外访问，因此不进行预处理】
        }
        private static void WriteDebugLine(string msg)
        {
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }
}
