using System;
using System.Collections.Generic;
using System.Threading;
using CYQ.Data.Json;
using CYQ.Data.Tool;
using Taurus.Mvc;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class RegistryCenterOfMaster
    {
        public class Run
        {
            private static bool IsAllowToRun
            {
                get
                {
                    return MsConfig.Server.IsEnable && MsConfig.IsRegistryCenterOfMaster;
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
                        MsLog.WriteDebugLine("MicroService Type ：RegistryCenter of Master");
                        MsLog.WriteDebugLine("--------------------------------------------------");
                        //持续时间长、不占用线程队列。
                        new Thread(new ThreadStart(RunLoopRegistryCenterOfMaster)).Start();
                    }
                }
            }
            /// <summary>
            /// 注册中心 主 - 运行
            /// </summary>
            /// <param name="threadID"></param>
            private static void RunLoopRegistryCenterOfMaster()
            {
                InitRegistryCenterHostList();
                bool isFirstRun = true;
                while (IsAllowToRun)
                {
                    CheckAndClearExpireHost(isFirstRun);
                    isFirstRun = false;
                    Thread.Sleep(3000);
                }
                threadIsWorking = false;
            }
            static bool hasInit = false;
            /// <summary>
            /// 初始化 注册中心 （主） 数据。
            /// </summary>
            private static void InitRegistryCenterHostList()
            {
                if (hasInit) return;
                hasInit = true;
                try
                {
                    string hostListJson = IO.Read(MsConst.ServerRegistryCenterJsonPath);
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
                        Gateway.PreConnection(dic);
                        Gateway.Server.HostList = dic;
                        Server.RegistryCenter.HostList = dic;
                        Server.RegistryCenter.HostListJson = hostListJson;

                    }
                }
                catch (Exception err)
                {
                    MsLog.Write(err);
                }
            }
        }

        /// <summary>
        /// 注册中心(主 - 从) - 清理过期服务。
        /// </summary>
        internal static void CheckAndClearExpireHost(bool isFirstRun)
        {
            try
            {
                var rcList = Server.RegistryCenter.HostList;
                Server.RegistryCenter.AddHost("RegistryCenter", MvcConfig.RunUrl, MvcConst.ProcessID, MvcConst.HostIP);
                Server.RegistryCenter.LoadHostByAdmin();//加载所有手工添加主机信息
                List<string> keys = rcList.GetKeys();
                var kvForRegistryCenter = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                var kvForGateway = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                foreach (string key in keys)
                {
                    if (rcList.ContainsKey(key))
                    {
                        var items = rcList[key];
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
                            kvForRegistryCenter.Add(key, regList);
                            kvForGateway.Add(key, gatewayList);
                        }
                    }
                }

                if (Server.IsChange || isFirstRun)
                {
                    Server.IsChange = false;
                    Server.Tick = DateTime.Now.Ticks;
                    Server.RegistryCenter.HostList = kvForRegistryCenter;
                    var json = JsonHelper.ToJson(kvForRegistryCenter);
                    Gateway.Server.HostList = kvForGateway;
                    Server.RegistryCenter.HostListJson = json;
                    Gateway.PreConnection(kvForGateway);
                    if (MsConfig.IsRegistryCenterOfMaster)
                    {
                        IO.Write(MsConst.ServerRegistryCenterJsonPath, json);//存（主）注册中心数据到硬盘文件中。
                    }
                    else
                    {
                        IO.Write(MsConst.ServerRegistryCenterOfSlaveJsonPath, json);//存（从）注册中心数据到硬盘文件中。
                    }
                }
                else
                {
                    if (Gateway.Server.HostList == null)
                    {
                        Gateway.Server.HostList = kvForGateway;
                    }
                    else
                    {
                        kvForGateway = null;
                    }
                    kvForRegistryCenter = null;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(err.Message);
                MsLog.Write(err.Message, "MicroService.Run.CheckAndClearExpireHost()", "");
            }
        }



    }

}
