using CYQ.Data;
using System;
using System.Net;

namespace Taurus.Mvc
{
    /// <summary>
    /// Taurus.Mvc Config
    /// </summary>
    public class MvcConfig
    {

        /// <summary>
        /// 指定控制器(控制器所在的项目名称) 如 Taurus.Controllers : Taurus.Controllers
        /// 默认值：Taurus.Controllers
        /// </summary>
        public static string Controllers
        {
            get
            {
                return AppConfig.GetApp(MvcConfigConst.Controllers, "");
            }
        }
        /// <summary>
        /// 配置请求路径的默认后缀 如 Taurus.Suffix : .html
        /// 默认值：空
        /// </summary>
        public static string Suffix
        {
            get
            {
                return AppConfig.GetApp(MvcConfigConst.Suffix, "");
            }
        }
        /// <summary>
        /// 配置路由模式 如 Taurus.RouteMode : 1[默认为1]
        /// 值为0：匹配{Action}/{Para}
        /// 值为1：匹配{Controller}/{Action}/{Para}
        /// 值为2：匹配{Module}/{Controller}/{Action}/{Para}
        /// </summary>
        public static int RouteMode
        {
            get { return AppConfig.GetAppInt(MvcConfigConst.RouteMode, 1); }
            set { AppConfig.SetApp(MvcConfigConst.RouteMode, value.ToString()); }
        }

        /// <summary>
        /// 配置页面起始访问路径 如 Taurus.DefaultUrl ： home/index
        /// 默认值：无
        /// </summary>
        public static string DefaultUrl
        {
            get
            {
                return AppConfig.GetApp(MvcConfigConst.DefaultUrl, "");
            }
        }

        /// <summary>
        /// 配置是否允许JS跨域请求 如 Taurus.IsAllowCORS ： false
        /// 默认值：true
        /// </summary>
        public static bool IsAllowCORS
        {
            get
            {
                return AppConfig.GetAppBool(MvcConfigConst.IsAllowCORS, true);
            }
        }
        /// <summary>
        /// 配置部署成子应用程序的名称 如 Taurus.SubAppName ： UI
        /// 默认值：无
        /// </summary>
        public static string SubAppName
        {
            get
            {
                return AppConfig.GetApp(MvcConfigConst.SubAppName, "");
            }
        }

        /// <summary>
        /// 获取当前 Taurus.Mvc 版本号
        /// </summary>
        public static string TaurusVersion
        {
            get
            {
                return System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        private static string _HostIP;
        /// <summary>
        /// 本机内网IP，若无，则返回主机名
        /// </summary>
        public static string HostIP
        {
            get
            {
                if (string.IsNullOrEmpty(_HostIP))
                {
                    IPAddress[] addressList = Dns.GetHostAddresses(Environment.MachineName);
                    foreach (IPAddress address in addressList)
                    {
                        string ip = address.ToString();
                        if (ip.EndsWith(".1") || ip.Contains(":")) // 忽略路由和网卡地址。
                        {
                            continue;
                        }
                        _HostIP = ip;
                        break;
                    }
                }
                return _HostIP ?? "127.0.0.1";
            }
        }
    }
}
