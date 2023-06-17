using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Tool;
using Taurus.Mvc;
using Taurus.Plugin.Limit;

namespace Taurus.Plugin.MicroService
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
            //初始化文件配置
            InitClientHostList();
            bool isFirst = true;
            while (true)
            {
                try
                {
                    if (MsConfig.Client.IsExitApplication)
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
                    if (MsConfig.Client.IsEnable)
                    {
                        if (isFirst)
                        {
                            isFirst = false;
                            AfterGetListOfClient(GetListOfClient());
                            Thread.Sleep(1000);
                        }
                        AfterRegHost(RegHost());
                    }
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
        private static void InitClientHostList()
        {
            string hostListJson = IO.Read(MsConst.ClientGatewayJsonPath);
            if (!string.IsNullOrEmpty(hostListJson))
            {
                var dic = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(hostListJson);
                PreConnection(dic);
                Client.Gateway.HostList = dic;
            }
        }
        #region 服务注册

        /// <summary>
        /// 微服务应用中心调用：服务注册。
        /// </summary>
        /// <returns></returns>
        private static string RegHost()
        {
            string url = MsConfig.Client.RcUrl + MsConfig.Client.RcPath + "/reg";
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Client.RcKey);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    //Content - Type: multipart / form - data; boundary = ----WebKitFormBoundaryxSUOuGdhfM6ceac8
                    string data = "name={0}&host={1}&version={2}&isVirtual={3}&domain={4}&pid=" + MvcConst.ProcessID;
                    data = string.Format(data, MsConfig.Client.Name, MvcConfig.RunUrl, MsConfig.Client.Version, MsConfig.Client.IsVirtual ? 1 : 0, MsConfig.Client.Domain);
                    string result = wc.UploadString(url, data);
                    if (JsonHelper.IsSuccess(result))
                    {
                        Client.IsLiveOfMasterRC = true;
                        MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg : {1} Version : {2} => OK", MvcConst.ProcessID, MsConfig.Client.Name, MsConfig.Client.Version));
                    }
                    else
                    {
                        Client.IsLiveOfMasterRC = false;
                        if (!string.IsNullOrEmpty(Client.Host2))
                        {
                            //主备切换。
                            string rcUrl = MsConfig.Client.RcUrl + "";
                            MsConfig.Client.RcUrl = Client.Host2;
                            Client.Host2 = rcUrl;
                        }
                        MsLog.Write(result, url, "POST");
                        MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg.Fail : {1}: ", MvcConst.ProcessID, result));
                    }
                    return result;
                }
            }
            catch (Exception err)
            {
                Client.IsLiveOfMasterRC = false;
                if (!string.IsNullOrEmpty(Client.Host2))
                {
                    //主备切换。
                    string rcUrl = MsConfig.Client.RcUrl + "";
                    MsConfig.Client.RcUrl = Client.Host2;
                    Client.Host2 = rcUrl;
                }
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg.Error : {1}: ", MvcConst.ProcessID, err.Message));
                MsLog.Write(err.Message, url, "POST");
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

                long configTick = JsonHelper.GetValue<long>(result, "configtick");
                SyncConfig(configTick);
            }
        }


        #endregion

        #region 获取注册列表
        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        internal static string GetListOfClient()
        {
            string url = MsConfig.Client.RcUrl + MsConfig.Client.RcPath + "/getlist?tick=" + Client.Tick;
            try
            {
                string result = string.Empty;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Client.RcKey);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    result = wc.DownloadString(url);
                }
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList : Tick : {1}  => OK", MvcConst.ProcessID, Client.Tick));
                return result;
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList.Error : {1}", MvcConst.ProcessID, err.Message));
                MsLog.Write(err.Message, url, "GET");
                return err.Message;
            }
        }
        private static void AfterGetListOfClient(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                long tick = JsonHelper.GetValue<long>(result, "tick");
                if (Client.Tick > tick) { return; }
                Client.Tick = tick;
                string json = JsonHelper.GetValue<string>(result, "msg");
                if (!string.IsNullOrEmpty(json))
                {
                    var dic = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                    PreConnection(dic);
                    Client.Gateway.HostList = dic;
                    IO.Write(MsConst.ClientGatewayJsonPath, json);
                }
            }
        }

        #endregion



    }

}
