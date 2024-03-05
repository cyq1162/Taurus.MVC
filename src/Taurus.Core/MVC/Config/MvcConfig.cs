using CYQ.Data;
using System;
using Taurus.Plugin.MicroService;

namespace Taurus.Mvc
{
    /// <summary>
    /// Taurus - Mvc Config
    /// </summary>
    public static partial class MvcConfig
    {
        internal static void OnChange(string key, string value)
        {
            _IsEnable = null;
            _IsAllowIPHost = null;
            _IsAddTaurusHeader = null;
            _IsPrintRequestLog = null;
            _IsPrintRequestSql = null;
            _Suffix = null;
            _RouteMode = null;
            _DefaultUrl = null;
            _Views = null;
            _SubAppName = null;
            _RunUrl = null;
        }
        private static bool? _IsEnable;
        /// <summary>
        /// 配置是否启用Mvc功能 
        /// 如 Mvc.IsEnable ：true
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                if (_IsEnable.HasValue) { return _IsEnable.Value; }
                _IsEnable = AppConfig.GetAppBool("Mvc.IsEnable", true);
                return _IsEnable.Value;
            }
            set
            {
                AppConfig.SetApp("Mvc.IsEnable", value.ToString());
                _IsEnable = value;
            }
        }
        ///// <summary>
        ///// 配置是否启用Mvc预热功能 
        ///// 如 Mvc.IsEnable ：true
        ///// </summary>
        //public static bool IsPreheat
        //{
        //    get
        //    {
        //        return AppConfig.GetAppBool("Mvc.IsEnable", true);
        //    }
        //    set
        //    {
        //        AppConfig.SetApp("Mvc.IsEnable", value.ToString());
        //    }
        //}
        private static bool? _IsAllowIPHost;
        /// <summary>
        /// 配置是否 Mvc 允许 通过IP访问
        /// 如 Mvc.IsAllowIPHost ：true， 默认值：true
        /// </summary>
        public static bool IsAllowIPHost
        {
            get
            {
                if (_IsAllowIPHost.HasValue) { return _IsAllowIPHost.Value; }
                _IsAllowIPHost = AppConfig.GetAppBool("Mvc.IsAllowIPHost", true);
                return _IsAllowIPHost.Value;
            }
            set
            {
                AppConfig.SetApp("Mvc.IsAllowIPHost", value.ToString());
                _IsAllowIPHost = value;
            }
        }

        private static bool? _IsAddTaurusHeader;
        /// <summary>
        /// 配置是否 Mvc 添加Taurus主机头
        /// 如 Mvc.IsAddTaurusHeader ：true， 默认值：true
        /// </summary>
        public static bool IsAddTaurusHeader
        {
            get
            {
                if (_IsAddTaurusHeader.HasValue) { return _IsAddTaurusHeader.Value; }
                _IsAddTaurusHeader = AppConfig.GetAppBool("Mvc.IsAddTaurusHeader", MsConfig.IsClient || MsConfig.IsServer);
                return _IsAddTaurusHeader.Value;
            }
            set
            {
                AppConfig.SetApp("Mvc.IsAddTaurusHeader", value.ToString());
                _IsAddTaurusHeader = value;
            }
        }

        private static bool? _IsPrintRequestLog;
        /// <summary>
        /// 配置是否打印请求日志【用于调试打印请求日志】 
        /// 如 Mvc.IsPrintRequestLog ：false（默认值）
        /// </summary>
        public static bool IsPrintRequestLog
        {
            get
            {
                if (_IsPrintRequestLog.HasValue) { return (_IsPrintRequestLog.Value); }
                _IsPrintRequestLog = AppConfig.GetAppBool("Mvc.IsPrintRequestLog", false);
                return _IsPrintRequestLog.Value;
            }
            set
            {
                AppConfig.SetApp("Mvc.IsPrintRequestLog", value.ToString());
                _IsPrintRequestLog = value;
            }
        }

        private static bool? _IsPrintRequestSql;
        /// <summary>
        /// 配置是否打印请求执行的Sql语句【用于调试打印请求执行的Sql语句】 
        /// 如 Mvc.IsPrintRequestSql ：false（默认值）
        /// </summary>
        public static bool IsPrintRequestSql
        {
            get
            {
                if (_IsPrintRequestSql.HasValue) { return _IsPrintRequestSql.Value; }
                _IsPrintRequestSql = AppConfig.GetAppBool("Mvc.IsPrintRequestSql", false);
                return _IsPrintRequestSql.Value;
            }
            set
            {
                AppConfig.SetApp("Mvc.IsPrintRequestSql", value.ToString());
                _IsPrintRequestSql = value;
            }
        }
        ///// <summary>
        ///// 指定控制器(控制器所在的项目名称)。
        ///// 如 Mvc.Controllers : "Taurus.Controllers"， 默认值："*"
        ///// </summary>
        //public static string Controllers
        //{
        //    get
        //    {
        //        return AppConfig.GetApp("Mvc.Controllers", "*");
        //    }
        //    set
        //    {
        //        AppConfig.SetApp("Mvc.Controllers", value);
        //    }
        //}

        private static string _Suffix;
        /// <summary>
        /// 配置请求路径的默认后缀。
        /// 如 Mvc.Suffix : ".html"，默认值：空
        /// </summary>
        public static string Suffix
        {
            get
            {
                if (_Suffix != null) { return _Suffix; }
                _Suffix = AppConfig.GetApp("Mvc.Suffix", "");
                return _Suffix;
            }
            set
            {
                AppConfig.SetApp("Mvc.Suffix", value);
                _Suffix = value;
            }
        }

        private static int? _RouteMode;
        /// <summary>
        /// 配置路由模式。
        /// 如 Mvc.RouteMode : 1，默认值1。
        /// 值为0：匹配{Action}/{Para}
        /// 值为1：匹配{Controller}/{Action}/{Para}
        /// 值为2：匹配{Module}/{Controller}/{Action}/{Para}
        /// </summary>
        public static int RouteMode
        {
            get
            {
                if (_RouteMode.HasValue) { return _RouteMode.Value; }
                _RouteMode = AppConfig.GetAppInt("Mvc.RouteMode", 1);
                return _RouteMode.Value;
            }
            set
            {
                AppConfig.SetApp("Mvc.RouteMode", value.ToString());
                _RouteMode = value;
            }
        }

        private static string _DefaultUrl;
        /// <summary>
        /// 配置页面起始访问路径。
        /// 如 Mvc.DefaultUrl ： "home/index"
        /// </summary>
        public static string DefaultUrl
        {
            get
            {
                if (_DefaultUrl != null) { return _DefaultUrl; }
                _DefaultUrl = AppConfig.GetApp("Mvc.DefaultUrl", "");
                return _DefaultUrl;
            }
            set
            {
                AppConfig.SetApp("Mvc.DefaultUrl", value);
                _DefaultUrl = value;
            }
        }

        private static string _Views;
        /// <summary>
        /// 配置Mvc的Views目录文件夹。
        /// 如 Mvc.Views ： "Views"， 默认值：Views（默认文件夹名称）
        /// </summary>
        public static string Views
        {
            get
            {
                if (_Views != null) { return _Views; }
                _Views = AppConfig.GetApp("Mvc.Views", "Views");
                return _Views;
            }
            set
            {
                AppConfig.SetApp("Mvc.Views", value);
                _Views = value;
            }
        }

        private static string _SubAppName;
        /// <summary>
        /// 配置部署成子应用程序的名称。
        /// 如 Mvc.SubAppName ： "UI"
        /// </summary>
        public static string SubAppName
        {
            get
            {
                if (_SubAppName != null) { return _SubAppName; }
                _SubAppName = AppConfig.GetApp("Mvc.SubAppName", "");
                return _SubAppName;
            }
            set
            {
                AppConfig.SetApp("Mvc.SubAppName", value);
                _SubAppName = value;
            }
        }

        private static string _RunUrl;
        /// <summary>
        /// 应用配置：当前Web Application运行Url【Kestrel启动运行需要】
        /// </summary>
        public static string RunUrl
        {
            get
            {
                if (_RunUrl != null) { return _RunUrl; }
                string url = AppConfig.GetApp("Mvc.RunUrl", "");
                if (string.IsNullOrEmpty(url))
                {
                    string dockerUrl = Environment.GetEnvironmentVariable("RunUrl");
                    if (!string.IsNullOrEmpty(dockerUrl))
                    {
                        url = dockerUrl;
                    }
                }
                _RunUrl = url.TrimEnd('/');
                AppConfig.SetApp("Mvc.RunUrl", _RunUrl);
                return _RunUrl;
            }
            set
            {
                AppConfig.SetApp("Mvc.RunUrl", value);
                _RunUrl = value;
            }
        }

        private static string InitDockerUrl()
        {
            // Docker 部署：则返回映射后的地址
            return Environment.GetEnvironmentVariable("RunUrl");//跨服务器配置完整路径：http://ip:port
            //string dockerUrl =
            //if (!string.IsNullOrEmpty(dockerUrl))
            //{
            //    return dockerUrl;
            //}
            //string host = Kestrel.Urls;
            //if (!string.IsNullOrEmpty(host))
            //{
            //    if (host.EndsWith(":80"))
            //    {
            //        host = host.Replace(":80", "");//去掉默认端口
            //    }
            //    string ip = MvcConst.HostIP;
            //    return host.Replace("*", ip);//设置启动路径
            //}
            //return host;
        }
    }
}
