using CYQ.Data;
using Taurus.Mvc;

namespace Taurus.MicroService
{
    /// <summary>
    /// 对应【AppSettings】可配置项
    /// </summary>
    public static class MsConfig
    {
        /// <summary>
        /// 是否退出应用程序
        /// </summary>
        internal static bool IsApplicationExit = false;

        #region AppSetting 配置
        /// <summary>
        /// 服务端配置【网关或注册中心】
        /// </summary>
        public static class Server
        {
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
            /// 如 MicroService.Server.RcPath ： "microservice"， 默认值：microservice
            /// </summary>
            public static string RcPath
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Server.RcPath", "microservice").Trim('/');
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.RcPath", value);
                }
            }
            /// <summary>
            /// 网关或注册中心配置：系统间调用密钥串【任意字符串】
            /// 如 MicroService.Server.RcKey ： "Taurus.MicroService"， 默认值：Taurus.MicroService
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
            /// 网关：接收请求（大文件上传）超时时间，单位秒（s）
            /// 默认：60（s）
            /// </summary>
            public static int GatewayTimeout
            {
                get
                {
                    return AppConfig.GetAppInt("MicroService.Server.GatewayTimeout", 60);
                }
                set
                {
                    AppConfig.SetApp("MicroService.Server.GatewayTimeout", value.ToString());
                }
            }
        }
        /// <summary>
        /// 微服务客户端配置
        /// </summary>
        public static class Client
        {
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
            public static bool RemoteExit
            {
                get
                {
                    return AppConfig.GetAppBool("MicroService.Client.RemoteExit", IsClient);
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.RemoteExit", value.ToString());
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
            /// 如 MicroService.Client.RcPath ： "microservice"， 默认值：microservice
            /// </summary>
            public static string RcPath
            {
                get
                {
                    return AppConfig.GetApp("MicroService.Client.RcPath", "microservice").Trim('/');
                }
                set
                {
                    AppConfig.SetApp("MicroService.Client.RcPath", value);
                }
            }
            /// <summary>
            /// 微服务应用配置：系统间调用密钥串【任意字符串】
            /// 如 MicroService.Client.RcKey ： "Taurus.MicroService"， 默认值：Taurus.MicroService
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

        #endregion


        #region 只读属性

        /// <summary>
        /// 当前程序是否作为客务端运行：微服务应用程序
        /// </summary>
        public static bool IsClient
        {
            get
            {
                return !string.IsNullOrEmpty(MsConfig.Client.Name) && !string.IsNullOrEmpty(MsConfig.Client.RcUrl) && MsConfig.Client.RcUrl != MvcConfig.RunUrl;
            }
        }

        /// <summary>
        /// 当前程序是否作为服务端运行：网关或注册中心
        /// </summary>
        public static bool IsServer
        {
            get
            {
                return IsRegCenter || IsGateway;
            }
        }
        /// <summary>
        /// 是否注册中心
        /// </summary>
        public static bool IsRegCenter
        {
            get
            {
                return MsConfig.Server.Name.ToLower() == MsConst.RegCenter;
            }
        }
        /// <summary>
        /// 是否网关中心
        /// </summary>
        public static bool IsGateway
        {
            get
            {
                return MsConfig.Server.Name.ToLower() == MsConst.Gateway;
            }
        }
        /// <summary>
        /// 是否注册中心（主）
        /// </summary>
        public static bool IsRegCenterOfMaster
        {
            get
            {
                return IsRegCenter && (string.IsNullOrEmpty(MsConfig.Server.RcUrl) || MsConfig.Server.RcUrl == MvcConfig.RunUrl);
            }
        }
        #endregion


        #region 注册中心 - 数据库配置

        //private static string _MsConn = null;
        ///// <summary>
        ///// 微服务 - 注册中心  数据库链接配置
        ///// </summary>
        //public static string MsConn
        //{
        //    get
        //    {
        //        if (_MsConn == null)
        //        {
        //            _MsConn = AppConfig.GetConn("MsConn");
        //        }
        //        return _MsConn;
        //    }
        //    set
        //    {
        //        _MsConn = value;
        //    }
        //}

        //private static string _MsTableName;
        ///// <summary>
        ///// 异常日志表名（默认为MsRegCenter，可配置）
        ///// </summary>
        //public static string MsTableName
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(_MsTableName))
        //        {
        //            _MsTableName = AppConfig.GetApp("MsTableName", "MsRegCenter");
        //        }
        //        return _MsTableName;
        //    }
        //    set
        //    {
        //        _MsTableName = value;
        //    }
        //}
        #endregion
    }
}
