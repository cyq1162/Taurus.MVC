using CYQ.Data;
using System;

namespace Taurus.Mvc
{
    /// <summary>
    /// Taurus - Mvc Config
    /// </summary>
    public static partial class MvcConfig
    {
        /// <summary>
        /// 配置是否启用Mvc功能 
        /// 如 Mvc.IsEnable ：true
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                return AppConfig.GetAppBool("Mvc.IsEnable", true);
            }
            set
            {
                AppConfig.SetApp("Mvc.IsEnable", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否 Mvc 允许 通过IP访问
        /// 如 Mvc.IsAllowIPHost ：true， 默认值：true
        /// </summary>
        public static bool IsAllowIPHost
        {
            get
            {
                return AppConfig.GetAppBool("Mvc.IsAllowIPHost", true);
            }
            set
            {
                AppConfig.SetApp("Mvc.IsAllowIPHost", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否 Mvc 添加Taurus主机头
        /// 如 Mvc.IsAddTaurusHeader ：true， 默认值：true
        /// </summary>
        public static bool IsAddTaurusHeader
        {
            get
            {
                return AppConfig.GetAppBool("Mvc.IsAddTaurusHeader", true);
            }
            set
            {
                AppConfig.SetApp("Mvc.IsAddTaurusHeader", value.ToString());
            }
        }

        /// <summary>
        /// 配置是否打印请求日志【用于调试打印请求日志】 
        /// 如 Mvc.IsPrintRequestLog ：false（默认值）
        /// </summary>
        public static bool IsPrintRequestLog
        {
            get
            {
                return AppConfig.GetAppBool("Mvc.IsPrintRequestLog", false);
            }
            set
            {
                AppConfig.SetApp("Mvc.IsPrintRequestLog", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否打印请求执行的Sql语句【用于调试打印请求执行的Sql语句】 
        /// 如 Mvc.IsPrintRequestSql ：false（默认值）
        /// </summary>
        public static bool IsPrintRequestSql
        {
            get
            {
                return AppConfig.GetAppBool("Mvc.IsPrintRequestSql", false);
            }
            set
            {
                AppConfig.SetApp("Mvc.IsPrintRequestSql", value.ToString());
            }
        }
        /// <summary>
        /// 指定控制器(控制器所在的项目名称)。
        /// 如 Mvc.Controllers : "Taurus.Controllers"， 默认值："*"
        /// </summary>
        public static string Controllers
        {
            get
            {
                return AppConfig.GetApp("Mvc.Controllers", "*");
            }
            set
            {
                AppConfig.SetApp("Mvc.Controllers", value);
            }
        }
        /// <summary>
        /// 配置请求路径的默认后缀。
        /// 如 Mvc.Suffix : ".html"，默认值：空
        /// </summary>
        public static string Suffix
        {
            get
            {
                return AppConfig.GetApp("Mvc.Suffix", "");
            }
            set
            {
                AppConfig.SetApp("Mvc.Suffix", value);
            }
        }
        /// <summary>
        /// 配置路由模式。
        /// 如 Mvc.RouteMode : 1，默认值1。
        /// 值为0：匹配{Action}/{Para}
        /// 值为1：匹配{Controller}/{Action}/{Para}
        /// 值为2：匹配{Module}/{Controller}/{Action}/{Para}
        /// </summary>
        public static int RouteMode
        {
            get { return AppConfig.GetAppInt("Mvc.RouteMode", 1); }
            set { AppConfig.SetApp("Mvc.RouteMode", value.ToString()); }
        }

        /// <summary>
        /// 配置页面起始访问路径。
        /// 如 Mvc.DefaultUrl ： "home/index"
        /// </summary>
        public static string DefaultUrl
        {
            get
            {
                return AppConfig.GetApp("Mvc.DefaultUrl", "");
            }
            set
            {
                AppConfig.SetApp("Mvc.DefaultUrl", value);
            }
        }

        /// <summary>
        /// 配置Mvc的Views目录文件夹。
        /// 如 Mvc.Views ： "Views"， 默认值：Views（默认文件夹名称）
        /// </summary>
        public static string Views
        {
            get
            {
                return AppConfig.GetApp("Mvc.Views", "Views");
            }
            set
            {
                AppConfig.SetApp("Mvc.Views", value);
            }
        }
        /// <summary>
        /// 配置部署成子应用程序的名称。
        /// 如 Mvc.SubAppName ： "UI"
        /// </summary>
        public static string SubAppName
        {
            get
            {
                return AppConfig.GetApp("Mvc.SubAppName", "");
            }
            set
            {
                AppConfig.SetApp("Mvc.SubAppName", value);
            }
        }

        /// <summary>
        /// 应用配置：当前Web Application运行Url【Kestrel启动运行需要】
        /// </summary>
        public static string RunUrl
        {
            get
            {
                string url = AppConfig.GetApp("Mvc.RunUrl", "");
                if (string.IsNullOrEmpty(url))
                {
                    url = InitRunUrl();
                    if (!string.IsNullOrEmpty(url))
                    {
                        AppConfig.SetApp("Mvc.RunUrl", url);
                    }
                }
                return url.TrimEnd('/');
            }
            set
            {
                AppConfig.SetApp("Mvc.RunUrl", value);
            }
        }

        private static string InitRunUrl()
        {
            // Docker部署：设置映射后的地址
            string dockerUrl = Environment.GetEnvironmentVariable("RunUrl");//跨服务器配置完整路径：http://ip:port
            if (!string.IsNullOrEmpty(dockerUrl))
            {
                return dockerUrl;
            }
            string host = Kestrel.Urls;
            if (!string.IsNullOrEmpty(host))
            {
                if (host.EndsWith(":80"))
                {
                    host = host.Replace(":80", "");//去掉默认端口
                }
                string ip = MvcConst.HostIP;
                return host.Replace("localhost", ip).Replace("*", ip);//设置启动路径
            }
            return host;
        }
    }
}
