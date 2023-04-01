using CYQ.Data;

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
                return AppConfig.GetApp("Taurus.Controllers", "*");
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
                return AppConfig.GetApp("Taurus.Suffix", "");
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
            get { return AppConfig.GetAppInt("Taurus.RouteMode", 1); }
            set { AppConfig.SetApp("Taurus.RouteMode", value.ToString()); }
        }

        /// <summary>
        /// 配置页面起始访问路径 如 Taurus.DefaultUrl ： home/index
        /// 默认值：无
        /// </summary>
        public static string DefaultUrl
        {
            get
            {
                return AppConfig.GetApp("Taurus.DefaultUrl", "");
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
                return AppConfig.GetAppBool("Taurus.IsAllowCORS", true);
            }
        }
        /// <summary>
        /// 配置Mvc的Views目录文件夹 如 Taurus.Views ： Views（默认文件夹名称）
        /// 默认值：Views
        /// </summary>
        public static string Views
        {
            get
            {
                return AppConfig.GetApp("Taurus.Views", "Views");
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
                return AppConfig.GetApp("Taurus.SubAppName", "");
            }
        }
    }
}
