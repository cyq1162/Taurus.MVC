﻿using CYQ.Data;
using CYQ.Data.Table;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;

namespace Taurus.MicroService
{
    /// <summary>
    /// 网关或注册中心端编码
    /// </summary>
    internal partial class Server
    {
        /// <summary>
        /// 分离出网关 - 与 注册中心原始存档隔离，避免同一对象在并发下需要锁而降低性能
        /// </summary>
        public class Gateway
        {
            private static string _HostListJson = null;

            /// <summary>
            /// 对于网关或注册中心（从） - 记录并备份返回的注册数据
            /// </summary>
            internal static string HostListJson
            {
                get
                {
                    if (_HostListJson == null)
                    {
                        _HostListJson = IO.Read(MsConst.ServerGatewayJsonPath);
                    }
                    return _HostListJson;
                }
                set
                {
                    _HostListJson = value;
                    if (string.IsNullOrEmpty(value))
                    {
                        IO.Delete(MsConst.ServerGatewayJsonPath);
                    }
                    else
                    {
                        IO.Write(MsConst.ServerGatewayJsonPath, value);
                    }
                }
            }
            private static MDictionary<string, List<HostInfo>> _HostList;
            /// <summary>
            /// 作为微服务主程序时，存档的微服务列表【和注册中心列表各自独立】
            /// </summary>
            public static MDictionary<string, List<HostInfo>> HostList
            {
                get
                {
                    return _HostList;
                }
                set
                {
                    _HostList = value;
                }
            }
            /// <summary>
            /// 获取模块所在的对应主机网址【若存在多个：每次获取都会循环下一个】。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <returns></returns>
            public static string GetHost(string name)
            {
                List<HostInfo> infoList = GetHostList(name);//微服务程序。
                if (infoList != null && infoList.Count > 0)
                {
                    // bool isRegCenter = MsConfig.IsRegCenterOfMaster;
                    //分拆出网关：网关列表，不具备注册时间（即不更新注册时间），所以取消该时间判断。
                    HostInfo firstInfo = infoList[0];
                    for (int i = 0; i < infoList.Count; i++)
                    {
                        int callIndex = firstInfo.CallIndex + i;
                        if (callIndex >= infoList.Count)
                        {
                            callIndex = 0;
                        }
                        HostInfo info = infoList[callIndex];

                        if (info.Version < 0 || (info.CallTime > DateTime.Now && infoList.Count > 0))//正常5-10秒注册1次: || (isRegCenter && info.RegTime < DateTime.Now.AddSeconds(-10))
                        {
                            continue;//已经断开服务的。
                        }
                        firstInfo.CallIndex = callIndex + 1;//指向下一个。
                        return infoList[callIndex].Host;
                    }
                }
                return string.Empty;
            }
            /// <summary>
            /// 获取模块的所有Host列表。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <returns></returns>
            public static List<HostInfo> GetHostList(string name)
            {
                //System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
                //sw.Start();
                var hostList = HostList;//先获取引用【避免执行过程，因线程更换了引用的对象】
                if (!string.IsNullOrEmpty(name) && hostList != null)
                {
                    List<HostInfo> list = new List<HostInfo>();
                    if (hostList.ContainsKey(name))//微服务程序。
                    {
                        list.AddRange(hostList[name]);
                    }
                    if (name.Contains("."))//域名
                    {
                        if (list.Count == 0 && name.Split('.').Length > 2)//2级泛域名检测
                        {
                            string seName = "*" + name.Substring(name.IndexOf("."));
                            if (hostList.ContainsKey(seName))
                            {
                                list.AddRange(hostList[seName]);
                            }
                        }
                        if (name != "*.*" && hostList.ContainsKey("*.*"))
                        {
                            List<HostInfo> commList = hostList["*.*"];
                            if (commList.Count > 0)
                            {
                                if (list.Count == 0 || commList[0].Version >= list[0].Version)//版本号比较处理
                                {
                                    list.AddRange(commList);//增加“*.*”模块的通用符号处理。
                                }
                            }
                        }
                    }
                    else //普通模块
                    {
                        if (name != "*" && hostList.ContainsKey("*"))
                        {
                            List<HostInfo> commList = hostList["*"];
                            if (commList.Count > 0)
                            {
                                if (list.Count == 0 || commList[0].Version >= list[0].Version)//版本号比较处理
                                {
                                    list.AddRange(commList);//增加“*”模块的通用符号处理。
                                }
                            }
                        }
                    }
                    //sw.Stop();
                    //if (sw.ElapsedMilliseconds > 1000)
                    //{
                    //    Log.WriteLogToTxt("GetHostList : " + sw.ElapsedMilliseconds, "DebugMS");
                    //}
                    return list;
                }
                return null;
            }
        }
    }
}