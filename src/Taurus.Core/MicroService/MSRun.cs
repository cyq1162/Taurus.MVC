using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Tool;
using Taurus.Mvc;
namespace Taurus.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MSRun
    {
        #region 微服务线程启动 - 自循环

        private static bool isStart = false;
        /// <summary>
        /// 微服务客户端启动
        /// </summary>
        internal static void Start(string host)
        {
            if (!isStart)
            {
                isStart = true;
                if (MSConfig.IsServer)
                {
                    if (ServicePointManager.DefaultConnectionLimit == 2)
                    {
                        ServicePointManager.DefaultConnectionLimit = 2048;//对.net framework有效。
                        ThreadPool.SetMinThreads(30, 50);
                    }
                }
                MSLog.WriteDebugLine("MicroService.Run.Start.V" + MvcConfig.TaurusVersion + " : ");
                if (MSConfig.IsRegCenterOfMaster)
                {
                    MSLog.WriteDebugLine("Run As MicroService.Server : Master.RegCenter");
                    Thread thread = new Thread(new ThreadStart(ClearServerTable));
                    thread.Start();
                }

                if (string.IsNullOrEmpty(MSConfig.AppRunUrl))
                {
                    MSConfig.AppRunUrl = host.ToLower();//设置当前程序运行的请求网址。
                }
                if (!string.IsNullOrEmpty(host))
                {
                    MSLog.WriteDebugLine("MicroService.App.RunUrl : " + host);
                }

                if (!string.IsNullOrEmpty(MSConfig.ServerName) && !string.IsNullOrEmpty(MSConfig.ServerRegUrl) && MSConfig.ServerRegUrl != MSConfig.AppRunUrl)
                {
                    if (MSConfig.IsRegCenter || MSConfig.IsGateway)
                    {
                        if (MSConfig.IsRegCenter)
                        {
                            MSLog.WriteDebugLine("Run As MicroService.Server : Slave.RegCenter");
                        }
                        else
                        {
                            MSLog.WriteDebugLine("Run As MicroService.Server : Gateway");
                        }
                        MSLog.WriteDebugLine("MicroService.Server.RegUrl : " + MSConfig.ServerRegUrl);
                        ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(ServerRunByLoop));
                    }
                }
                if (!string.IsNullOrEmpty(MSConfig.ClientName) && !string.IsNullOrEmpty(MSConfig.ClientRegUrl) && MSConfig.ClientRegUrl != MSConfig.AppRunUrl)
                {
                    MSLog.WriteDebugLine("Run As MicroService.Client : " + MSConfig.ClientName);
                    MSLog.WriteDebugLine("MicroService.Client.RegUrl : " + MSConfig.ClientRegUrl);
                    ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(ClientRunByLoop));
                }
            }
        }


        /// <summary>
        /// 网关、注册中心运行时。
        /// </summary>
        private static void ServerRunByLoop(object threadID)
        {
            
            MSLog.WriteDebugLine("MicroService.Run.ServerRunByLoop.");
            while (true)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString()+ " : MicroService.Run.ServerRunByLoop...");
                    if (MSConfig.IsGateway)
                    {
                        AfterGetList(GetHostList(true), true);//仅读取服务列表
                    }
                    else if (MSConfig.IsRegCenter)
                    {
                        AfterRegHost2(RegHost2());//注册中心（备用节点、走数据同步）
                    }
                    Thread.Sleep(new Random().Next(5000, 10000));//5-10秒循环1次。
                }
                catch (Exception err)
                {
                    Thread.Sleep(new Random().Next(5000, 10000));//5-10秒循环1次。
                    Log.Write(err.Message, "MicroService");
                }
            }
        }
        /// <summary>
        /// 微服务模块运行时。
        /// </summary>
        private static void ClientRunByLoop(object threadID)
        {
            MSLog.WriteDebugLine("MicroService.Run.ClientRunByLoop.");
            while (true)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + " : MicroService.Run.ClientRunByLoop...");
                    ////测试并发与大数据量
                    //for (int i = 0; i < 10000; i++)
                    //{
                    //    string hostName = MSConfig.ClientName;
                    //    if (i > 0)
                    //    {
                    //        hostName = "test" + i;
                    //    }

                    //}
                    AfterRegHost(RegHost());
                    Thread.Sleep(new Random().Next(5000, 10000));//5-10秒循环1次。
                }
                catch (Exception err)
                {
                    Thread.Sleep(new Random().Next(5000, 10000));//5-10秒循环1次。
                    Log.Write(err.Message, "MicroService");
                }
            }
        }

        #endregion

        #region 网络请求 - 后续逻辑处理

        private static void AfterGetList(string result, bool isServer)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                string host2 = JsonHelper.GetValue<string>(result, "host2");
                string host = JsonHelper.GetValue<string>(result, "host");
                if (!string.IsNullOrEmpty(host) && host != MSConfig.ServerRegUrl)
                {
                    MSConfig.ServerRegUrl = host;//从备份请求切回主程序
                }
                long tick = JsonHelper.GetValue<long>(result, "tick");
                if (isServer)
                {
                    if (Server.Tick > tick) { return; }
                    Server.Tick = tick;
                    Server.Host2 = host2;

                    if (MSConfig.ServerName.ToLower() == MSConst.Gateway)
                    {
                        if (!string.IsNullOrEmpty(host2))
                        {
                            IO.Write(MSConst.ServerHost2Path, host2);
                        }
                        else
                        {
                            IO.Delete(MSConst.ServerHost2Path);
                        }
                    }

                }
                else
                {
                    if (Client.Tick > tick) { return; }
                    Client.Tick = tick;
                    Client.Host2 = host2;
                }

                string json = JsonHelper.GetValue<string>(result, "msg");
                if (!string.IsNullOrEmpty(json))
                {
                    if (isServer)
                    {
                        Server._HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                        IO.Write(MSConst.ServerHostListJsonPath, json);
                    }
                    else
                    {
                        Client._HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                        IO.Write(MSConst.ClientHostListJsonPath, json);
                    }
                }
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
                        AfterGetList(GetHostList(true), true);
                    }
                }
            }
        }
        private static void AfterRegHost(string result)
        {
            if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
            {
                long tick = JsonHelper.GetValue<long>(result, "tick");
                Client.Host2 = JsonHelper.GetValue<string>(result, "host2");
                if (!string.IsNullOrEmpty(Client.Host2))
                {
                    IO.Write(MSConst.ClientHost2Path, Client.Host2);
                }
                else
                {
                    IO.Delete(MSConst.ClientHost2Path);
                }
                string host = JsonHelper.GetValue<string>(result, "host");
                if (!string.IsNullOrEmpty(host) && host != MSConfig.ClientRegUrl)
                {
                    MSConfig.ClientRegUrl = host;//从备份请求切回主程序
                }
                if (tick > Client.Tick)
                {
                    AfterGetList(GetHostList(false), false);
                }
            }
        }
        #endregion

        #region 网络请求

        /// <summary>
        /// 微服务应用中心调用：服务注册。
        /// </summary>
        /// <returns></returns>
        private static string RegHost()
        {
            string url = MSConfig.ClientRegUrl + "/MicroService/Reg";

            try
            {

                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MSConst.HeaderKey, MSConfig.ClientKey);
                    wc.Headers.Add("Referer", MSConfig.AppRunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    //Content - Type: multipart / form - data; boundary = ----WebKitFormBoundaryxSUOuGdhfM6ceac8
                    string data = "name={0}&host={1}&version={2}";
                    string result = wc.UploadString(url, string.Format(data, MSConfig.ClientName, MSConfig.AppRunUrl, MSConfig.ClientVersion));
                    Client.RegCenterIsLive = true;
                    MSLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Client.RegHost : " + result);
                    return result;
                }
            }
            catch (Exception err)
            {
                MSLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Client.RegHost Error : " + err.Message);
                Client.RegCenterIsLive = false;
                if (!string.IsNullOrEmpty(Client.Host2))
                {
                    MSConfig.ClientRegUrl = Client.Host2;//切换到备用库。
                }
                MSLog.Write(err.Message, url, "POST", MSConfig.ClientName);
                return err.Message;
            }
        }
        /// <summary>
        /// （备用）注册中心调用：备用地址注册。
        /// </summary>
        /// <returns></returns>
        private static string RegHost2()
        {
            string url = MSConfig.ServerRegUrl + "/MicroService/Reg2";
            try
            {
                string result = string.Empty;
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MSConst.HeaderKey, MSConfig.ServerKey);
                    wc.Headers.Add("Referer", MSConfig.AppRunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    string data = "host={0}&tick=" + Server.Tick;
                    result = wc.UploadString(url, string.Format(data, MSConfig.AppRunUrl));
                }
                MSLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.RegHost2 : " + result);
                Server.RegCenterIsLive = true;
                return result;
            }
            catch (Exception err)
            {
                MSLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.RegHost2 Error : " + err.Message);
                Server.RegCenterIsLive = false;
                MSLog.Write(err.Message, url, "POST", MSConfig.ServerName);
                return err.Message;
            }
        }
        /// <summary>
        /// 注册中心-数据同步【备用=》主机】。
        /// </summary>
        /// <returns></returns>
        private static void SyncHostList()
        {
            string url = MSConfig.ServerRegUrl + "/MicroService/SyncList";
            try
            {

                string data = string.Format("json={0}&tick=" + Server.Tick, JsonHelper.ToJson(Server.HostList));
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MSConst.HeaderKey, MSConfig.ServerKey);
                    wc.Headers.Add("Referer", MSConfig.AppRunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    wc.UploadString(url, data);
                }
                Server.RegCenterIsLive = true;
                MSLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.SyncHostList : ");
            }
            catch (Exception err)
            {
                MSLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.SyncHostList Error : " + err.Message);
                Server.RegCenterIsLive = false;
                MSLog.Write(err.Message, url, "POST", MSConfig.ServerName);
            }
        }
        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        /// <param name="isServer">请求端</param>
        internal static string GetHostList(bool isServer)
        {
            string url = (isServer ? MSConfig.ServerRegUrl : MSConfig.ClientRegUrl) + "/MicroService/GetList?tick=" + (isServer ? Server.Tick : Client.Tick);
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MSConst.HeaderKey, (isServer ? MSConfig.ServerKey : MSConfig.ClientKey));
                    wc.Headers.Set("Referer", MSConfig.AppRunUrl);
                    string result = wc.DownloadString(url);
                    if (isServer)
                    {
                        Server.RegCenterIsLive = true;
                    }
                    else
                    {
                        Client.RegCenterIsLive = true;
                    }
                    MSLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : {0}.GetHostList OK : Tick : {1}", (isServer ? "Server" : "Client"), (isServer ? Server.Tick : Client.Tick)));
                    return result;
                }
            }
            catch (Exception err)
            {
                MSLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : GetHostList Error : " + err.Message);
                if (isServer)
                {
                    Server.RegCenterIsLive = false;
                    if (!string.IsNullOrEmpty(Server.Host2))
                    {
                        MSConfig.ServerRegUrl = Server.Host2;//切换到备用库。
                    }
                }
                else
                {
                    Client.RegCenterIsLive = false;
                    if (!string.IsNullOrEmpty(Client.Host2))
                    {
                        MSConfig.ServerRegUrl = Client.Host2;//切换到备用库。
                    }
                }
                MSLog.Write(err.Message, url, "GET", isServer ? MSConfig.ServerName : MSConfig.ClientName);
                return err.Message;
            }
        }


        #endregion

        #region 服务主机清理。
        /// <summary>
        /// 清理服务主机。
        /// </summary>
        internal static void ClearServerTable()
        {
            while (true)
            {
                try
                {
                    lock (MSConst.tableLockObj)
                    {
                        if (Server._HostList != null && Server._HostList.Count > 0)
                        {
                            MDictionary<string, List<HostInfo>> keyValuePairs = Server._HostList;//拿到引用
                            MDictionary<string, List<HostInfo>> newKeyValuePairs = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                            foreach (var item in keyValuePairs)
                            {
                                List<HostInfo> newList = new List<HostInfo>();
                                foreach (var info in item.Value)
                                {
                                    if (info.RegTime < DateTime.Now.AddSeconds(-11) || info.Version < 0)
                                    {
                                        Server.IsChange = true;
                                    }
                                    else
                                    {
                                        newList.Add(info);
                                    }
                                }
                                if (newList.Count > 0)
                                {
                                    newKeyValuePairs.Add(item.Key, newList);
                                }
                            }

                            if (Server.IsChange)
                            {
                                Server.IsChange = false;
                                Server.Tick = DateTime.Now.Ticks;
                                if (newKeyValuePairs.Count > 0)
                                {
                                    Server._HostListJson = JsonHelper.ToJson(newKeyValuePairs);
                                    IO.Write(MSConst.ServerHostListJsonPath, Server._HostListJson);
                                }
                                else
                                {
                                    Server._HostListJson = String.Empty;
                                    IO.Delete(MSConst.ServerHostListJsonPath);
                                }
                                Server._HostList = newKeyValuePairs;
                            }
                            else
                            {
                                newKeyValuePairs.Clear();
                                newKeyValuePairs = null;
                            }
                        }
                    }
                }
                catch (Exception err)
                {
                    MSLog.Write(err.Message, "MicroService.Run.ClearServerTable()", "", MSConfig.ServerName);
                }
                Thread.Sleep(5000);//测试并发。
            }
        }
        #endregion
    }

}
