using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Tool;
using Taurus.Mvc;
using Taurus.Plugin.Limit;

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
            bool isFirst = true;
            while (true)
            {
                try
                {
                    if (MsConfig.IsApplicationExit)
                    {
                        break;//停止注册，即注销。
                    }
                    #region 测试并发注册
                    //string name = MsConfig.ClientName;
                    //for (int i = 0; i < 1000000; i++)
                    //{
                    //    MsConfig.ClientName = name + "," + i;
                    //    MsConfig.AppRunUrl = "http://localhost:" + i;
                    //    AfterRegHost(RegHost());
                    //}
                    #endregion

                    //获取列表，再注册【Rpc的调用，需要有列表，再注册自身（获得请求分配）】

                    if (isFirst)
                    {
                        isFirst = false;
                        AfterGetListOfClient(GetListOfClient());
                        Thread.Sleep(1000);
                    }

                    AfterRegHost(RegHost());
                }
                catch (Exception err)
                {
                    Log.Write(err.Message, "MicroService");
                }
                finally
                {
                    Thread.Sleep(new Random().Next(5000, 8000));//5-10秒循环1次，GetList中间卡了1秒。
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

            string url = MsConfig.Client.RcUrl + "/microservice/reg";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Client.Key);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MsConfig.App.RunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    //Content - Type: multipart / form - data; boundary = ----WebKitFormBoundaryxSUOuGdhfM6ceac8
                    string data = "name={0}&host={1}&version={2}";
                    string result = wc.UploadString(url, string.Format(data, MsConfig.Client.Name, MsConfig.App.RunUrl, MsConfig.Client.Version));
                    Client.RegCenterIsLive = true;
                    if (JsonHelper.IsSuccess(result))
                    {
                        MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg : {1} Version : {2} => OK", MvcConst.ProcessID, MsConfig.Client.Name, MsConfig.Client.Version));
                    }
                    else
                    {
                        MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg.Fail : {1}: ", MvcConst.ProcessID, result));
                    }
                    return result;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg.Error : {1}: ", MvcConst.ProcessID, err.Message));
                Client.RegCenterIsLive = false;
                if (!string.IsNullOrEmpty(Client.Host2))
                {
                    MsConfig.Client.RcUrl = Client.Host2;//切换到备用库。
                }
                MsLog.Write(err.Message, url, "POST", MsConfig.Client.Name);
                return err.Message;
            }
        }

        private static void AfterRegHost(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                long tick = JsonHelper.GetValue<long>(result, "tick");
                Client.Host2 = JsonHelper.GetValue<string>(result, "host2");
                string host = JsonHelper.GetValue<string>(result, "host");
                if (!string.IsNullOrEmpty(host) && host != MsConfig.Client.RcUrl)
                {
                    MsConfig.Client.RcUrl = host;//从备份请求切回主程序
                }
                if (tick > Client.Tick)
                {
                    Thread.Sleep(1000);
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
            string url = MsConfig.Client.RcUrl + "/microservice/getlist?tick=" + Client.Tick;
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Client.Key);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MsConfig.App.RunUrl);
                    string result = wc.DownloadString(url);
                    Client.RegCenterIsLive = true;
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList : Tick : {1}  => OK", MvcConst.ProcessID, Client.Tick));
                    return result;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList.Error : {1}", MvcConst.ProcessID, err.Message));

                Client.RegCenterIsLive = false;
                if (!string.IsNullOrEmpty(Client.Host2))
                {
                    MsConfig.Client.RcUrl = Client.Host2;//切换到备用库。
                }

                MsLog.Write(err.Message, url, "GET", MsConfig.Client.Name);
                return err.Message;
            }
        }
        private static void AfterGetListOfClient(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                string host2 = JsonHelper.GetValue<string>(result, "host2");
                string host = JsonHelper.GetValue<string>(result, "host");
                if (!string.IsNullOrEmpty(host) && host != MsConfig.Client.RcUrl)
                {
                    MsConfig.Client.RcUrl = host;//从备份请求切回主程序
                }
                long tick = JsonHelper.GetValue<long>(result, "tick");

                if (Client.Tick > tick) { return; }
                Client.Tick = tick;
                Client.Host2 = host2;

                string json = JsonHelper.GetValue<string>(result, "msg");
                if (!string.IsNullOrEmpty(json))
                {
                    Client.Gateway.HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                    Client.Gateway.HostListJson = json;
                }
            }
        }

        #endregion



    }

}
