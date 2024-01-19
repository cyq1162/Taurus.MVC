using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using Taurus.Mvc.Reflect;

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
                ThreadPool.QueueUserWorkItem(new WaitCallback(ThreadStart), null);
            }
        }

        private static void ThreadStart(object p)
        {
            ControllerCollector.InitControllers();
            ViewEngine.InitStyles();
        }
    }
}
