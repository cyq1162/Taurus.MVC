using CYQ.Data;
using CYQ.Data.Tool;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System;

namespace Taurus.Mvc
{
    /// <summary>
    /// Taurus.Mvc Config
    /// </summary>
    public static class MvcConfig
    {

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
        /// 应用配置：Https 证书 存放路径【客户端默认开启、服务端默认关闭】
        /// </summary>
        public static string SslPath
        {
            get
            {
                return AppConfig.GetApp("Taurus.SslPath", "/App_Data/ssl");
            }
            set
            {
                AppConfig.SetApp("Taurus.SslPath", value);
            }
        }
        /// <summary>
        /// 获取应用证书【证书路径由SslPath配置】（只读）
        /// </summary>
        public static Dictionary<string, X509Certificate2> SslCertificate
        {
            get
            {
                var certificates = new Dictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);
                string sslFolder = AppConfig.WebRootPath + SslPath;
                if (Directory.Exists(sslFolder))
                {
                    string[] files = Directory.GetFiles(sslFolder, "*.pfx", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        string pwdPath = file.Replace(".pfx", ".txt");
                        if (File.Exists(pwdPath))
                        {
                            string pwd = IOHelper.ReadAllText(pwdPath);
                            string domain = Path.GetFileName(pwdPath).Replace(".txt", "");
                            certificates.Add(domain, new X509Certificate2(file, pwd));
                        }
                    }
                }
                return certificates;
            }
        }
        /// <summary>
        /// 应用配置：当前Web Application运行Url【Kestrel启动运行需要】
        /// </summary>
        public static string RunUrl
        {
            get
            {
                return AppConfig.GetApp("Taurus.RunUrl", "").TrimEnd('/');
            }
            set
            {
                AppConfig.SetApp("Taurus.RunUrl", value);
            }
        }
    }
}
