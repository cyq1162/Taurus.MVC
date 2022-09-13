using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Plugin.Auth
{
    /// <summary>
    /// Taurus.Plugin.Auth AuthConfig
    /// </summary>
    public class AuthConfig
    {
        /// <summary>
        /// 配置则启用默认的Token机制 如 Taurus.Auth :{TableName:用户表名,UserName:用户名字段名,Password:密码字段名,TokenExpireTime:24}
        /// 可配置的映射字段：TableName,UserName,Password(这三个必填写，后面可选）,FullName,Status,PasswordExpireTime,Email,Mobile,RoleID,TokenExpireTime(这个是配置小时）
        /// 默认值：无
        /// </summary>
        public static string Auth
        {
            get
            {
                return AppConfig.GetApp(AuthConst.Auth, "");
            }
        }
    }
}
