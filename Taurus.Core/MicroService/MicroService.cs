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
        internal static readonly object tableLockObj = new object();
        #region 公共区域
        /// <summary>
        /// 日志记录
        /// </summary>
        internal static void LogWrite(string msg, string url, string httpMethod, string moduleName)
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
        
        /// <summary>
        /// 当前程序是否作为服务端运行
        /// </summary>
        public static bool IsServer
        {
            get
            {
                string name = Config.ServerName.ToLower();
                return !string.IsNullOrEmpty(name) && (name == Const.RegCenter || (name == Const.Gateway && !string.IsNullOrEmpty(Config.ServerHost) && Config.ServerHost != Config.ClientHost));
            }
        }
        /// <summary>
        /// 当前程序是否作为客务端运行
        /// </summary>
        public static bool IsClient
        {
            get
            {
                return !string.IsNullOrEmpty(Config.ClientName) && !string.IsNullOrEmpty(Config.ServerHost) && Config.ServerHost != Config.ClientHost;
            }
        }
        /// <summary>
        /// 存档请求的客户端信息
        /// </summary>
        public class HostInfo
        {
            /// <summary>
            /// 主机地址：http://localhost:8080
            /// </summary>
            public string Host { get; set; }
            /// <summary>
            /// 版本号：用于版本升级。
            /// </summary>
            public int Version { get; set; }
            /// <summary>
            /// 注册时间（最新）
            /// </summary>
            public DateTime RegTime { get; set; }
            /// <summary>
            /// 记录调用时间，用于隔离无法调用的服务，延时调用。
            /// </summary>
            public DateTime CallTime { get; set; }
            /// <summary>
            /// 记录调用顺序，用于负载均衡
            /// </summary>
            public int CallIndex { get; set; }

        }
        /// <summary>
        /// 定义安全路径，防止存档数据被直接访问。
        /// </summary>
        internal class IO
        {
            public static void Write(string path, string text)
            {
                path = AppConfig.WebRootPath + "/App_Data/" + path;
                IOHelper.Write(path, text);
            }

            public static string Read(string path)
            {
                path = AppConfig.WebRootPath + "/App_Data/" + path;
                return IOHelper.ReadAllText(path);
            }
            public static void Delete(string path)
            {
                path = AppConfig.WebRootPath + "/App_Data/" + path;
                IOHelper.Delete(path);
            }
        }
      

        /// <summary>
        /// 服务端【网关Gateway或注册中心RegCenter】相关
        /// </summary>
        public class Server
        {
            /// <summary>
            /// 是否注册中心（主）
            /// </summary>
            internal static bool IsMainRegCenter
            {
                get
                {
                    return Config.ServerName.ToLower() == Const.RegCenter && (string.IsNullOrEmpty(Config.ServerHost) || Config.ServerHost == Config.ClientHost);
                }
            }
            /// <summary>
            /// 注册中心备份 - 检测状态。
            /// </summary>
            internal static bool IsLive = false;
            /// <summary>
            /// 作为注册中心时的最后更新标识.
            /// </summary>
            internal static long Tick = 0;
            /// <summary>
            /// 注册中心 - 数据是否发生改变
            /// </summary>
            internal static bool IsChange = false;
            internal static string _HostListJson = String.Empty;
            /// <summary>
            /// 注册中心 - 返回的表数据Json
            /// </summary>
            internal static string HostListJson
            {
                get
                {
                    if (string.IsNullOrEmpty(_HostListJson) && IsChange && _HostList != null && _HostList.Count > 0)
                    {
                        IsChange = false;
                        lock (tableLockObj)
                        {
                            _HostListJson = JsonHelper.ToJson(HostList);
                        }
                        IO.Write(Const.ServerHostListJsonPath, _HostListJson);
                    }
                    return _HostListJson;
                }
                set
                {
                    _HostListJson = value;
                }
            }
            internal static string _Host2 = string.Empty;
            /// <summary>
            /// 注册中心【存档】故障转移备用链接。
            /// </summary>
            internal static string Host2
            {
                get
                {
                    if (_Host2 == null)
                    {
                        _Host2 = IO.Read(Const.ServerHost2Path);//首次读取，以便于恢复。
                    }
                    return _Host2;
                }
                set
                {
                    _Host2 = value;
                }
            }
            /// <summary>
            /// 备份链接最新注册时间
            /// </summary>
            internal static DateTime Host2LastRegTime = DateTime.MinValue;


            internal static MDictionary<string, List<HostInfo>> _HostList;
            /// <summary>
            /// 作为微服务主程序时，存档的微服务列表
            /// </summary>
            public static MDictionary<string, List<HostInfo>> HostList
            {
                get
                {
                    if (_HostList == null)
                    {
                        string json = IO.Read(MicroService.Const.ServerHostListJsonPath);
                        if (!string.IsNullOrEmpty(json))//数据恢复。
                        {
                            #region 从Json文件恢复数据
                            MDictionary<string, List<HostInfo>> keys = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                            if (keys != null && keys.Count > 0)
                            {
                                //恢复时间，避免被清除。
                                foreach (var item in keys)
                                {
                                    if (item.Value != null)
                                    {
                                        foreach (var ci in item.Value)
                                        {
                                            ci.RegTime = DateTime.Now;
                                        }
                                    }
                                }
                            }
                            _HostList = keys;
                            #endregion
                        }
                        if (_HostList == null)
                        {
                            _HostList = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                        }
                    }
                    return _HostList;
                }
            }
            /// <summary>
            /// 获取模块所在的对应主机网址。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <returns></returns>
            public static string GetHost(string name)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    if (_HostList != null && _HostList.ContainsKey(name))//微服务程序。
                    {
                        List<HostInfo> list = _HostList[name];
                        if (list != null && list.Count > 0)
                        {
                            HostInfo firstInfo = list[0];
                            if (firstInfo.CallIndex >= list.Count)
                            {
                                firstInfo.CallIndex = 1;//每次获取都自动标记下一位。
                                return firstInfo.Host;
                            }
                            else
                            {
                                firstInfo.CallIndex++;//每次获取都自动标记下一位。
                                return list[firstInfo.CallIndex - 1].Host;
                            }
                        }
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
                    if (_HostList != null && _HostList.ContainsKey(name))//微服务程序。
                    {
                        return _HostList[name];
                    }
                }
                return null;
            }
        }
        /// <summary>
        /// 客户端【微服务模块】相关
        /// </summary>
        public class Client
        {
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
                        _Host2 = IO.Read(Const.ClientHost2Path);//首次读取，以便于恢复。
                    }
                    return _Host2;
                }
                set
                {
                    _Host2 = value;
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
                    if (_HostList == null)
                    {
                        string json = IO.Read(MicroService.Const.ClientHostListJsonPath);
                        if (!string.IsNullOrEmpty(json))
                        {
                            _HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                        }
                        if (_HostList == null)
                        {
                            _HostList = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                        }
                    }
                    return _HostList;
                }
            }

            /// <summary>
            /// 获取模块所在的对应主机网址。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <returns></returns>
            public static string GetHost(string name)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    if (_HostList != null && _HostList.ContainsKey(name))//微服务程序。
                    {
                        List<HostInfo> list = _HostList[name];
                        if (list != null && list.Count > 0)
                        {
                            HostInfo firstInfo = list[0];
                            if (firstInfo.CallIndex >= list.Count)
                            {
                                firstInfo.CallIndex = 1;//每次获取都自动标记下一位。
                                return firstInfo.Host;
                            }
                            else
                            {
                                firstInfo.CallIndex++;//每次获取都自动标记下一位。
                                return list[firstInfo.CallIndex - 1].Host;
                            }
                        }
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
                    if (_HostList != null && _HostList.ContainsKey(name))//微服务程序。
                    {
                        return _HostList[name];
                    }
                }
                return null;
            }
        }
    }
}
