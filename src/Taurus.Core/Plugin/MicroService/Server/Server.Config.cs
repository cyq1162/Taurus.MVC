using CYQ.Data;
using Taurus.Mvc;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 对应【AppSettings】可配置项
    /// </summary>
    public static partial class MsConfig
    {
        /// <summary>
        /// 服务端配置【网关或注册中心】
        /// </summary>
        public static class Server
        {
            /// <summary>
            /// 配置是否启用微服务服务端功能 
            /// 如 MicroService.Server.IsEnable ：true
            /// </summary>
            public static bool IsEnable
            {
                get
                {
                    return AppConfig.GetAppBool("MicroService.Server.IsEnable", MsConfig.IsServer);
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.IsEnable", value.ToString());
                }
            }
            /// <summary>
            /// 网关或注册中心配置：服务端模块名称【可配置：Gateway或RegCenter】
            /// 如 MicroService.Server.Name ： "RegCenter"
            /// </summary>
            public static string Name
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
            /// 网关或注册中心配置：注册中心地址
            /// 如 MicroService.Server.RcUrl ： "http://localhost:8000"
            /// </summary>
            public static string RcUrl
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Server.RcUrl", "").TrimEnd('/');
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.RcUrl", value);
                }
            }

            /// <summary>
            /// 配置注册中心的访问路径
            /// 如 MicroService.Server.RcPath ： "/microservice"， 默认值：/microservice
            /// </summary>
            public static string RcPath
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Server.RcPath", "/microservice");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.RcPath", value);
                }
            }
            /// <summary>
            /// 网关或注册中心配置：系统间调用密钥串【任意字符串】
            /// 如 MicroService.Server.RcKey ： "Taurus.Plugin.MicroService"， 默认值：Taurus.Plugin.MicroService
            /// </summary>
            public static string RcKey
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Server.RcKey", "Taurus.MicroService");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.RcKey", value);
                }
            }
            /// <summary>
            /// 应用配置：开启应用程序远程同步IP功能
            /// </summary>
            public static bool IsAllowSyncIP
            {
                get
                {
                    return AppConfig.GetAppBool("MicroService.Server.IsAllowSyncIP", IsClient);
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.IsAllowSyncIP", value.ToString());
                }
            }
            /// <summary>
            /// 网关：接收请求（大文件上传时需要根据情况重新设置）超时时间，单位秒（s）
            /// 默认：10（s）
            /// </summary>
            public static int GatewayTimeout
            {
                get
                {
#if DEBUG
                    return AppConfig.GetAppInt("MicroService.Server.GatewayTimeout", 120);
#endif

                    return AppConfig.GetAppInt("MicroService.Server.GatewayTimeout", 10);
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.GatewayTimeout", value.ToString());
                }
            }
        }
    }
}
