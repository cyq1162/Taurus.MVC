using System;
using System.Collections.Generic;
using System.Threading;
using CYQ.Data.Tool;
using Taurus.Mvc;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {

        /// <summary>
        /// 注册中心 主 - 运行
        /// </summary>
        /// <param name="threadID"></param>
        private static void RunLoopRegCenterOfMaster(object threadID)
        {
            InitRegCenterHostList();
            bool isFirstRun = true;
            while (true)
            {
                if (MsConfig.Server.IsEnable)
                {
                    CheckAndClearExpireHost(isFirstRun);
                    isFirstRun = false;
                }
                Thread.Sleep(3000);//测试并发。
            }
        }
        /// <summary>
        /// 注册中心(主 - 从) - 清理过期服务。
        /// </summary>
        internal static void CheckAndClearExpireHost(bool isFirstRun)
        {
            try
            {
                var regCenterList = Server.RegCenter.HostList;
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
                    Server.IsChange = false;
                    Server.Tick = DateTime.Now.Ticks;
                    Server.RegCenter.HostList = kvForRegCenter;
                    var json = JsonHelper.ToJson(kvForRegCenter);
                    Server.Gateway.HostList = kvForGateway;
                    Server.RegCenter.HostListJson = json;
                    PreConnection(kvForGateway);
                    if (MsConfig.IsRegCenterOfMaster)
                    {
                        IO.Write(MsConst.ServerRegCenterJsonPath, json);//存（主）注册中心数据到硬盘文件中。
                    }
                    else
                    {
                        IO.Write(MsConst.ServerRegCenterOfSlaveJsonPath, json);//存（从）注册中心数据到硬盘文件中。
                    }
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
            catch (Exception err)
            {
                MsLog.WriteDebugLine(err.Message);
                MsLog.Write(err.Message, "MicroService.Run.ClearExpireHost()", "", MsConfig.Server.Name);
            }
        }


        /// <summary>
        /// 初始化 注册中心 （主） 数据。
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

    }

}
