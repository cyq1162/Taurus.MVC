using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Core
{
    /// <summary>
    /// Taurus 默认可以设置的配置项
    /// </summary>
    public class AppSettings
    {
        /// <summary>
        /// 指定控制器(控制器所在的项目名称) 如 Taurus.Controllers : Taurus.Controllers
        /// 默认值：Taurus.Controllers
        /// </summary>
        public const string Controllers = "Taurus.Controllers";
        /// <summary>
        /// 配置是否启用WebAPI文档自动生成功能 如 Taurus.Doc ：true
        /// 默认值：false
        /// </summary>
        public const string IsStartDoc = "Taurus.IsStartDoc";
        /// <summary>
        /// 配置则启用默认的Token机制 如 Taurus.Auth :{TableName:用户表名,UserName:用户名字段名,Password:密码字段名,TokenExpireTime:24}
        /// 可配置的映射字段：TableName,UserName,Password(这三个必填写，后面可选）,FullName,Status,PasswordExpireTime,Email,Mobile,RoleID,TokenExpireTime(这个是配置小时）
        /// 默认值：无
        /// </summary>
        public const string Auth = "Taurus.Auth";
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
        

    }
}
