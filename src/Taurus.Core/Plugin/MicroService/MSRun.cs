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
                string folder = AppConst.WebRootPath + "App_Data/microservice";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
            }
        }

        private static bool hasStart = false;
        /// <summary>
        /// ASP.NET 启动。
        /// </summary>
        /// <param name="uri"></param>
        internal static void ASPNET_Start(Uri uri)
        {
            if (!hasStart)
            {
                string urlAbs = uri.AbsoluteUri;
                string urlPath = uri.PathAndQuery;
                string host = urlAbs.Substring(0, urlAbs.Length - urlPath.Length);
                if (string.IsNullOrEmpty(MvcConfig.RunUrl) && !string.IsNullOrEmpty(host))
                {
                    MvcConfig.RunUrl = host.ToLower().TrimEnd('/');//设置当前程序运行的请求网址。
                }
                Start();
            }
        }
        /// <summary>
        /// ASP.NET Core 启动。
        /// </summary>
        internal static void Start()
        {
            if (!hasStart)
            {
                hasStart = true;
                ReStart();
            }
        }
        internal static void ReStart()
        {
            //Start 内部自有判断。
            RegistryCenterOfMaster.Run.Start();
            RegistryCenterOfSlave.Run.Start();
            Gateway.Run.Start();
            Client.Run.Start();
        }
    }

}
