using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using System.Diagnostics;
using static Taurus.MicroService.Rpc;

namespace Taurus.MicroService
{
    /// <summary>
    /// 运行中心 - 客户端
    /// </summary>
    internal partial class MsRun
    {
        /// <summary>
        /// 微服务模块运行时。
        /// </summary>
        private static void RunLoopOfClient(object threadID)
        {
            while (true)
            {
                try
                {
                    AfterRegHost(RegHost());
                }
                catch (Exception err)
                {
                    Log.Write(err.Message, "MicroService");
                }
                finally
                {
                    Thread.Sleep(new Random().Next(5000, 10000));//5-10秒循环1次。
                }
            }
        }
        #region 服务注册

        /// <summary>
        /// 微服务应用中心调用：服务注册。
        /// </summary>
        /// <returns></returns>
        private static string RegHost()
        {
            string url = MsConfig.ClientRegUrl + "/microservice/reg";

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.ClientKey);
                    wc.Headers.Add("Referer", MsConfig.AppRunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    //Content - Type: multipart / form - data; boundary = ----WebKitFormBoundaryxSUOuGdhfM6ceac8
                    string data = "name={0}&host={1}&version={2}";
                    string result = wc.UploadString(url, string.Format(data, MsConfig.ClientName, MsConfig.AppRunUrl, MsConfig.ClientVersion));
                    Client.RegCenterIsLive = true;
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : RegHost OK : Tick : {0} Module : {1}", Client.Tick, MsConfig.ClientName));
                    return result;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : RegHost.Error : " + err.Message);
                Client.RegCenterIsLive = false;
                if (!string.IsNullOrEmpty(Client.Host2))
                {
                    MsConfig.ClientRegUrl = Client.Host2;//切换到备用库。
                }
                MsLog.Write(err.Message, url, "POST", MsConfig.ClientName);
                return err.Message;
            }
        }

        private static void AfterRegHost(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                long tick = JsonHelper.GetValue<long>(result, "tick");
                Client.Host2 = JsonHelper.GetValue<string>(result, "host2");
                if (!string.IsNullOrEmpty(Client.Host2))
                {
                    IO.Write(MsConst.ClientHost2Path, Client.Host2);
                }
                else
                {
                    IO.Delete(MsConst.ClientHost2Path);
                }
                string host = JsonHelper.GetValue<string>(result, "host");
                if (!string.IsNullOrEmpty(host) && host != MsConfig.ClientRegUrl)
                {
                    MsConfig.ClientRegUrl = host;//从备份请求切回主程序
                }
                if (tick > Client.Tick)
                {
                    AfterGetListOfClient(GetListOfClient());
                }
            }
        }


        #endregion

        #region 获取注册列表
        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        internal static string GetListOfClient()
        {
            string url = MsConfig.ClientRegUrl + "/microservice/getlist?tick=" + Client.Tick;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.ClientKey);
                    wc.Headers.Set("Referer", MsConfig.AppRunUrl);
                    string result = wc.DownloadString(url);
                    Client.RegCenterIsLive = true;
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : GetList OK : Tick : {0}", Client.Tick));
                    return result;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : GetList.Error : " + err.Message);

                Client.RegCenterIsLive = false;
                if (!string.IsNullOrEmpty(Client.Host2))
                {
                    MsConfig.ClientRegUrl = Client.Host2;//切换到备用库。
                }

                MsLog.Write(err.Message, url, "GET", MsConfig.ClientName);
                return err.Message;
            }
        }
        private static void AfterGetListOfClient(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                string host2 = JsonHelper.GetValue<string>(result, "host2");
                string host = JsonHelper.GetValue<string>(result, "host");
                if (!string.IsNullOrEmpty(host) && host != MsConfig.ServerRegUrl)
                {
                    MsConfig.ServerRegUrl = host;//从备份请求切回主程序
                }
                long tick = JsonHelper.GetValue<long>(result, "tick");

                if (Client.Tick > tick) { return; }
                Client.Tick = tick;
                Client.Host2 = host2;

                string json = JsonHelper.GetValue<string>(result, "msg");
                if (!string.IsNullOrEmpty(json))
                {
                    Client.HostListJson = json;
                }
            }
        }

        #endregion



    }

}
