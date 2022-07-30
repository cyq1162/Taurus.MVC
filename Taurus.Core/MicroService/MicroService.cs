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
        /// 存档请求的客户端信息
        /// </summary>
        public class HostInfo
        {
            public string Host { get; set; }
            public int Version { get; set; }
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
        /// 常量
        /// </summary>
        public class Const
        {
            /// <summary>
            /// 请求头带上的Header的Key名称
            /// </summary>
            public const string HeaderKey = "microservice";
            /// <summary>
            /// 网关
            /// </summary>
            public const string Gateway = "gateway";
            /// <summary>
            /// 注册中心
            /// </summary>
            public const string RegCenter = "regcenter";

            internal const string ServerTablePath = "MicroService_Server_Table.json";
            internal const string ServerHost2Path = "MicroService_Server_Host2.json";
            internal const string ClientTablePath = "MicroService_Client_Table.json";
            internal const string ClientHost2Path = "MicroService_Client_Host2.json";
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
            internal static string _TableJson = String.Empty;
            /// <summary>
            /// 注册中心 - 返回的表数据Json
            /// </summary>
            internal static string TableJson
            {
                get
                {
                    if (string.IsNullOrEmpty(_TableJson) && IsChange && _Table != null && _Table.Count > 0)
                    {
                        IsChange = false;
                        lock (tableLockObj)
                        {
                            _TableJson = JsonHelper.ToJson(Table);
                        }
                        IO.Write(Const.ServerTablePath, _TableJson);
                    }
                    return _TableJson;
                }
                set
                {
                    _TableJson = value;
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


            internal static MDictionary<string, List<HostInfo>> _Table;
            /// <summary>
            /// 作为微服务主程序时，存档的微服务列表
            /// </summary>
            public static MDictionary<string, List<HostInfo>> Table
            {
                get
                {
                    if (_Table == null)
                    {
                        string json = IO.Read(MicroService.Const.ServerTablePath);
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
                            _Table = keys;
                            #endregion
                        }
                        if (_Table == null)
                        {
                            _Table = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                        }
                    }
                    return _Table;
                }
            }
            /// <summary>
            /// 获取模块所在的对应主机网址。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <returns></returns>
            public static string GetHost(string name)
            {
                //if (!string.IsNullOrEmpty(name))
                //{
                //    if (_Table != null && _Table.ContainsKey(name))//微服务程序。
                //    {
                //        List<ClientInfo> list = _Table[name];
                //        if (list.Count > 0)
                //        {
                //            var firstInfo = list[0];
                //            firstInfo.CallTime = DateTime.Now;
                //            if (firstInfo.CallIndex >= list.Count)
                //            {
                //                firstInfo.CallIndex = 1;
                //                return firstInfo.Host;
                //            }
                //            else
                //            {
                //                firstInfo.CallIndex++;
                //                return list[firstInfo.CallIndex - 1].Host;
                //            }
                //        }
                //    }
                //}
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
                    if (_Table != null && _Table.ContainsKey(name))//微服务程序。
                    {
                        return _Table[name];
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
            internal static MDictionary<string, List<HostInfo>> _Table;
            /// <summary>
            /// 从微服务主程序端获取的微服务列表【用于微服务间内部调用运转】
            /// </summary>
            public static MDictionary<string, List<HostInfo>> Table
            {
                get
                {
                    if (_Table == null)
                    {
                        string json = IO.Read(MicroService.Const.ClientTablePath);
                        _Table = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
                        if (_Table == null)
                        {
                            _Table = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                        }
                    }
                    return _Table;
                }
            }

            /// <summary>
            /// 获取模块所在的对应主机网址。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <returns></returns>
            public static string GetHost(string name)
            {
                //if (!string.IsNullOrEmpty(name))
                //{
                //    if (_Table != null && _Table.Rows.Count > 0)//微服务程序。
                //    {
                //        string where = string.Format("name='{0}' order by calltime asc", name);//触发负载均衡
                //        MDataRow row = _Table.FindRow(where);
                //        if (row != null)
                //        {
                //            row.Set("calltime", DateTime.Now);//触发负载均衡
                //            return row.Get<string>("host");
                //        }
                //    }
                //}
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
                    if (_Table != null && _Table.ContainsKey(name))//微服务程序。
                    {
                        return _Table[name];
                    }
                }
                return null;
            }
        }


        /// <summary>
        /// 对应【AppSettings】可配置项
        /// </summary>
        public class Config
        {
            #region AppSetting 配置

            /// <summary>
            /// 微服务：服务端模块名称【可配置：GateWay或RegCenter】
            /// </summary>
            public static string ServerName
            {
                get
                {
                    return AppConfig.GetApp("MicroService.ServerName");
                }
                set
                {
                    AppConfig.SetApp("MicroService.ServerName", value);
                }
            }

            /// <summary>
            /// 微服务：注册中心地址【示例：http://localhost:9999】
            /// </summary>
            public static string ServerHost
            {
                get
                {
                    return AppConfig.GetApp("MicroService.ServerHost");
                }
                set
                {
                    AppConfig.SetApp("MicroService.ServerHost", value);
                }
            }
            /// <summary>
            /// 微服务：系统间调用密钥串【任意字符串】
            /// </summary>
            public static string ServerKey
            {
                get
                {
                    return AppConfig.GetApp("MicroService.ServerKey", "Taurus.MicroService.Key");
                }
                set
                {
                    AppConfig.SetApp("MicroService.ServerKey", value);
                }
            }
            /// <summary>
            /// 微服务：客户端模块名称【示例：Test】
            /// </summary>
            public static string ClientName
            {
                get
                {
                    return AppConfig.GetApp("MicroService.ClientName");
                }
                set
                {
                    AppConfig.SetApp("MicroService.ClientName", value);
                }
            }

            /// <summary>
            /// 微服务：当前运行Host【可不配置，系统自动读取】
            /// </summary>
            public static string ClientHost
            {
                get
                {
                    return AppConfig.GetApp("MicroService.ClientHost");
                }
                set
                {
                    AppConfig.SetApp("MicroService.ClientHost", value);
                }
            }

            /// <summary>
            /// 微服务：客户端模块版本号（用于版本间升级）【示例：1】
            /// </summary>
            public static int ClientVersion
            {
                get
                {
                    return AppConfig.GetAppInt("MicroService.ClientVersion", 1);
                }
                set
                {
                    AppConfig.SetApp("MicroService.ClientVersion", value.ToString());
                }
            }
            #endregion
        }
    }
}
