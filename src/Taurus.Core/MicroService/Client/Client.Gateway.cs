using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.MicroService
{
    /// <summary>
    /// 微服务应用程序编码
    /// </summary>
    internal partial class Client
    {
        /// <summary>
        /// 客户端 - 网关数据
        /// </summary>
        public class Gateway
        {
            private static string _HostListJson = null;

            /// <summary>
            /// 返回的注册数据
            /// </summary>
            internal static string HostListJson
            {
                get
                {
                    if (_HostListJson == null)
                    {
                        _HostListJson = IO.Read(MsConst.ClientGatewayJsonPath);
                    }
                    return _HostListJson;
                }
                set
                {
                    _HostListJson = value;
                    if (string.IsNullOrEmpty(value))
                    {
                        IO.Delete(MsConst.ClientGatewayJsonPath);
                    }
                    else
                    {
                        IO.Write(MsConst.ClientGatewayJsonPath, value);
                    }

                }
            }
            private static MDictionary<string, List<HostInfo>> _HostList;
            /// <summary>
            /// 从微服务主程序端获取的微服务列表【用于微服务间内部调用运转】
            /// </summary>
            public static MDictionary<string, List<HostInfo>> HostList
            {
                get
                {
                    return _HostList;
                }
                set { _HostList = value; }
            }

            /// <summary>
            /// 获取模块所在的对应主机网址【若存在多个：每次获取都会循环下一个】。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <returns></returns>
            public static string GetHost(string name)
            {
                List<HostInfo> infoList = GetHostList(name);
                if (infoList != null && infoList.Count > 0)
                {
                    HostInfo firstInfo = infoList[0];
                    for (int i = 0; i < infoList.Count; i++)
                    {
                        int callIndex = firstInfo.CallIndex + i;
                        if (callIndex >= infoList.Count)
                        {
                            callIndex = 0;
                        }
                        HostInfo info = infoList[callIndex];

                        if (info.Version < 0)//正常5-10秒注册1次。
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
                return GetHostList(name, true);
            }
            /// <summary>
            /// 获取模块的所有Host列表。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <param name="withStar">是否包含星号通配符（默认true）</param>
            /// <returns></returns>
            public static List<HostInfo> GetHostList(string name, bool withStar)
            {
                var hostList = HostList;//先获取引用【避免执行过程，因线程更换了引用的对象】
                if (hostList != null)
                {
                    if (string.IsNullOrEmpty(name))
                    {
                        name = "/";
                    }
                    List<HostInfo> list = new List<HostInfo>();
                    if (hostList.ContainsKey(name))//微服务程序。
                    {
                        list.AddRange(hostList[name]);
                    }
                    if (withStar)
                    {
                        if (name.Contains("."))//域名
                        {
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
                            switch (name)
                            {
                                case "*":
                                case "RegCenter":
                                case "RegCenterOfSlave":
                                case "Gateway":
                                    return list;
                            }
                            if (hostList.ContainsKey("*"))
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
                    }
                    return list;
                }
                return null;
            }
        }
    }
}
