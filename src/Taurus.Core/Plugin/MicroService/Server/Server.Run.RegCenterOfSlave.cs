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
        /// <summary>
        /// 注册中心 从 - 运行
        /// </summary>
        /// <param name="threadID"></param>
        private static void RunLoopRegCenterOfSlave(object threadID)
        {
            InitRegCenterOfSlaveHostList();
            bool isFirstRun = true;
            while (true)
            {
                try
                {
                    if (MsConfig.Server.IsEnable)
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
                    if (!Server.IsLiveOfMasterRC)
                    {
                        if (isFirstRun)
                        {
                            isFirstRun = false;
                            //恢复时间，避免被清除。
                            var regCenterList = Server.RegCenter.HostList;
                            List<string> keys = regCenterList.GetKeys();
                            foreach (string key in keys)
                            {
                                if(regCenterList.ContainsKey(key))
                                {
                                    var item = regCenterList[key];
                                    foreach (var ci in item)
                                    {
                                        ci.RegTime = DateTime.Now;
                                    }
                                }
                            }
                        }
                        CheckAndClearExpireHost(false);
                    }
                    else
                    {
                        isFirstRun = true;
                    }
                }
            }
        }

        /// <summary>
        /// 初始化 注册中心 （从） 数据。
        /// </summary>
        private static void InitRegCenterOfSlaveHostList()
        {
            string hostListJson = IO.Read(MsConst.ServerRegCenterOfSlaveJsonPath);
            if (!string.IsNullOrEmpty(hostListJson))
            {
                var dic = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(hostListJson);
                PreConnection(dic);
                Server.Gateway.HostList = dic;
                if (MsConfig.IsRegCenterOfSlave)
                {
                    //做为一名 从 注册中心，要时刻把自己放在随时接替主的位置。
                    Server.RegCenter.HostList = dic;
                    Server.RegCenter.HostListJson = hostListJson;
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
                        AfterGetListOfSlave(GetListOfSlave());
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

        private static void AfterGetListOfSlave(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                long ipTick = JsonHelper.GetValue<long>(result, "iptick");
                IPLimit.SyncIPList(ipTick);

                long tick = JsonHelper.GetValue<long>(result, "tick");
                if (Server.Tick > tick) { return; }
                Server.Tick = tick;
                string json = JsonHelper.GetValue<string>(result, "msg");
                if (!string.IsNullOrEmpty(json))
                {
                    var keyValues = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                    PreConnection(keyValues);
                    Server.Gateway.HostList = keyValues;
                    //做为一名 从 注册中心，要时刻把自己放在随时接替主的位置。
                    Server.RegCenter.HostList = keyValues;
                    Server.RegCenter.HostListJson = json;

                    IO.Write(MsConst.ServerRegCenterOfSlaveJsonPath, json);
                }
            }
        }

        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        internal static string GetListOfSlave()
        {
            string url = MsConfig.Server.RcUrl + MsConfig.Server.RcPath + "/getlist?tick=" + Server.Tick;
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
                MsLog.Write(err.Message, url, "GET", MsConfig.Server.Name);
                return err.Message;
            }
        }

        #endregion

    }

}
