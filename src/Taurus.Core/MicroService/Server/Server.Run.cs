using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using System.Diagnostics;

namespace Taurus.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {

        /// <summary>
        /// 网关、注册中心运行时。
        /// </summary>
        private static void RunLoopOfServer(object threadID)
        {
            while (true)
            {
                try
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

        #region 注册中心 - 从 - 同步
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
        #endregion

        #region 网络请求

        private static void AfterGetListOfServer(string result)
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

                string json = JsonHelper.GetValue<string>(result, "msg");
                if (!string.IsNullOrEmpty(json))
                {
                    Server._HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                    IO.Write(MsConst.ServerHostListJsonPath, json);
                }
            }
        }

        /// <summary>
        /// 获取注册中心注册数据列表。
        /// </summary>
        internal static string GetListOfServer()
        {
            string url = MsConfig.ServerRegUrl + "/microservice/getlist?tick=" + Server.Tick;
            if (MsConfig.IsGateway)
            {
                url += "&isGateway=1";
            }
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.ServerKey);
                    wc.Headers.Set("Referer", MsConfig.AppRunUrl);
                    string result = wc.DownloadString(url);
                    Server.RegCenterIsLive = true;
                    MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : GetList OK : Tick : {0}", Server.Tick));
                    return result;
                }
            }
            catch (Exception err)
            {
                MsLog.WriteDebugLine(DateTime.Now.ToString("HH:mm:ss") + " : GetHostList Error : " + err.Message);
                Server.RegCenterIsLive = false;
                if (!string.IsNullOrEmpty(Server.Host2))
                {
                    MsConfig.ServerRegUrl = Server.Host2;//切换到备用库。
                }
                MsLog.Write(err.Message, url, "GET", MsConfig.ServerName);
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
                    if (table.Rows.Count > 0)
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
