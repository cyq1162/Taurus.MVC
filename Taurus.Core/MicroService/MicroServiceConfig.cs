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

            internal const string ServerHostListJsonPath = "MicroService_Server_HostList.json";
            internal const string ClientHostListJsonPath = "MicroService_Client_HostList.json";
            internal const string ServerHost2Path = "MicroService_Server_Host2.json";
            internal const string ClientHost2Path = "MicroService_Client_Host2.json";
        }
    }
}
