using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using System.Diagnostics;
using static Taurus.MicroService.Rpc;

namespace Taurus.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {
        #region 微服务线程启动 - 自循环

        private static bool isStart = false;
        /// <summary>
        /// 微服务客户端启动
        /// </summary>
        internal static void Start(string host)
        {
            if (string.IsNullOrEmpty(MsConfig.AppRunUrl))
            {
                MsConfig.AppRunUrl = host.ToLower().TrimEnd('/');//设置当前程序运行的请求网址。
            }
            if (!isStart)
            {
                MsLog.WriteDebugLine("--------------------------------------------------");
                MsLog.WriteDebugLine("Current App Process ID    ：" + Process.GetCurrentProcess().Id);
                MsLog.WriteDebugLine("Current Taurus Version    ：" + MvcConfig.TaurusVersion);
                isStart = true;
                if (MsConfig.IsServer)
                {
                    if (ServicePointManager.DefaultConnectionLimit == 2)
                    {
                        ServicePointManager.DefaultConnectionLimit = 2048;//对.net framework有效。
                        ThreadPool.SetMinThreads(30, 50);
                    }
                }
                
                if (MsConfig.IsRegCenterOfMaster)
                {
                    MsLog.WriteDebugLine("Current MicroService Type ：RegCenter of Master");
                    Thread thread = new Thread(new ThreadStart(ClearServerTable));
                    thread.Start();
                }
                //if (!string.IsNullOrEmpty(host))
                //{
                //    MsLog.WriteDebugLine("MicroService.App.RunUrl : " + host);
                //}

                if (!string.IsNullOrEmpty(MsConfig.ServerName) && !string.IsNullOrEmpty(MsConfig.ServerRegUrl) && MsConfig.ServerRegUrl != MsConfig.AppRunUrl)
                {
                    if (MsConfig.IsRegCenter || MsConfig.IsGateway)
                    {
                        if (MsConfig.IsRegCenter)
                        {
                            MsLog.WriteDebugLine("Current MicroService Type ：RegCenter of Slave");
                        }
                        else
                        {
                            MsLog.WriteDebugLine("Current MicroService Type ：Gateway");
                        }
                        //MsLog.WriteDebugLine("MicroService.Server.RegUrl : " + MsConfig.ServerRegUrl);
                        ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(ServerRunByLoop));
                    }
                }
                if (!string.IsNullOrEmpty(MsConfig.ClientName) && !string.IsNullOrEmpty(MsConfig.ClientRegUrl) && MsConfig.ClientRegUrl != MsConfig.AppRunUrl)
                {
                    MsLog.WriteDebugLine("Current MicroService Type ：Client of 【" + MsConfig.ClientName+"】");
                    //MsLog.WriteDebugLine("MicroService.Client.RegUrl : " + MsConfig.ClientRegUrl);
                    ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(ClientRunByLoop));
                }
                MsLog.WriteDebugLine("--------------------------------------------------");
            }
        }


        /// <summary>
        /// 网关、注册中心运行时。
        /// </summary>
        private static void ServerRunByLoop(object threadID)
        {

            MsLog.WriteDebugLine("MicroService.Run.ServerRunByLoop.");
            while (true)
            {
                try
                {
                    System.Diagnostics.Debug.WriteLine(DateTime.Now.ToString() + " : MicroService.Run.ServerRunByLoop...");
                    if (MsConfig.IsGateway)
                    {
                        AfterGetList(GetHostList(true), true);//仅读取服务列表
                    }
                    else if (MsConfig.IsRegCenter)
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
            MsLog.WriteDebugLine("MicroService.Run.ClientRunByLoop.");
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
                if (!string.IsNullOrEmpty(host) && host != MsConfig.ServerRegUrl)
                {
                    MsConfig.ServerRegUrl = host;//从备份请求切回主程序
                }
                long tick = JsonHelper.GetValue<long>(result, "tick");
                if (isServer)
                {
                    if (Server.Tick > tick) { return; }
                    Server.Tick = tick;
                    Server.Host2 = host2;

                    if (MsConfig.ServerName.ToLower() == MsConst.Gateway)
                    {
                        if (!string.IsNullOrEmpty(host2))
                        {
                            IO.Write(MsConst.ServerHost2Path, host2);
                        }
                        else
                        {
                            IO.Delete(MsConst.ServerHost2Path);
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
                        IO.Write(MsConst.ServerHostListJsonPath, json);
                    }
                    else
                    {
                        Client._HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                        IO.Write(MsConst.ClientHostListJsonPath, json);
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
                    IO.Write(MsConst.ClientHost2Path, Client.Host2);
                }
                else
                {
                    IO.Delete(MsConst.ClientHost2Path);
                }
                string host = JsonHelper.GetValue<string>(result, "host");
                if (!string.IsNullOrEmpty(host) && host != MsConfig.ClientRegUrl)
                {
                    MsConfig.ClientRegUrl = host;//从备份请求切回主程序
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
            string url = MsConfig.ClientRegUrl + "/microservice/reg";

            try
            {

                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.ClientKey);
                    wc.Headers.Add("Referer", MsConfig.AppRunUrl);
                    wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                    //Content - Type: multipart / form - data; boundary = ----WebKitFormBoundaryxSUOuGdhfM6ceac8
                    string data = "name={0}&host={1}&version={2}";
                    string result = wc.UploadString(url, string.Format(data, MsConfig.ClientName, MsConfig.AppRunUrl, MsConfig.ClientVersion));
                    Client.RegCenterIsLive = true;
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Client.RegHost : " + result);
                    return result;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : Client.RegHost Error : " + err.Message);
                Client.RegCenterIsLive = false;
                if (!string.IsNullOrEmpty(Client.Host2))
                {
                    MsConfig.ClientRegUrl = Client.Host2;//切换到备用库。
                }
                MsLog.Write(err.Message, url, "POST", MsConfig.ClientName);
                return err.Message;
            }
        }
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
        /// <summary>
        /// 注册中心-数据同步【备用=》主机】。
        /// </summary>
        /// <returns></returns>
        private static void SyncHostList()
        {
            string url = MsConfig.ServerRegUrl + "/microservice/synclist";
            try
            {

                string data = string.Format("json={0}&tick=" + Server.Tick, JsonHelper.ToJson(Server.HostList));
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
        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        /// <param name="isServer">请求端</param>
        internal static string GetHostList(bool isServer)
        {
            string url = (isServer ? MsConfig.ServerRegUrl : MsConfig.ClientRegUrl) + "/microservice/getlist?tick=" + (isServer ? Server.Tick : Client.Tick);
            if (MsConfig.IsGateway)
            {
                url += "&isGateway=1";
            }
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, (isServer ? MsConfig.ServerKey : MsConfig.ClientKey));
                    wc.Headers.Set("Referer", MsConfig.AppRunUrl);
                    string result = wc.DownloadString(url);
                    if (isServer)
                    {
                        Server.RegCenterIsLive = true;
                    }
                    else
                    {
                        Client.RegCenterIsLive = true;
                    }
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : {0}.GetHostList OK : Tick : {1}", (isServer ? "Server" : "Client"), (isServer ? Server.Tick : Client.Tick)));
                    return result;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : GetHostList Error : " + err.Message);
                if (isServer)
                {
                    Server.RegCenterIsLive = false;
                    if (!string.IsNullOrEmpty(Server.Host2))
                    {
                        MsConfig.ServerRegUrl = Server.Host2;//切换到备用库。
                    }
                }
                else
                {
                    Client.RegCenterIsLive = false;
                    if (!string.IsNullOrEmpty(Client.Host2))
                    {
                        MsConfig.ServerRegUrl = Client.Host2;//切换到备用库。
                    }
                }
                MsLog.Write(err.Message, url, "GET", isServer ? MsConfig.ServerName : MsConfig.ClientName);
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
                    lock (MsConst.tableLockObj)
                    {
                        if (Server.HostList != null)//Server._HostList != null && 
                        {
                            Server.AddHost("RegCenter", MsConfig.AppRunUrl);
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

                            if (newKeyValuePairs.Count > 0)
                            {
                                Server._HostListJson = JsonHelper.ToJson(newKeyValuePairs);
                                IO.Write(MsConst.ServerHostListJsonPath, Server._HostListJson);
                            }
                            else
                            {
                                IO.Delete(MsConst.ServerHostListJsonPath);
                                Server._HostListJson = String.Empty;
                            }
                            WriteToDb(keyValuePairs);
                            if (Server.IsChange)
                            {
                                Server.IsChange = false;
                                Server.Tick = DateTime.Now.Ticks;
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
                    MsLog.WriteDebugLine(err.Message);
                    MsLog.Write(err.Message, "MicroService.Run.ClearServerTable()", "", MsConfig.ServerName);
                }
                Thread.Sleep(5000);//测试并发。
            }
        }

        private static void WriteToDb(MDictionary<string, List<HostInfo>> hostList)
        {
            if (hostList != null && hostList.Count > 0 && !string.IsNullOrEmpty(MsConfig.MsConn))
            {
                if (DBTool.TestConn(MsConfig.MsConn))
                {
                    MDataTable table = Server.CreateTable(hostList);
                    if(table.Rows.Count > 0)
                    {
                        table.AcceptChanges(AcceptOp.Auto, System.Data.IsolationLevel.Unspecified, null, "MsName", "Host");
                        //bool result = table.AcceptChanges(AcceptOp.Auto, System.Data.IsolationLevel.Unspecified, null, "MsName", "Host");
                        //MsLog.WriteDebugLine("AcceptChanges : " + result.ToString());
                       // if(!result
                    }
                    table.Rows.Clear();
                }
            }
        }

        #endregion
    }

}
