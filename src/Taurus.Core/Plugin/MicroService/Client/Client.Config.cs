using CYQ.Data;
namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 对应【AppSettings】可配置项
    /// </summary>
    public static partial class MsConfig
    {
        /// <summary>
        /// 微服务客户端配置
        /// </summary>
        public static class Client
        {
            /// <summary>
            /// 是否退出应用程序
            /// </summary>
            internal static bool IsExitApplication = false;
            /// <summary>
            /// 配置是否启用微服务客户端功能 
            /// 如 MicroService.Client.IsEnable ：true
            /// </summary>
            public static bool IsEnable
            {
                get
                {
                    return AppConfig.GetAppBool("MicroService.Client.IsEnable", MsConfig.IsClient);
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.IsEnable", value.ToString());
                }
            }
            /// <summary>
            /// 微服务应用配置：客户端模块名称【示例：Test】
            /// </summary>
            public static string Name
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
            /// 微服务应用配置：客户端模块绑定域名【示例：www.cyqdata.com】
            /// </summary>
            public static string Domain
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Client.Domain", "");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.Domain", value);
                }
            }

            /// <summary>
            /// 微服务应用配置：客户端模块版本号（用于版本间升级）【示例：1】
            /// </summary>
            public static int Version
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
            /// 微服务应用配置：是否虚拟路径【，默认false,为true时，name名称不转发过来】
            /// </summary>
            public static bool IsVirtual
            {
                get
                {
                    return AppConfig.GetAppBool("MicroService.Client.IsVirtual", false);
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.IsVirtual", value.ToString());
                }
            }

            /// <summary>
            /// 应用配置：开启应用程序远程退出功能【是否允许注册中心控制客户端退出】
            /// </summary>
            public static bool IsAllowRemoteExit
            {
                get
                {
                    return AppConfig.GetAppBool("MicroService.Client.IsAllowRemoteExit", IsClient);
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.IsAllowRemoteExit", value.ToString());
                }
            }
            /// <summary>
            /// 微服务应用配置：注册中心的Url
            /// 如 MicroService.Client.RcUrl ： "http://192.168.9.121:8000"
            /// </summary>
            public static string RcUrl
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Client.RcUrl", "").TrimEnd('/');
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.RcUrl", value);
                }
            }
            /// <summary>
            /// 配置注册中心的访问路径
            /// 如 MicroService.Client.RcPath ： "/microservice"， 默认值：/microservice
            /// </summary>
            public static string RcPath
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Client.RcPath", "/microservice");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.RcPath", value);
                }
            }
            /// <summary>
            /// 微服务应用配置：系统间调用密钥串【任意字符串】
            /// 如 MicroService.Client.RcKey ： "Taurus.Plugin.MicroService"， 默认值：Taurus.Plugin.MicroService
            /// </summary>
            public static string RcKey
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Client.RcKey", "Taurus.MicroService");
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.RcKey", value);
                }
            }
        }

    }
}
