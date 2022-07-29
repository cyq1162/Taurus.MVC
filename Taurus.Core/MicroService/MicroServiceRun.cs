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
        internal class Run
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
                    if (string.IsNullOrEmpty(Config.ClientHost))
                    {
                        Config.ClientHost = host.ToLower();//设置当前程序运行的请求网址。
                    }
                    if (Server.IsMainRegCenter)
                    {
                        Thread thread = new Thread(new ThreadStart(ClearServerTable));
                        thread.Start();
                    }
                    if (!string.IsNullOrEmpty(Config.ServerHost))
                    {
                        if (Config.ServerHost.ToLower() == Config.ClientHost)
                        {
                            return;//主机指向自身时，不做任何处理。
                        }

                        if (!string.IsNullOrEmpty(Config.ServerName))
                        {
                            switch (Config.ServerName.ToLower())
                            {
                                case Const.RegCenter:
                                case Const.Gateway:
                                    Thread thread = new Thread(new ThreadStart(ServerRunByLoop));
                                    thread.Start();
                                    break;
                            }
                        }
                        if (!string.IsNullOrEmpty(Config.ClientName))
                        {
                            Thread thread = new Thread(new ThreadStart(ClientRunByLoop));
                            thread.Start();
                        }
                    }
                }
            }


            /// <summary>
            /// 网关、注册中心运行时。
            /// </summary>
            private static void ServerRunByLoop()
            {
                while (true)
                {
                    try
                    {
                        switch (Config.ServerName.ToLower())
                        {
                            case Const.Gateway://网关
                                AfterGetList(GetHostList(true), true);//仅读取服务列表
                                break;
                            case Const.RegCenter://注册中心（备用节点、走数据同步）
                                AfterRegHost2(RegHost2());
                                break;
                        }
                        Thread.Sleep(5000);
                    }
                    catch (Exception err)
                    {
                        Thread.Sleep(5000);
                        Log.Write(err.Message, "MicroService");
                    }
                }
            }
            /// <summary>
            /// 微服务模块运行时。
            /// </summary>
            private static void ClientRunByLoop()
            {
                while (true)
                {
                    try
                    {
                        AfterRegHost(RegHost());
                        Thread.Sleep(5000);
                    }
                    catch (Exception err)
                    {
                        Thread.Sleep(5000);
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
                    if (!string.IsNullOrEmpty(host) && host != Config.ServerHost)
                    {
                        Config.ServerHost = host;//从备份请求切回主程序
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
                            Server._Table = MDataTable.CreateFrom(json);
                            IO.Write(Const.ServerTablePath, json);
                        }
                        else
                        {
                            Client._Table = MDataTable.CreateFrom(json);
                            IO.Write(Const.ClientTablePath, json);
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
                    if (!string.IsNullOrEmpty(host) && host != Config.ServerHost)
                    {
                        Config.ServerHost = host;//从备份请求切回主程序
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
            /// 注册中心 - 地址注册。
            /// </summary>
            /// <returns></returns>
            private static string RegHost()
            {
                string url = Config.ServerHost + "/MicroService/Reg";
                try
                {

                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(Const.HeaderKey, Config.ServerKey);
                        wc.Headers.Add("Referer", Config.ClientHost);
                        string data = "name={0}&host={1}&version={2}";
                        string result = wc.UploadString(url, string.Format(data, Config.ClientName, Config.ClientHost, Config.ClientVersion));
                        Server.IsLive = true;
                        return result;
                    }
                }
                catch (Exception err)
                {
                    Server.IsLive = false;
                    if (!string.IsNullOrEmpty(Client.Host2))
                    {
                        Config.ServerHost = Client.Host2;//切换到备用库。
                    }
                    LogWrite(err.Message, url, "POST", Config.ClientName);
                    return err.Message;
                }
            }
            /// <summary>
            /// 注册中心-地址注册（备用）。
            /// </summary>
            /// <returns></returns>
            private static string RegHost2()
            {
                string url = Config.ServerHost + "/MicroService/Reg2";
                try
                {
                    string result = string.Empty;
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(Const.HeaderKey, Config.ServerKey);
                        wc.Headers.Add("Referer", Config.ClientHost);
                        string data = "host={0}&tick=" + Server.Tick;
                        result = wc.UploadString(url, string.Format(data, Config.ClientHost));
                    }
                    Server.IsLive = true;
                    return result;
                }
                catch (Exception err)
                {
                    Server.IsLive = false;
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
                string url = Config.ServerHost + "/MicroService/SyncList";
                try
                {

                    string data = string.Format("json={0}&tick=" + Server.Tick, Server.Table.ToJson(false, true));
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(Const.HeaderKey, Config.ServerKey);
                        wc.Headers.Add("Referer", Config.ClientHost);
                        wc.UploadString(url, data);
                    }
                    Server.IsLive = true;
                }
                catch (Exception err)
                {
                    Server.IsLive = false;
                    LogWrite(err.Message, url, "POST", Config.ServerName);
                }
            }
            /// <summary>
            /// 获取注册中心注册数据列表。
            /// </summary>
            /// <param name="isServer">请求端</param>
            internal static string GetHostList(bool isServer)
            {
                string url = Config.ServerHost + "/MicroService/GetList?tick=" + (isServer ? Server.Tick : Client.Tick);
                try
                {
                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Add(Const.HeaderKey, Config.ServerKey);
                        wc.Headers.Set("Referer", Config.ClientHost);
                        string result = wc.DownloadString(url);
                        Server.IsLive = true;
                        return result;
                    }
                }
                catch (Exception err)
                {
                    Server.IsLive = false;
                    if (isServer)
                    {
                        if (!string.IsNullOrEmpty(Server.Host2))
                        {
                            Config.ServerHost = Server.Host2;//切换到备用库。
                        }
                    }
                    else
                    {
                        if (!string.IsNullOrEmpty(Client.Host2))
                        {
                            Config.ServerHost = Client.Host2;//切换到备用库。
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
            internal static void Proxy(IController controller)
            {
                string module = controller.Module;
                if (string.IsNullOrEmpty(module))
                {
                    module = QueryTool.GetLocalPath().Trim('/').Split('/')[0];
                }
                MDataTable dt = MicroService.Server.GetHostList(module);
                if (dt == null || dt.Rows.Count == 0)
                {
                    string msg = module + " server is stopped.";
                    LogWrite(msg, "MicroService.Run.Proxy()", "", Config.ServerName);
                    controller.Write(msg);
                }
                else
                {
                    int max = 3;//最多循环3个节点，避免长时间循环卡机。
                    foreach (MDataRow row in dt.Rows)
                    {
                        max--;
                        string host = row.Get<string>("host");
                        if (Proxy(controller, host))
                        {
                            row.Set("calltime", DateTime.Now);
                            return;
                        }
                        else
                        {
                            row.Set("calltime", DateTime.Now.AddMinutes(1));//网络异常的，延时1分钟检测。
                        }
                        if (max == 0)
                        {
                            return;
                        }
                    }
                }
            }
            private static bool Proxy(IController controller, string host)
            {
                HttpRequest request = controller.Context.Request;
                string url = String.Empty;
                try
                {
                    byte[] bytes = null;

                    url = host + request.RawUrl;//.Substring(module.Length + 1);

                    using (WebClient wc = new WebClient())
                    {
                        wc.Headers.Set(Const.HeaderKey, Config.ServerKey);
                        wc.Headers.Set("X-Real-IP", request.UserHostAddress);
                        wc.Headers.Set("Referer", Config.ClientHost);
                        foreach (string key in request.Headers.Keys)
                        {
                            switch (key)
                            {
                                case "Connection"://引发异常 链接已关闭
                                case "Host"://引发请求地址错乱。
                                case "Accept-Encoding"://引发乱码
                                case "Accept"://引发下载类型错乱
                                case "Referer":
                                    break;
                                default:
                                    wc.Headers.Set(key, request.Headers[key]);
                                    break;
                            }
                        }
                        if (controller.IsHttpGet)
                        {
                            bytes = wc.DownloadData(url);
                        }
                        else
                        {
                            byte[] data = null;
                            if (controller.IsHttpPost && request.ContentLength > 0)
                            {
                                data = new byte[(int)request.ContentLength];
                                request.InputStream.Read(data, 0, data.Length);
                            }
                            bytes = wc.UploadData(url, request.HttpMethod, data);
                        }
                        try
                        {
                            foreach (string key in wc.ResponseHeaders.Keys)
                            {
                                controller.Context.Response.Headers.Set(key, wc.ResponseHeaders[key]);
                            }
                        }
                        catch
                        {

                        }
                    }
                    controller.Write(bytes);
                    return true;
                }
                catch (Exception err)
                {
                    LogWrite(err.Message, url, request.HttpMethod, Config.ServerName);
                    controller.Write(err.Message);
                    return false;
                }
            }
            /// <summary>
            /// 清理服务主机。
            /// </summary>
            public static void ClearServerTable()
            {
                while (true)
                {
                    try
                    {
                        if (Server.Table.Rows.Count > 0)
                        {
                            string where = string.Format("time<'{0}'", DateTime.Now.AddSeconds(-30));
                            MDataRowCollection rows = Server.Table.FindAll(where);
                            if (rows != null && rows.Count > 0)
                            {
                                foreach (MDataRow row in rows)
                                {
                                    Server.Table.Rows.Remove(row);
                                }
                                Server.Tick = DateTime.Now.Ticks;
                            }

                        }
                    }
                    catch (Exception err)
                    {
                        LogWrite(err.Message, "MicroService.Run.ClearServerTable()", "", Config.ServerName);
                    }
                    Thread.Sleep(30000);
                }
            }

            /// <summary>
            /// 日志记录
            /// </summary>
            private static void LogWrite(string msg, string url, string httpMethod, string moduleName)
            {
                SysLogs sysLogs = new SysLogs();
                sysLogs.LogType = "MicroService";
                sysLogs.Message = msg;
                sysLogs.PageUrl = url;
                sysLogs.HttpMethod = httpMethod;
                sysLogs.ClientIP = sysLogs.Host;
                sysLogs.Host = Config.ClientHost;
                sysLogs.HostName = moduleName;
                sysLogs.Write();
            }
            #endregion
        }
    }
}
