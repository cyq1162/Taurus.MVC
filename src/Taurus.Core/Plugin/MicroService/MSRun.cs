using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Tool;
using Taurus.Mvc;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {
        /// <summary>
        /// 应用程序启用时间
        /// </summary>
        public static DateTime StartTime = DateTime.MinValue;

        static MsRun()
        {
            StartTime = DateTime.Now;
            if (MsConfig.IsServer || MsConfig.IsClient)
            {
                string folder = AppConfig.WebRootPath + "App_Data/microservice";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }

        private static bool hasStart = false;
        internal static void Start(Uri uri)
        {
            if (!hasStart)
            {
                string urlAbs = uri.AbsoluteUri;
                string urlPath = uri.PathAndQuery;
                string host = urlAbs.Substring(0, urlAbs.Length - urlPath.Length);
                Start(host);
            }
        }
        /// <summary>
        /// 微服务客户端启动
        /// </summary>
        internal static void Start(string host)
        {
            if (string.IsNullOrEmpty(MvcConfig.RunUrl) && !string.IsNullOrEmpty(host))
            {
                MvcConfig.RunUrl = host.ToLower().TrimEnd('/');//设置当前程序运行的请求网址。
            }
            if (!hasStart)
            {
                hasStart = true;
                //Start 内部自有判断。
                MsRun.RegistryCenterOfMaster.Start();
                MsRun.RegistryCenterOfSlave.Start();
                MsRun.Gateway.Start();
                MsRun.ClientRun.Start();
            }
        }

        #region 链接预建立检测。

        static bool hasInitThreads = false;
        private static void InitThreads()
        {
            if (hasInitThreads) { return; }
            hasInitThreads = true;
            ServicePointManager.ServerCertificateValidationCallback = delegate { return true; };
            if (ServicePointManager.DefaultConnectionLimit == 2)
            {
                ServicePointManager.DefaultConnectionLimit = 2048;//对.net framework有效。
                ThreadPool.SetMinThreads(30, 50);
            }
        }


        private static void PreConnection(MDictionary<string, List<HostInfo>> keyValues)
        {
            Dictionary<string, byte> keyValuePairs = new Dictionary<string, byte>();
            foreach (var items in keyValues)
            {
                string lowerKey = items.Key;
                if (lowerKey == MsConst.RegistryCenter || lowerKey == MsConst.RegistryCenterOfSlave || lowerKey == MsConst.Gateway || lowerKey.Contains("."))
                {
                    continue;//不需要对服务端进行预请求，域名也不需要进行。
                }
                foreach (var info in items.Value)
                {
                    if (!keyValuePairs.ContainsKey(info.Host))
                    {
                        keyValuePairs.Add(info.Host, 1);
                        Rpc.Gateway.PreConnection(info);//对于新加入的请求，发起一次请求建立预先链接。
                    }
                }
            }
            keyValuePairs.Clear();
        }

        //private static void WriteToDb(MDictionary<string, List<HostInfo>> hostList)
        //{
        //    if (hostList != null && hostList.Count > 0 && !string.IsNullOrEmpty(MsConfig.MsConn))
        //    {
        //        if (DBTool.TestConn(MsConfig.MsConn))
        //        {
        //            MDataTable table = Server.CreateTable(hostList);
        //            if (table.Rows.Count > 0)
        //            {
        //                table.AcceptChanges(AcceptOp.Auto, System.Data.IsolationLevel.Unspecified, null, "MsName", "Host");
        //                //bool result = table.AcceptChanges(AcceptOp.Auto, System.Data.IsolationLevel.Unspecified, null, "MsName", "Host");
        //                //MsLog.WriteDebugLine("AcceptChanges : " + result.ToString());
        //                // if(!result
        //            }
        //            table.Rows.Clear();
        //        }
        //    }
        //}

        #endregion
    }

}
