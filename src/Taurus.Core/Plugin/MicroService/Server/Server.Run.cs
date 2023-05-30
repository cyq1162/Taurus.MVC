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
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {
        #region 注册中心 - 主

        /// <summary>
        /// 注册中心 主 - 运行
        /// </summary>
        /// <param name="threadID"></param>
        private static void RunLoopRegCenterOfMaster(object threadID)
        {
            InitRegCenterHostList();
            CheckAndClearExpireHost();
        }
        /// <summary>
        /// 注册中心(主) - 清理过期服务。
        /// 对于 注册中心（从），不调用此进行清理，哪怕切换到主(临时），也只做持续存档
        /// </summary>
        internal static void CheckAndClearExpireHost()
        {
            bool isFirstRun = true;
            while (true)
            {
                if (MsConfig.Server.IsEnable)
                {
                    try
                    {
                        var regCenterList = Server.RegCenter.HostList;
                        if (regCenterList != null)
                        {
                            Server.RegCenter.AddHost("RegCenter", MvcConfig.RunUrl, MvcConst.ProcessID, MvcConst.HostIP);
                            Server.RegCenter.LoadHostByAdmin();//加载所有手工添加主机信息
                            List<string> keys = regCenterList.GetKeys();
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
                                var json = JsonHelper.ToJson(kvForRegCenter);
                                Server.Gateway.HostList = kvForGateway;
                                Server.RegCenter.HostListJson = json;
                                PreConnection(kvForGateway);
                                IO.Write(MsConst.ServerRegCenterJsonPath, json);//存注册中心数据到硬盘文件中。

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
                }
                Thread.Sleep(3000);//测试并发。
            }
        }
        #endregion

        #region 网关 - 从注册中心

        /// <summary>
        /// 网关、从注册中心 - 运行
        /// </summary>
        private static void RunLoopOfServer(object threadID)
        {
            //初始化文件配置
            InitServerHostList();
            while (true)
            {
                try
                {
                    if (MsConfig.Server.IsEnable)
                    {
                        if (MsConfig.IsGateway)
                        {
                            AfterGetListOfServer(GetListOfServer());//仅读取服务列表
                        }
                        else if (MsConfig.IsRegCenter)
                        {
                            AfterRegHost2(RegHost2());//注册中心（备用节点、走数据同步）
                        }
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
        /// 初始化 注册中心 数据。
        /// </summary>
        private static void InitRegCenterHostList()
        {
            string hostListJson = IO.Read(MsConst.ServerRegCenterJsonPath);
            if (!string.IsNullOrEmpty(hostListJson))
            {
                var dic = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(hostListJson);
                //恢复时间，避免被清除。
                foreach (var item in dic)
                {
                    if (item.Value != null)
                    {
                        foreach (var ci in item.Value)
                        {
                            ci.RegTime = DateTime.Now;
                        }
                    }
                }
                PreConnection(dic);
                Server.Gateway.HostList = dic;
                Server.RegCenter.HostList = dic;
                Server.RegCenter.HostListJson = hostListJson;

            }
        }
        /// <summary>
        /// 初始化 网关 -  从注册中心 数据。
        /// </summary>
        private static void InitServerHostList()
        {
            string hostListJson = IO.Read(MsConst.ServerGatewayJsonPath);
            if (!string.IsNullOrEmpty(hostListJson))
            {
                var dic = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(hostListJson);
                PreConnection(dic);
                Server.Gateway.HostList = dic;
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
            string url = MsConfig.Server.RcUrl + MsConfig.Server.RcPath + "/reg2";
            try
            {
                string result = string.Empty;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Server.RcKey);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    string data = string.Format("host={0}&tick={1}&pid={2}", MvcConfig.RunUrl, Server.Tick, MvcConst.ProcessID);
                    result = wc.UploadString(url, data);
                }
                if (JsonHelper.IsSuccess(result))
                {
                    Server.IsLiveOfMasterRC = true;
                }
                else
                {
                    Server.IsLiveOfMasterRC = false;
                    MsLog.Write(result, url, "POST", MsConfig.Server.Name);
                }
                Server.Host2 = MsConfig.Server.RcUrl;
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg2 : {1}: ", MvcConst.ProcessID, result));
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
            string url = MsConfig.Server.RcUrl + MsConfig.Server.RcPath + "/synclist";
            try
            {

                string data = string.Format("json={0}&tick=" + Server.Tick, Server.RegCenter.HostListJson);
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Server.RcKey);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    wc.UploadString(url, data);
                }
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} SyncHostList : = > OK. ", MvcConst.ProcessID));
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} SyncHostList.Error : {1}", MvcConst.ProcessID, err.Message));
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
                if (MsConfig.IsGateway)
                {
                    string host = JsonHelper.GetValue<string>(result, "host");
                    if (!string.IsNullOrEmpty(host) && host != MsConfig.Server.RcUrl)
                    {
                        MsConfig.Server.RcUrl = host;//从备份请求切回主程序
                    }
                    Server.Host2 = JsonHelper.GetValue<string>(result, "host2");
                }
                long tick = JsonHelper.GetValue<long>(result, "tick");
                if (Server.Tick > tick) { return; }
                Server.Tick = tick;
                string json = JsonHelper.GetValue<string>(result, "msg");
                if (!string.IsNullOrEmpty(json))
                {
                    var keyValues = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                    PreConnection(keyValues);
                    Server.Gateway.HostList = keyValues;
                    if (MsConfig.IsRegCenterOfSlave)
                    {
                        //做为一名 从 注册中心，要时刻把自己放在随时接替主的位置。
                        Server.RegCenter.HostList = keyValues;
                        Server.RegCenter.HostListJson = json;
                    }
                    IO.Write(MsConst.ServerGatewayJsonPath, json);
                }
            }
        }

        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        internal static string GetListOfServer()
        {
            string url = MsConfig.Server.RcUrl + MsConfig.Server.RcPath + "/getlist?tick=" + Server.Tick;
            if (MsConfig.IsGateway)
            {
                url += "&isGateway=1&pid=" + MvcConst.ProcessID;
            }
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
                if (MsConfig.IsGateway)
                {
                    //网关检测（从注册中心在注册主机处已有检测）
                    if (JsonHelper.IsSuccess(result))
                    {
                        Server.IsLiveOfMasterRC = true;
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
                        MsLog.Write(result, url, "POST", MsConfig.Server.Name);
                    }
                }
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList : Tick : {1}  => OK", MvcConst.ProcessID, Server.Tick));
                return result;
            }
            catch (Exception err)
            {
                if (MsConfig.IsGateway)
                {
                    Server.IsLiveOfMasterRC = false;
                    if (!string.IsNullOrEmpty(Server.Host2))
                    {
                        //主备切换。
                        string rcUrl = MsConfig.Server.RcUrl + "";//避开引用
                        MsConfig.Server.RcUrl = Server.Host2;
                        Server.Host2 = rcUrl;
                    }
                }
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList.Error : {1}", MvcConst.ProcessID, err.Message));
                MsLog.Write(err.Message, url, "GET", MsConfig.Server.Name);
                return err.Message;
            }
        }

        #endregion

        #endregion

    }

}
