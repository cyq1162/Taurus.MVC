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
        /// 对应【AppSettings】可配置项
        /// </summary>
        public class Config
        {
            #region AppSetting 配置

            /// <summary>
            /// 网关或注册中心配置：服务端模块名称【可配置：GateWay或RegCenter】
            /// </summary>
            public static string ServerName
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Server.Name");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.Name", value);
                }
            }

            /// <summary>
            /// 网关或注册中心配置：注册中心地址【示例：http://localhost:9999】
            /// </summary>
            public static string ServerRegUrl
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Server.RegUrl");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.RegUrl", value);
                }
            }
            /// <summary>
            /// 网关或注册中心配置：系统间调用密钥串【任意字符串】
            /// </summary>
            public static string ServerKey
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Server.Key", "Taurus.MicroService");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.Key", value);
                }
            }
            /// <summary>
            /// 微服务应用配置：系统间调用密钥串【任意字符串】
            /// </summary>
            public static string ClientKey
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Client.Key", "Taurus.MicroService");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.Key", value);
                }
            }
            /// <summary>
            /// 微服务应用配置：客户端模块名称【示例：Test】
            /// </summary>
            public static string ClientName
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Client.Name");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.Name", value);
                }
            }

            /// <summary>
            /// 微服务应用配置：注册中心的Url
            /// </summary>
            public static string ClientRegUrl
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Client.RegUrl");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.RegUrl", value);
                }
            }

            /// <summary>
            /// 微服务应用配置：客户端模块版本号（用于版本间升级）【示例：1】
            /// </summary>
            public static int ClientVersion
            {
                get
                {
                    return AppConfig.GetAppInt("MicroService.Client.Version", 1);
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.Version", value.ToString());
                }
            }

            /// <summary>
            /// 应用配置：当前运行Url【Kestrel启动运行需要】
            /// </summary>
            public static string AppRunUrl
            {
                get
                {
                    return AppConfig.GetApp("MicroService.App.RunUrl");
                }
                set
                {
                    AppConfig.SetApp("MicroService.App.RunUrl", value);
                }
            }
            #endregion
        }

        /// <summary>
        /// 常量
        /// </summary>
        public class Const
        {
            internal static readonly object tableLockObj = new object();
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

            internal const string ServerHostListJsonPath = "MicroService_Server_HostList.json";
            internal const string ClientHostListJsonPath = "MicroService_Client_HostList.json";
            internal const string ServerHost2Path = "MicroService_Server_Host2.json";
            internal const string ClientHost2Path = "MicroService_Client_Host2.json";
        }
    }
}
