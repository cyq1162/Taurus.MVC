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
            if (string.IsNullOrEmpty(MvcConfig.RunUrl))
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} RunUrl is empty.", MvcConst.ProcessID));
                return "";
            }
            string url = MsConfig.Server.RcUrl + "/" + MsConfig.Server.Path + "/reg2";
            try
            {
                string result = string.Empty;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Server.Key);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    string data = "host={0}&tick=" + Server.Tick;
                    result = wc.UploadString(url, string.Format(data, MvcConfig.RunUrl));
                }
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg2 : {1}: ", MvcConst.ProcessID, result));
                Server.IsLiveOfMasterRC = true;
                return result;
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg2.Error : {1}: ", MvcConst.ProcessID, err.Message));
                Server.IsLiveOfMasterRC = false;
                MsLog.Write(err.Message, url, "POST", MsConfig.Server.Name);
                return err.Message;
            }
        }

        private static void AfterRegHost2(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                long tick = JsonHelper.GetValue<long>(result, "tick");
                long ipTick = JsonHelper.GetValue<long>(result, "iptick");
                IPLimit.SyncIPList(ipTick);
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
            string url = MsConfig.Server.RcUrl + "/" + MsConfig.Server.Path + "/synclist";
            try
            {

                string data = string.Format("json={0}&tick=" + Server.Tick, Server.Gateway.HostListJson);
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Server.Key);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    wc.UploadString(url, data);
                }
                Server.IsLiveOfMasterRC = true;
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} SyncHostList : = > OK. ", MvcConst.ProcessID));
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} SyncHostList.Error : {1}", MvcConst.ProcessID, err.Message));
                Server.IsLiveOfMasterRC = false;
                MsLog.Write(err.Message, url, "POST", MsConfig.Server.Name);
            }
        }
        #endregion

        #region 网络请求

        private static void AfterGetListOfServer(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                long ipTick = JsonHelper.GetValue<long>(result, "iptick");
                IPLimit.SyncIPList(ipTick);

                string host2 = JsonHelper.GetValue<string>(result, "host2");
                string host = JsonHelper.GetValue<string>(result, "host");
                if (!string.IsNullOrEmpty(host) && host != MsConfig.Server.RcUrl)
                {
                    MsConfig.Server.RcUrl = host;//从备份请求切回主程序
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
                    if (MsConfig.IsRegCenter)
                    {
                        //做为一名 从 注册中心，要时刻把自己放在随时接替主的位置。
                        Server.RegCenter.HostList = keyValues;
                        Server.RegCenter.HostListJson = json;
                    }
                }
            }
        }

        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        internal static string GetListOfServer()
        {
            string url = MsConfig.Server.RcUrl + "/" + MsConfig.Server.Path + "/getlist?tick=" + Server.Tick;
            if (MsConfig.IsGateway)
            {
                url += "&isGateway=1";
            }
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Server.Key);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    string result = wc.DownloadString(url);
                    Server.IsLiveOfMasterRC = true;
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList : Tick : {1}  => OK", MvcConst.ProcessID, Server.Tick));
                    return result;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList.Error : {1}", MvcConst.ProcessID, err.Message));
                Server.IsLiveOfMasterRC = false;
                if (!string.IsNullOrEmpty(Server.Host2))
                {
                    MsConfig.Server.RcUrl = Server.Host2;//切换到备用库。
                }
                MsLog.Write(err.Message, url, "GET", MsConfig.Server.Name);
                return err.Message;
            }
        }

        #endregion

        #region 服务主机清理。
        /// <summary>
        /// 注册中心(主) - 清理过期服务。
        /// 对于 注册中心（从），不调用此进行清理，哪怕切换到主(临时），也只做持续存档
        /// </summary>
        internal static void ClearExpireHost()
        {
            bool isFirstRun = true;
            while (true)
            {
                try
                {
                    var regCenterList = Server.RegCenter.HostList;
                    if (regCenterList != null)
                    {
                        Server.RegCenter.AddHost("RegCenter", MvcConfig.RunUrl);
                        Server.RegCenter.LoadHostByAdmin();//加载所有手工添加主机信息
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

                        if (Server.IsChange || isFirstRun)
                        {
                            isFirstRun = false;
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
                            if (Server.Gateway.HostList == null)
                            {
                                Server.Gateway.HostList = kvForGateway;
                            }
                            else
                            {
                                kvForGateway = null;
                            }
                            kvForRegCenter = null;
                        }
                    }

                }
                catch (Exception err)
                {
                    MsLog.WriteDebugLine(err.Message);
                    MsLog.Write(err.Message, "MicroService.Run.ClearExpireHost()", "", MsConfig.Server.Name);
                }

                Thread.Sleep(3000);//测试并发。
            }
        }

        private static void PreConnection(MDictionary<string, List<HostInfo>> keyValues)
        {
            foreach (var items in keyValues)
            {
                if (items.Key == "RegCenter" || items.Key == "RegCenterOfSlave" || items.Key == "Gateway")
                {
                    continue;//不需要对服务端进行预请求。
                }
                foreach (var info in items.Value)
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
