using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Policy;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Json;
using CYQ.Data.Tool;
using Taurus.Mvc;
using Taurus.Plugin.Limit;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {

        /// <summary>
        /// 网关 - 运行
        /// </summary>
        private static void RunLoopOfGateway(object threadID)
        {
            //初始化文件配置
            InitGatewayHostList();
            while (true)
            {
                try
                {
                    if (MsConfig.Server.IsEnable && MsConfig.IsGateway)
                    {
                        string url = MsConfig.Server.RcUrl + MsConfig.Server.RcPath + "/getlist?tick=" + Server.Tick;
                        url += "&isGateway=1&pid=" + MvcConst.ProcessID;
                        AfterGetListOfGateway(GetListOfGateway(url), url);
                    }
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

        /// <summary>
        /// 初始化 网关 -  数据。
        /// </summary>
        private static void InitGatewayHostList()
        {
            string hostListJson = IO.Read(MsConst.ServerGatewayJsonPath);
            if (!string.IsNullOrEmpty(hostListJson))
            {
                var dic = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(hostListJson);
                PreConnection(dic);
                Server.Gateway.HostList = dic;
            }
        }

        private static void AfterGetListOfGateway(string result, string url)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                Server.IsLiveOfMasterRC = true;

                #region IP 黑名单同步

                long ipTick = JsonHelper.GetValue<long>(result, "iptick");
                SyncIPList(ipTick);
                #endregion

                #region 从备份请求切回主程序
                string host = JsonHelper.GetValue<string>(result, "host");
                if (!string.IsNullOrEmpty(host) && host != MsConfig.Server.RcUrl)
                {
                    MsConfig.Server.RcUrl = host;
                }
                Server.Host2 = JsonHelper.GetValue<string>(result, "host2");
                #endregion

                long tick = JsonHelper.GetValue<long>(result, "tick");
                if (Server.Tick > tick) { return; }
                Server.Tick = tick;
                string json = JsonHelper.GetValue<string>(result, "msg");
                if (!string.IsNullOrEmpty(json))
                {
                    var keyValues = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                    PreConnection(keyValues);
                    Server.Gateway.HostList = keyValues;
                    IO.Write(MsConst.ServerGatewayJsonPath, json);
                }
            }
            else
            {
                Server.IsLiveOfMasterRC = false;
                if (!string.IsNullOrEmpty(Server.Host2))
                {
                    //主备切换。
                    string rcUrl = MsConfig.Server.RcUrl + "";//避开引用
                    MsConfig.Server.RcUrl = Server.Host2;
                    Server.Host2 = rcUrl;
                }
                MsLog.Write(result, url, "POST");
            }
        }

        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        internal static string GetListOfGateway(string url)
        {
            try
            {
                string result = string.Empty;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Server.RcKey);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    result = wc.DownloadString(url);
                }
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList : Tick : {1}  => OK", MvcConst.ProcessID, Server.Tick));
                return result;
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList.Error : {1}", MvcConst.ProcessID, err.Message));
                MsLog.Write(err.Message, url, "GET");
                return err.Message;
            }

        }


    }

}
