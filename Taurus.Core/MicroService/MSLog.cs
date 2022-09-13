using CYQ.Data;
using System;
using System.Diagnostics;
namespace Taurus.MicroService
{
    internal class MSLog
    {
        /// <summary>
        /// 内部日志记录
        /// </summary>
        public static void Write(string msg, string url, string httpMethod, string moduleName)
        {
            SysLogs sysLogs = new SysLogs();
            sysLogs.LogType = "MicroService";
            sysLogs.Message = msg;
            sysLogs.PageUrl = url;
            sysLogs.HttpMethod = httpMethod;
            sysLogs.ClientIP = sysLogs.Host;
            sysLogs.Host = MSConfig.AppRunUrl;
            sysLogs.HostName = moduleName;
            sysLogs.Write();
        }

        public static void WriteDebugLine(string msg)
        {
            Console.WriteLine(msg);
            Debug.WriteLine(msg);
        }
    }
}
