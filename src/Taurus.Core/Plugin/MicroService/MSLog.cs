using CYQ.Data;
using System;
using System.Diagnostics;
using Taurus.Mvc;

namespace Taurus.Plugin.MicroService
{
    internal class MsLog
    {
        /// <summary>
        /// 内部日志记录
        /// </summary>
        public static void Write(string msg, string url, string httpMethod)
        {
            SysLogs sysLogs = new SysLogs();
            sysLogs.LogType = "MicroService";
            sysLogs.Message = msg;
            sysLogs.RequestUrl = url;
            sysLogs.RefererUrl = MvcConfig.RunUrl;
            sysLogs.HttpMethod = httpMethod;
            sysLogs.Write();
        }

        public static void WriteDebugLine(string msg)
        {
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }
}
