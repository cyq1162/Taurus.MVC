using System;
using System.Web;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Table;
using CYQ.Data.Tool;

namespace Taurus.Core
{
    /// <summary>
    /// 微服务的核心类
    /// </summary>
    public partial class MicroService
    {
        /// <summary>
        /// 运行中心
        /// </summary>
        internal partial class Run
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
                    Console.WriteLine("MicroService.Run.Start.V" + Version + " : ");
                    if (string.IsNullOrEmpty(Config.AppRunUrl))
                    {
                        Config.AppRunUrl = host.ToLower();//设置当前程序运行的请求网址。
                    }
                    if (Server.IsRegCenterOfMaster)
                    {
                        Console.WriteLine("Run As MicroService.Server : Master.RegCenter");
                        Thread thread = new Thread(new ThreadStart(ClearServerTable));
                        thread.Start();
                    }

                    if (!string.IsNullOrEmpty(Config.ServerName) && !string.IsNullOrEmpty(Config.ServerRegUrl) && Config.ServerRegUrl != Config.AppRunUrl)
                    {
                        if (Server.IsRegCenter || Server.IsGateway)
                        {
                            if (Server.IsRegCenter)
                            {
                                Console.WriteLine("Run As MicroService.Server : Slave.RegCenter");
                            }
                            else
                            {
                                Console.WriteLine("Run As MicroService.Server : Gateway");
                            }
                            Console.WriteLine("MicroService.Server.RegUrl : " + Config.ServerRegUrl);
                            Thread thread = new Thread(new ThreadStart(ServerRunByLoop));
                            thread.Start();

                        }
                    }
                    if (!string.IsNullOrEmpty(Config.ClientName) && !string.IsNullOrEmpty(Config.ClientRegUrl) && Config.ClientRegUrl != Config.AppRunUrl)
                    {
                        Console.WriteLine("Run As MicroService.Client : " + Config.ClientName);
                        Console.WriteLine("MicroService.Client.RegUrl : " + Config.ClientRegUrl);
                        Thread thread = new Thread(new ThreadStart(ClientRunByLoop));
                        thread.Start();
                    }
                }
            }


            /// <summary>
            /// 网关、注册中心运行时。
            /// </summary>
            private static void ServerRunByLoop()
            {
                Console.WriteLine("MicroService.Run.ServerRunByLoop.");
                while (true)
                {
                    try
                    {
                        if (Server.IsGateway)
                        {
                            AfterGetList(GetHostList(true), true);//仅读取服务列表
                        }
                        else if (Server.IsRegCenter)
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
            private static void ClientRunByLoop()
            {
                Console.WriteLine("MicroService.Run.ClientRunByLoop.");
                while (true)
                {
                    try
                    {
                        ////测试并发与大数据量
                        //for (int i = 0; i < 10000; i++)
                        //{
                        //    string hostName = Config.ClientName;
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
                    if (!string.IsNullOrEmpty(host) && host != Config.ServerRegUrl)
                    {
                        Config.ServerRegUrl = host;//从备份请求切回主程序
                    }
                    long tick = JsonHelper.GetValue<long>(result, "tick");
                    if (isServer)
                    {
                        if (Server.Tick > tick) { return; }
                        Server.Tick = tick;
                        Server.Host2 = host2;

                        if (Config.ServerName.ToLower() == Const.Gateway)
                        {
                            if (!string.IsNullOrEmpty(host2))
                            {
                                IO.Write(Const.ServerHost2Path, host2);
                            }
                            else
                            {
                                IO.Delete(Const.ServerHost2Path);
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
                            IO.Write(Const.ServerHostListJsonPath, json);
                        }
                        else
                        {
                            Client._HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                            IO.Write(Const.ClientHostListJsonPath, json);
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
                        IO.Write(Const.ClientHost2Path, Client.Host2);
                    }
                    else
                    {
                        IO.Delete(Const.ClientHost2Path);
                    }
                    string host = JsonHelper.GetValue<string>(result, "host");
                    if (!string.IsNullOrEmpty(host) && host != Config.ServerRegUrl)
                    {
                        Config.ServerRegUrl = host;//从备份请求切回主程序
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
                string url = Config.ClientRegUrl + "/MicroService/Reg";

                try
                {

                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(Const.HeaderKey, Config.ClientKey);
                        wc.Headers.Add("Referer", Config.AppRunUrl);
                        wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                        //Content - Type: multipart / form - data; boundary = ----WebKitFormBoundaryxSUOuGdhfM6ceac8
                        string data = "name={0}&host={1}&version={2}";
                        string result = wc.UploadString(url, string.Format(data, Config.ClientName, Config.AppRunUrl, Config.ClientVersion));
                        Client.RegCenterIsLive = true;
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Client.RegHost : " + result);
                        return result;
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Client.RegHost Error : " + err.Message);
                    Client.RegCenterIsLive = false;
                    if (!string.IsNullOrEmpty(Client.Host2))
                    {
                        Config.ClientRegUrl = Client.Host2;//切换到备用库。
                    }
                    LogWrite(err.Message, url, "POST", Config.ClientName);
                    return err.Message;
                }
            }
            /// <summary>
            /// （备用）注册中心调用：备用地址注册。
            /// </summary>
            /// <returns></returns>
            private static string RegHost2()
            {
                string url = Config.ServerRegUrl + "/MicroService/Reg2";
                try
                {
                    string result = string.Empty;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(Const.HeaderKey, Config.ServerKey);
                        wc.Headers.Add("Referer", Config.AppRunUrl);
                        wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                        string data = "host={0}&tick=" + Server.Tick;
                        result = wc.UploadString(url, string.Format(data, Config.AppRunUrl));
                    }
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.RegHost2 : " + result);
                    Server.RegCenterIsLive = true;
                    return result;
                }
                catch (Exception err)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.RegHost2 Error : " + err.Message);
                    Server.RegCenterIsLive = false;
                    LogWrite(err.Message, url, "POST", Config.ServerName);
                    return err.Message;
                }
            }
            /// <summary>
            /// 注册中心-数据同步【备用=》主机】。
            /// </summary>
            /// <returns></returns>
            private static void SyncHostList()
            {
                string url = Config.ServerRegUrl + "/MicroService/SyncList";
                try
                {

                    string data = string.Format("json={0}&tick=" + Server.Tick, JsonHelper.ToJson(Server.HostList));
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(Const.HeaderKey, Config.ServerKey);
                        wc.Headers.Add("Referer", Config.AppRunUrl);
                        wc.Headers.Add("Content-Type", "application/x-www-form-urlencoded");
                        wc.UploadString(url, data);
                    }
                    Server.RegCenterIsLive = true;
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.SyncHostList : ");
                }
                catch (Exception err)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.SyncHostList Error : " + err.Message);
                    Server.RegCenterIsLive = false;
                    LogWrite(err.Message, url, "POST", Config.ServerName);
                }
            }
            /// <summary>
            /// 获取注册中心注册数据列表。
            /// </summary>
            /// <param name="isServer">请求端</param>
            internal static string GetHostList(bool isServer)
            {
                string url = (isServer ? Config.ServerRegUrl : Config.ClientRegUrl) + "/MicroService/GetList?tick=" + (isServer ? Server.Tick : Client.Tick);
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(Const.HeaderKey, (isServer ? Config.ServerKey : Config.ClientKey));
                        wc.Headers.Set("Referer", Config.AppRunUrl);
                        string result = wc.DownloadString(url);
                        if (isServer)
                        {
                            Server.RegCenterIsLive = true;
                        }
                        else
                        {
                            Client.RegCenterIsLive = true;
                        }
                        Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : {0}.GetHostList OK : Tick : {1}", (isServer ? "Server" : "Client"), (isServer ? Server.Tick : Client.Tick)));
                        return result;
                    }
                }
                catch (Exception err)
                {
                    Console.WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : GetHostList Error : " + err.Message);
                    if (isServer)
                    {
                        Server.RegCenterIsLive = false;
                        if (!string.IsNullOrEmpty(Server.Host2))
                        {
                            Config.ServerRegUrl = Server.Host2;//切换到备用库。
                        }
                    }
                    else
                    {
                        Client.RegCenterIsLive = false;
                        if (!string.IsNullOrEmpty(Client.Host2))
                        {
                            Config.ServerRegUrl = Client.Host2;//切换到备用库。
                        }
                    }
                    LogWrite(err.Message, url, "GET", isServer ? Config.ServerName : Config.ClientName);
                    return err.Message;
                }
            }


            #endregion

            #region 网关代理、服务主机清理。
            /// <summary>
            /// 网关代理转发方法
            /// </summary>
            internal static bool Proxy(HttpContext context, bool isServerCall)
            {
                if ((isServerCall && !Server.IsServer) || (!isServerCall && !Client.IsClient))
                {
                    return false;
                }
                List<HostInfo> infoList = new List<HostInfo>();
                string module = string.Empty;
                IPAddress iPAddress;
                List<HostInfo> domainList = null;
                if (context.Request.Url.Host != "localhost" && !IPAddress.TryParse(context.Request.Url.Host, out iPAddress))
                {
                    module = context.Request.Url.Host;//域名转发优先。
                    domainList = isServerCall ? MicroService.Server.GetHostList(module) : MicroService.Client.GetHostList(module);
                    if (domainList == null || domainList.Count == 0)
                    {
                        return false;
                    }
                }

                if (context.Request.Url.LocalPath == "/")
                {
                    module = QueryTool.GetDefaultUrl().TrimStart('/').Split('/')[0];
                }
                else
                {
                    module = context.Request.Url.LocalPath.TrimStart('/').Split('/')[0];
                }
                List<HostInfo> moduleList = isServerCall ? MicroService.Server.GetHostList(module) : MicroService.Client.GetHostList(module);

                if (domainList == null || domainList.Count == 0) { infoList = moduleList; }
                else if (moduleList == null || moduleList.Count == 0) { infoList = domainList; }
                else
                {
                    foreach (var item in domainList)//过滤掉不在域名下的主机
                    {
                        foreach (var keyValue in moduleList)
                        {
                            if (item.Host == keyValue.Host)
                            {
                                infoList.Add(item);
                                break;
                            }
                        }
                    }
                }

                if (infoList==null || infoList.Count == 0)
                {
                    return false;
                }
                else
                {
                    int max = 3;//最多循环3个节点，避免长时间循环卡机。
                    bool isRegCenter = Server.IsRegCenterOfMaster;
                    HostInfo firstInfo = infoList[0];
                    if (firstInfo.CallIndex >= infoList.Count)
                    {
                        firstInfo.CallIndex = 0;//处理节点移除后，CallIndex最大值的问题。
                    }
                    for (int i = 0; i < infoList.Count; i++)
                    {
                        int callIndex = firstInfo.CallIndex + i;
                        if (callIndex >= infoList.Count)
                        {
                            callIndex = callIndex - infoList.Count;
                        }
                        HostInfo info = infoList[callIndex];

                        if (info.Version < 0 || (info.CallTime > DateTime.Now && infoList.Count > 0) || (isRegCenter && info.RegTime < DateTime.Now.AddSeconds(-10)))//正常5-10秒注册1次。
                        {
                            continue;//已经断开服务的。
                        }

                        if (Proxy(context, info.Host, isServerCall))
                        {
                            firstInfo.CallIndex = callIndex + 1;//指向下一个。
                            return true;
                        }
                        else
                        {
                            info.CallTime = DateTime.Now.AddMinutes(1);//网络异常的，延时1分钟检测。
                            max--;
                            if (max == 0)
                            {
                                return false;
                            }
                        }

                    }
                    return false;
                }
            }

            private static bool Proxy(HttpContext context, string host, bool isServerCall)
            {
                HttpRequest request = context.Request;
                string url = String.Empty;
                try
                {
                    byte[] bytes = null;

                    url = host + request.RawUrl;//.Substring(module.Length + 1);

                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Set(Const.HeaderKey, (isServerCall ? Config.ServerKey : Config.ClientKey));
                        wc.Headers.Set("X-Real-IP", request.UserHostAddress);
                        wc.Headers.Set("Referer", Config.AppRunUrl);//当前运行地址。
                        foreach (string key in request.Headers.Keys)
                        {
                            switch (key)
                            {
                                case "Connection"://引发异常 链接已关闭
                                case "Accept-Encoding"://引发乱码
                                case "Accept"://引发下载类型错乱
                                case "Referer":
                                    break;
                                default:
                                    wc.Headers.Set(key, request.Headers[key]);
                                    break;
                            }

                        }
                        if (request.HttpMethod == "GET")
                        {
                            bytes = wc.DownloadData(url);
                        }
                        else
                        {
                            byte[] data = null;
                            if (request.ContentLength > 0)
                            {
                                //Synchronous operations are disallowed. Call ReadAsync or set AllowSynchronousIO to true instead.”
                                data = new byte[(int)request.ContentLength];
                                request.InputStream.Read(data, 0, data.Length);
                            }
                            bytes = wc.UploadData(url, request.HttpMethod, data);
                        }
                        try
                        {
                            //context.Response.AppendHeader("Content-Length", bytes.Length.ToString());
                            foreach (string key in wc.ResponseHeaders.Keys)
                            {
                                switch (key)
                                {
                                    case "Transfer-Encoding"://输出这个会造成时不时的503
                                        continue;
                                }
                                if (key == "Content-Type" && wc.ResponseHeaders[key].Split(';').Length == 1)
                                {
                                    continue;
                                }
                                context.Response.AppendHeader(key, wc.ResponseHeaders[key]);
                            }

                        }
                        catch
                        {

                        }
                    }
                    context.Response.BinaryWrite(bytes);
                    return true;
                }
                catch (Exception err)
                {
                    LogWrite(err.Message, url, request.HttpMethod, isServerCall ? Config.ServerName : Config.ClientName);
                    return false;
                }
            }


            /// <summary>
            /// 清理服务主机。
            /// </summary>
            internal static void ClearServerTable()
            {
                while (true)
                {
                    try
                    {
                        lock (Const.tableLockObj)
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
                                        IO.Write(Const.ServerHostListJsonPath, Server._HostListJson);
                                    }
                                    else
                                    {
                                        Server._HostListJson = String.Empty;
                                        IO.Delete(Const.ServerHostListJsonPath);
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
                        LogWrite(err.Message, "MicroService.Run.ClearServerTable()", "", Config.ServerName);
                    }
                    Thread.Sleep(5000);//测试并发。
                }
            }
            #endregion
        }
    }

}
