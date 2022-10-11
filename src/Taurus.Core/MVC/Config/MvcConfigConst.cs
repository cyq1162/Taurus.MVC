using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Mvc
{
    /// <summary>
    /// Taurus 默认可以设置的配置项
    /// </summary>
    internal partial class MvcConfigConst
    {
        /// <summary>
        /// 指定控制器(控制器所在的项目名称) 如 Taurus.Controllers : Taurus.Controllers
        /// 默认值：Taurus.Controllers
        /// </summary>
        public const string Controllers = "Taurus.Controllers";

        /// <summary>
        /// 配置请求路径的默认后缀 如 Taurus.Suffix : .html
        /// 默认值：空
        /// </summary>
        public const string Suffix = "Taurus.Suffix";
        /// <summary>
        /// 配置路由模式 如 Taurus.RouteMode : 1
        ///值为0：匹配{Action}/{Para}
        ///值为1：匹配{Controller}/{Action}/{Para}
        ///值为2：匹配{Module}/{Controller}/{Action}/{Para}
        ///默认值：1
        /// </summary>
        public const string RouteMode = "Taurus.RouteMode";
        /// <summary>
        /// 配置页面起始访问路径 如 Taurus.DefaultUrl ： home/index
        /// 默认值：无
        /// </summary>
        public const string DefaultUrl = "Taurus.DefaultUrl";

        /// <summary>
        /// 配置是否允许JS跨域请求 如 Taurus.IsAllowCORS ： false
        /// 默认值：true
        /// </summary>
        public const string IsAllowCORS = "Taurus.IsAllowCORS";

        /// <summary>
        /// 配置部署成子应用程序的名称 如 Taurus.SubAppName ： UI
        /// 默认值：无
        /// </summary>
        public const string SubAppName = "Taurus.SubAppName";

        /// <summary>
        ///配置Mvc的Views目录文件夹 如 Taurus.Views ： Views（默认文件夹名称）
        /// 默认值：Views
        /// </summary>
        public const string Views = "Taurus.Views";


    }
}
