using System;
using System.Collections.Generic;
using System.Net;
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
        public class RegistryCenterOfSlave
        {
            private static bool IsAllowToRun
            {
                get
                {
                    return MsConfig.Server.IsEnable && MsConfig.IsRegistryCenterOfSlave;
                }
            }
            static object lockObj = new object();
            static bool threadIsWorking = false;
            public static void Start()
            {
                if (threadIsWorking || !IsAllowToRun) { return; }
                lock (lockObj)
                {
                    if (!threadIsWorking)
                    {
                        threadIsWorking = true;
                        MsLog.WriteDebugLine("--------------------------------------------------");
                        MsLog.WriteDebugLine("Start Time        ：" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));
                        MsLog.WriteDebugLine("MicroService Type ：RegistryCenter of Slave");
                        MsLog.WriteDebugLine("RegistryCenter Url：" + MsConfig.Server.RcUrl);
                        MsLog.WriteDebugLine("--------------------------------------------------");
                        InitThreads();
                        ThreadPool.QueueUserWorkItem(new WaitCallback(RunLoopRegistryCenterOfSlave), null);
                    }
                }
            }

            /// <summary>
            /// 注册中心 从 - 运行
            /// </summary>
            /// <param name="threadID"></param>
            private static void RunLoopRegistryCenterOfSlave(object threadID)
            {
                InitRegistryCenterOfSlaveHostList();
                bool isFirstRun = true;
                while (IsAllowToRun)
                {
                    try
                    {
                        AfterRegHost2(RegHost2());//注册中心（备用节点、走数据同步）
                    }
                    catch (Exception err)
                    {
                        Log.Write(err.Message, "MicroService");
                    }
                    finally
                    {
                        Thread.Sleep(new Random().Next(5000, 8000));//5-8秒循环1次。
                        if (!Server.IsLiveOfMasterRC)
                        {
                            if (isFirstRun)
                            {
                                isFirstRun = false;
                                //恢复时间，避免被清除。
                                var rcList = Server.RegistryCenter.HostList;
                                List<string> keys = rcList.GetKeys();
                                foreach (string key in keys)
                                {
                                    if (rcList.ContainsKey(key))
                                    {
                                        var item = rcList[key];
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
                threadIsWorking = false;
            }

            static bool hasInit = false;
            /// <summary>
            /// 初始化 注册中心 （从） 数据。
            /// </summary>
            private static void InitRegistryCenterOfSlaveHostList()
            {
                if (hasInit) { return; }
                hasInit = true;
                string hostListJson = IO.Read(MsConst.ServerRegistryCenterOfSlaveJsonPath);
                if (!string.IsNullOrEmpty(hostListJson))
                {
                    var dic = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(hostListJson);
                    PreConnection(dic);
                    Server.Gateway.HostList = dic;
                    if (MsConfig.IsRegistryCenterOfSlave)
                    {
                        //做为一名 从 注册中心，要时刻把自己放在随时接替主的位置。
                        Server.RegistryCenter.HostList = dic;
                        Server.RegistryCenter.HostListJson = hostListJson;
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
                        MsLog.Write(result, url, "POST");
                    }
                    Server.Host2 = MsConfig.Server.RcUrl;
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg2 : {1}: ", MvcConst.ProcessID, result));
                    return result;
                }
                catch (Exception err)
                {
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} Reg2.Error : {1}: ", MvcConst.ProcessID, err.Message));
                    Server.IsLiveOfMasterRC = false;
                    MsLog.Write(err.Message, url, "POST");
                    return err.Message;
                }
            }

            private static void AfterRegHost2(string result)
            {
                if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
                {
                    long tick = JsonHelper.GetValue<long>(result, "tick");
                    long ipTick = JsonHelper.GetValue<long>(result, "iptick");
                    SyncIPList(ipTick);
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

                    string data = string.Format("json={0}&tick=" + Server.Tick, Server.RegistryCenter.HostListJson);
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
                    MsLog.Write(err.Message, url, "POST");
                }
            }
            #endregion

            #region 网络请求

            private static void AfterGetListOfSlave(string result)
            {
                if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
                {
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
                        Server.RegistryCenter.HostList = keyValues;
                        Server.RegistryCenter.HostListJson = json;

                        IO.Write(MsConst.ServerRegistryCenterOfSlaveJsonPath, json);
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
                    MsLog.Write(err.Message, url, "GET");
                    return err.Message;
                }
            }

            #endregion

        }
    }
}
