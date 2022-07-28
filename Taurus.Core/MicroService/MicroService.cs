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

            internal static MDataTable _Table;
            /// <summary>
            /// 作为微服务主程序时，存档的微服务列表
            /// </summary>
            public static MDataTable Table
            {
                get
                {
                    if (_Table == null)
                    {
                        string json = IO.Read(MicroService.Const.ServerTablePath);
                        if (!string.IsNullOrEmpty(json))//数据恢复。
                        {
                            _Table = MDataTable.CreateFrom(json);
                            if (_Table.Columns.Contains("time"))
                            {
                                _Table.Columns["time"].Set(DateTime.Now);//恢复时间，避免被清除。
                            }
                        }
                        else
                        {
                            _Table = new MDataTable();
                            _Table.Columns.Add("name,host");
                            _Table.Columns.Add("version", System.Data.SqlDbType.Int);
                            _Table.Columns.Add("time", System.Data.SqlDbType.DateTime);
                            _Table.Columns.Add("calltime", System.Data.SqlDbType.DateTime);
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
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.Split('.')[0];
                    if (_Table != null && _Table.Rows.Count > 0)//微服务程序。
                    {
                        string time = IsMainRegCenter ? string.Format(" and time>'{0}',", DateTime.Now.AddSeconds(-10)) : "";
                        string where = string.Format("name='{0}' {1} order by calltime asc", name, time);
                        MDataRow row = _Table.FindRow(where);
                        if (row != null)
                        {
                            row.Set("calltime", DateTime.Now);//触发负载均衡
                            return row.Get<string>("host");
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
            public static MDataTable GetHostList(string name)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.Split('.')[0];
                    if (_Table != null && _Table.Rows.Count > 0)//微服务程序。
                    {
                        string time = IsMainRegCenter ? string.Format(" and time>'{0}',", DateTime.Now.AddSeconds(-10)) : "";
                        string where = string.Format("name='{0}' {1} order by calltime asc", name, time);
                        return _Table.FindAll(where);
                    }
                }
                return null;
            }

            /// <summary>
            /// 检测是否在微服务程序中。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <returns></returns>
            public static bool Contains(string name)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.Split('.')[0];

                    if (Server._Table != null && Server._Table.Rows.Count > 0)//注册中心主程序作为网关时。
                    {
                        string time = IsMainRegCenter ? string.Format(" and time>'{0}',", DateTime.Now.AddSeconds(-10)) : "";
                        string where = string.Format("name='{0}' {1}", name, time);
                        if (Server._Table.FindRow(where) != null)
                        {
                            return true;
                        }
                    }
                }
                return false;
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
            internal static MDataTable _Table;
            /// <summary>
            /// 从微服务主程序端获取的微服务列表【用于微服务间内部调用运转】
            /// </summary>
            public static MDataTable Table
            {
                get
                {
                    if (_Table == null)
                    {
                        string json = IO.Read(MicroService.Const.ClientTablePath);
                        _Table = MDataTable.CreateFrom(json);
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
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.Split('.')[0];
                    if (_Table != null && _Table.Rows.Count > 0)//微服务程序。
                    {
                        string where = string.Format("name='{0}' order by calltime asc", name);//触发负载均衡
                        MDataRow row = _Table.FindRow(where);
                        if (row != null)
                        {
                            row.Set("calltime", DateTime.Now);//触发负载均衡
                            return row.Get<string>("host");
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
            public static MDataTable GetHostList(string name)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.Split('.')[0];
                    if (_Table != null && _Table.Rows.Count > 0)//微服务程序。
                    {
                        string where = string.Format("name='{0}' order by calltime asc", name);
                        return _Table.FindAll(where);
                    }
                }
                return null;
            }
            /// <summary>
            /// 检测模块是否在微服务程序中。
            /// </summary>
            /// <param name="name">服务模块名称</param>
            /// <returns></returns>
            public static bool Contains(string name)
            {
                if (!string.IsNullOrEmpty(name))
                {
                    name = name.Split('.')[0];
                    if (Client._Table != null && Client._Table.Rows.Count > 0)//网关、微服务程序
                    {
                        string where = string.Format("name='{0}'", name);
                        return Client._Table.FindRow(where) != null;
                    }
                }
                return false;
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
