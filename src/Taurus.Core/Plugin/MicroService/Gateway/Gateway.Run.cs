using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Policy;
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
    public partial class Gateway
    {
        internal static partial class Run
        {
            private static bool IsAllowToRun
            {
                get
                {
                    return MsConfig.Server.IsEnable && MsConfig.IsGateway && !string.IsNullOrEmpty(MsConfig.Server.RcUrl);
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
                        MsLog.WriteDebugLine("MicroService Type ：Gateway");
                        MsLog.WriteDebugLine("RegistryCenter Url：" + MsConfig.Server.RcUrl);
                        MsLog.WriteDebugLine("--------------------------------------------------");
                        //持续时间长、不占用线程队列。
                        new Thread(new ThreadStart(RunLoopOfGateway)).Start();
                    }
                }
            }
            /// <summary>
            /// 网关 - 运行
            /// </summary>
            private static void RunLoopOfGateway()
            {
                try
                {
                    //初始化文件配置
                    InitGatewayHostList();
                    while (IsAllowToRun)
                    {
                        string url = MsConfig.Server.RcUrl + MsConfig.Server.RcPath + "/getlist?tick=" + Info.Tick;
                        url += "&isGateway=1&pid=" + MvcConst.ProcessID;
                        AfterGetListOfGateway(GetListOfGateway(url), url);
                        Thread.Sleep(new Random().Next(5000, 10000));//5-10秒循环1次。
                    }
                }
                catch (Exception err)
                {
                    Log.Write(err.Message, "MicroService");
                }
                finally
                {
                    threadIsWorking = false;
                }
            }


            private static bool hasInit = false;
            /// <summary>
            /// 初始化 网关 -  数据。
            /// </summary>
            private static void InitGatewayHostList()
            {
                try
                {
                    if (!hasInit)
                    {
                        hasInit = true;
                        string hostListJson = IO.Read(MsConst.ServerGatewayJsonPath);
                        if (!string.IsNullOrEmpty(hostListJson))
                        {
                            var dic = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(hostListJson);
                            PreConnection(dic);
                            Server.HostList = dic;
                        }
                    }
                }
                catch (Exception err)
                {
                    MsLog.Write(err);
                }

            }

            private static void AfterGetListOfGateway(string result, string url)
            {
                try
                {
                    if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
                    {
                        Info.IsLiveOfMasterRC = true;

                        #region IP 黑名单同步

                        long ipTick = JsonHelper.GetValue<long>(result, "iptick");
                        SyncIPList(ipTick);
                        #endregion

                        #region 从备份请求切回主程序
                        string host = JsonHelper.GetValue<string>(result, "host");
                        if (!string.IsNullOrEmpty(host) && host != MsConfig.Server.RcUrl)
                        {
                            MsConfig.Server.RcUrl = host;
                        }
                        Info.Host2 = JsonHelper.GetValue<string>(result, "host2");
                        #endregion

                        long tick = JsonHelper.GetValue<long>(result, "tick");
                        if (Info.Tick > tick) { return; }
                        Info.Tick = tick;
                        string json = JsonHelper.GetValue<string>(result, "msg");
                        if (!string.IsNullOrEmpty(json))
                        {
                            var keyValues = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                            PreConnection(keyValues);
                            Gateway.Server.HostList = keyValues;
                            IO.Write(MsConst.ServerGatewayJsonPath, json);
                        }
                    }
                    else
                    {
                        Info.IsLiveOfMasterRC = false;
                        if (!string.IsNullOrEmpty(Info.Host2))
                        {
                            //主备切换。
                            string rcUrl = MsConfig.Server.RcUrl + "";//避开引用
                            MsConfig.Server.RcUrl = Info.Host2;
                            Info.Host2 = rcUrl;
                        }
                        MsLog.Write(result, url, "POST");
                    }
                }
                catch (Exception err)
                {
                    MsLog.Write(err);
                }
            }

            /// <summary>
            /// 获取注册中心注册数据列表。
            /// </summary>
            internal static string GetListOfGateway(string url)
            {
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
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList : Tick : {1}  => OK", MvcConst.ProcessID, Info.Tick));
                    return result;
                }
                catch (Exception err)
                {
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : PID : {0} GetList.Error : {1}", MvcConst.ProcessID, err.Message));
                    MsLog.Write(err.Message, url, "GET");
                    return err.Message;
                }

            }
        }
    }

}
