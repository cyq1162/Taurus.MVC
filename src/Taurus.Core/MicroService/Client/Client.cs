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
        #region 对外开放的方法或属性

        /// <summary>
        /// 微服务应用程序 - 检测注册中心是否安在。
        /// </summary>
        public static bool RegCenterIsLive = false;

        #endregion
        /// <summary>
        /// 读取：注册中心时的最后更新标识.
        /// </summary>
        internal static long Tick = 0;//从注册中心读取的标识号【用于发送做对比】
        /// <summary>
        /// 读取：注册中心【存档】故障转移备用链接。
        /// </summary>
        internal static string _Host2 = null;
        /// <summary>
        /// 读取：从注册中心读取的备用链接
        /// </summary>
        internal static string Host2
        {
            get
            {
                if (_Host2 == null)
                {
                    _Host2 = IO.Read(MsConst.ClientHost2Path);//首次读取，以便于恢复。
                }
                return _Host2;
            }
            set
            {
                _Host2 = value;
            }
        }
        /// <summary>
        /// 注册数据是否发生改变
        /// </summary>
        internal static bool IsChange = false;
        internal static string _HostListJson = null;

        /// <summary>
        /// 返回的注册数据
        /// </summary>
        internal static string HostListJson
        {
            get
            {
                if (_HostListJson == null)
                {
                    _HostListJson = IO.Read(MsConst.ClientHostListJsonPath);
                }
                return _HostListJson;
            }
            set
            {
                _HostListJson = value;
                IsChange = true;
                if (string.IsNullOrEmpty(value))
                {
                    IO.Delete(MsConst.ClientHostListJsonPath);
                }
                else
                {
                    IO.Write(MsConst.ClientHostListJsonPath, value);
                }

            }
        }

        internal static MDictionary<string, List<HostInfo>> _HostList;
        /// <summary>
        /// 从微服务主程序端获取的微服务列表【用于微服务间内部调用运转】
        /// </summary>
        public static MDictionary<string, List<HostInfo>> HostList
        {
            get
            {
                if (_HostList == null || IsChange)
                {
                    IsChange = false;
                    _HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(HostListJson);

                    if (_HostList == null)
                    {
                        _HostList = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                    }
                }
                return _HostList;
            }
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
            if (!string.IsNullOrEmpty(name))
            {
                List<HostInfo> list = new List<HostInfo>();
                if (HostList.ContainsKey(name))//微服务程序。
                {
                    list.AddRange(HostList[name]);
                }
                if (name.Contains("."))//域名
                {
                    if (name != "*.*" && HostList.ContainsKey("*.*"))
                    {
                        List<HostInfo> commList = HostList["*.*"];
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
                    if (name != "*" && HostList.ContainsKey("*"))
                    {
                        List<HostInfo> commList = HostList["*"];
                        if (commList.Count > 0)
                        {
                            if (list.Count == 0 || commList[0].Version >= list[0].Version)//版本号比较处理
                            {
                                list.AddRange(commList);//增加“*”模块的通用符号处理。
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
