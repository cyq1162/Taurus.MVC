using CYQ.Data;
using CYQ.Data.Tool;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System;
using System.Net.Sockets;
using System.Net;

namespace Taurus.Mvc
{
    /// <summary>
    /// Taurus.Mvc Config
    /// </summary>
    public static partial class MvcConfig
    {
        /// <summary>
        /// 配置是否启用Mvc功能 
        /// 如 Taurus.IsEnable ：true
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                return AppConfig.GetAppBool("Taurus.IsEnable", true);
            }
            set
            {
                AppConfig.SetApp("Taurus.IsEnable", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否打印请求日志【用于调试打印请求日志】 
        /// 如 Taurus.IsPrintRequestLog ：false（默认值）
        /// </summary>
        public static bool IsPrintRequestLog
        {
            get
            {
                return AppConfig.GetAppBool("Taurus.IsPrintRequestLog", false);
            }
            set
            {
                AppConfig.SetApp("Taurus.IsPrintRequestLog", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否打印请求执行的Sql语句【用于调试打印请求执行的Sql语句】 
        /// 如 Taurus.IsPrintRequestSql ：false（默认值）
        /// </summary>
        public static bool IsPrintRequestSql
        {
            get
            {
                return AppConfig.GetAppBool("Taurus.IsPrintRequestSql", false);
            }
            set
            {
                AppConfig.SetApp("Taurus.IsPrintRequestSql", value.ToString());
            }
        }
        /// <summary>
        /// 指定控制器(控制器所在的项目名称)。
        /// 如 Taurus.Controllers : "Taurus.Controllers"， 默认值："*"
        /// </summary>
        public static string Controllers
        {
            get
            {
                return AppConfig.GetApp("Taurus.Controllers", "*");
            }
            set
            {
                AppConfig.SetApp("Taurus.Controllers", value);
            }
        }
        /// <summary>
        /// 配置请求路径的默认后缀。
        /// 如 Taurus.Suffix : ".html"，默认值：空
        /// </summary>
        public static string Suffix
        {
            get
            {
                return AppConfig.GetApp("Taurus.Suffix", "");
            }
            set
            {
                AppConfig.SetApp("Taurus.Suffix", value);
            }
        }
        /// <summary>
        /// 配置路由模式。
        /// 如 Taurus.RouteMode : 1，默认值1。
        /// 值为0：匹配{Action}/{Para}
        /// 值为1：匹配{Controller}/{Action}/{Para}
        /// 值为2：匹配{Module}/{Controller}/{Action}/{Para}
        /// </summary>
        public static int RouteMode
        {
            get { return AppConfig.GetAppInt("Taurus.RouteMode", 1); }
            set { AppConfig.SetApp("Taurus.RouteMode", value.ToString()); }
        }

        /// <summary>
        /// 配置页面起始访问路径。
        /// 如 Taurus.DefaultUrl ： "home/index"
        /// </summary>
        public static string DefaultUrl
        {
            get
            {
                return AppConfig.GetApp("Taurus.DefaultUrl", "");
            }
            set
            {
                AppConfig.SetApp("Taurus.DefaultUrl", value);
            }
        }

        /// <summary>
        /// 配置是否允许JS跨域请求。
        /// 如 Taurus.IsAllowCORS ： false，默认值：true
        /// </summary>
        public static bool IsAllowCORS
        {
            get
            {
                return AppConfig.GetAppBool("Taurus.IsAllowCORS", true);
            }
            set
            {
                AppConfig.SetApp("Taurus.IsAllowCORS", value.ToString());
            }
        }
        /// <summary>
        /// 配置Mvc的Views目录文件夹。
        /// 如 Taurus.Views ： "Views"， 默认值：Views（默认文件夹名称）
        /// </summary>
        public static string Views
        {
            get
            {
                return AppConfig.GetApp("Taurus.Views", "Views");
            }
            set
            {
                AppConfig.SetApp("Taurus.Views", value);
            }
        }
        /// <summary>
        /// 配置部署成子应用程序的名称。
        /// 如 Taurus.SubAppName ： "UI"
        /// </summary>
        public static string SubAppName
        {
            get
            {
                return AppConfig.GetApp("Taurus.SubAppName", "");
            }
            set
            {
                AppConfig.SetApp("Taurus.SubAppName", value);
            }
        }

        /// <summary>
        /// 应用配置：当前Web Application运行Url【Kestrel启动运行需要】
        /// </summary>
        public static string RunUrl
        {
            get
            {
                string url = AppConfig.GetApp("Taurus.RunUrl", "");
                if (string.IsNullOrEmpty(url))
                {
                    url = InitRunUrl();
                    if (!string.IsNullOrEmpty(url))
                    {
                        AppConfig.SetApp("Taurus.RunUrl", url);
                    }
                }
                return url.TrimEnd('/');
            }
            set
            {
                AppConfig.SetApp("Taurus.RunUrl", value);
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
                string ip = MvcConst.HostIP;
                return host.Replace("localhost", ip).Replace("*", ip);//设置启动路径
            }
            return host;
        }
    }
}
