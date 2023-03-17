using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using System.Diagnostics;

namespace Taurus.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {

        /// <summary>
        /// 网关、注册中心运行时。
        /// </summary>
        private static void RunLoopOfServer(object threadID)
        {
            while (true)
            {
                try
                {
                    if (MsConfig.IsApplicationExit)
                    {
                        break;//停止注册，即注销。
                    }
                    if (MsConfig.IsGateway)
                    {
                        AfterGetListOfServer(GetListOfServer());//仅读取服务列表
                    }
                    else if (MsConfig.IsRegCenter)
                    {
                        AfterRegHost2(RegHost2());//注册中心（备用节点、走数据同步）
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

        #region 注册中心 - 从 - 同步
        /// <summary>
        /// （备用）注册中心调用：备用地址注册。
        /// </summary>
        /// <returns></returns>
        private static string RegHost2()
        {
            string url = MsConfig.ServerRegUrl + "/microservice/reg2";
            try
            {
                string result = string.Empty;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.ServerKey);
                    wc.Headers.Add("Referer", MsConfig.AppRunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    string data = "host={0}&tick=" + Server.Tick;
                    result = wc.UploadString(url, string.Format(data, MsConfig.AppRunUrl));
                }
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.RegHost2 : " + result);
                Server.RegCenterIsLive = true;
                return result;
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.RegHost2 Error : " + err.Message);
                Server.RegCenterIsLive = false;
                MsLog.Write(err.Message, url, "POST", MsConfig.ServerName);
                return err.Message;
            }
        }

        private static void AfterRegHost2(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                long tick = JsonHelper.GetValue<long>(result, "tick");
                if (tick != Server.Tick)
                {
                    if (Server.Tick > tick)//主机重启了。
                    {
                        //推送数据同步
                        SyncHostList();
                    }
                    else //正常状态读取备份
                    {
                        AfterGetListOfServer(GetListOfServer());
                    }
                }
            }
        }

        /// <summary>
        /// 注册中心-数据同步【备用=》主机】。
        /// </summary>
        /// <returns></returns>
        private static void SyncHostList()
        {
            string url = MsConfig.ServerRegUrl + "/microservice/synclist";
            try
            {

                string data = string.Format("json={0}&tick=" + Server.Tick, Server.Gateway.HostListJson);
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.ServerKey);
                    wc.Headers.Add("Referer", MsConfig.AppRunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    wc.UploadString(url, data);
                }
                Server.RegCenterIsLive = true;
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.SyncHostList : ");
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.SyncHostList Error : " + err.Message);
                Server.RegCenterIsLive = false;
                MsLog.Write(err.Message, url, "POST", MsConfig.ServerName);
            }
        }
        #endregion

        #region 网络请求

        private static void AfterGetListOfServer(string result)
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

                if (Server.Tick > tick) { return; }
                Server.Tick = tick;
                Server.Host2 = host2;
                string json = JsonHelper.GetValue<string>(result, "msg");
                if (!string.IsNullOrEmpty(json))
                {
                    var keyValues = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                    PreConnection(keyValues);
                    Server.Gateway.HostList = keyValues;
                    Server.Gateway.HostListJson = json;
                }
            }
        }

        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        internal static string GetListOfServer()
        {
            string url = MsConfig.ServerRegUrl + "/microservice/getlist?tick=" + Server.Tick;
            if (MsConfig.IsGateway)
            {
                url += "&isGateway=1";
            }
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.ServerKey);
                    wc.Headers.Set("Referer", MsConfig.AppRunUrl);
                    string result = wc.DownloadString(url);
                    Server.RegCenterIsLive = true;
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : GetList OK : Tick : {0}", Server.Tick));
                    return result;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : GetHostList Error : " + err.Message);
                Server.RegCenterIsLive = false;
                if (!string.IsNullOrEmpty(Server.Host2))
                {
                    MsConfig.ServerRegUrl = Server.Host2;//切换到备用库。
                }
                MsLog.Write(err.Message, url, "GET", MsConfig.ServerName);
                return err.Message;
            }
        }


        #endregion

        #region 服务主机清理。
        /// <summary>
        /// 注册中心 - 清理过期服务。
        /// </summary>
        internal static void ClearExpireHost()
        {
            while (true)
            {
                try
                {
                    var regCenterList = Server.RegCenter.HostList;
                    if (regCenterList != null)
                    {
                        Server.RegCenter.AddHost("RegCenter", MsConfig.AppRunUrl);
                        List<string> keys = new List<string>(regCenterList.Count);
                        foreach (string item in regCenterList.Keys)
                        {
                            keys.Add(item);
                        }
                        var kvForRegCenter = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                        var kvForGateway = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                        foreach (string key in keys)
                        {
                            var items = regCenterList[key];
                            List<HostInfo> regList = new List<HostInfo>();
                            List<HostInfo> gatewayList = new List<HostInfo>();
                            for (int i = 0; i < items.Count; i++)
                            {
                                var info = items[i];
                                if (info.RegTime < DateTime.Now.AddSeconds(-11) || info.Version < 0)
                                {
                                    Server.IsChange = true;
                                }
                                else
                                {
                                    regList.Add(info);
                                    gatewayList.Add(info);
                                }
                            }
                            if (regList.Count > 0)
                            {
                                kvForRegCenter.Add(key, regList);
                                kvForGateway.Add(key, gatewayList);

                            }
                        }

                        if (Server.IsChange)
                        {
                            Server.IsChange = false;
                            Server.Tick = DateTime.Now.Ticks;
                            Server.RegCenter.HostList = kvForRegCenter;
                            Server.RegCenter.HostListJson = JsonHelper.ToJson(kvForRegCenter);
                            PreConnection(kvForGateway);
                            Server.Gateway.HostList = kvForGateway;
                            //WriteToDb(kvForGateway);//为了性能，取消写数据库操作。
                        }
                        else
                        {
                            kvForRegCenter = kvForGateway = null;
                        }
                    }

                }
                catch (Exception err)
                {
                    MsLog.WriteDebugLine(err.Message);
                    MsLog.Write(err.Message, "MicroService.Run.ClearExpireHost()", "", MsConfig.ServerName);
                }
                Thread.Sleep(3000);//测试并发。
            }
        }

        private static void PreConnection(MDictionary<string, List<HostInfo>> keyValues)
        {
            foreach (var items in keyValues.Values)
            {
                foreach (var info in items)
                {
                    Rpc.Gateway.PreConnection(new Uri(info.Host));//对于新加入的请求，发起一次请求建立预先链接。
                }
            }
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
