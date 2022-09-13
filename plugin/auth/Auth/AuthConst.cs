using System;
namespace Taurus.Plugin.Auth
{
    /// <summary>
    /// 授权相关的信息
    /// </summary>
    internal class AuthConst
    {
        /// <summary>
        /// 提示密码已过期
        /// </summary>
        public const string PasswordExpired = "password has expired.";
        /// <summary>
        /// 密码错误
        /// </summary>
        public const string PasswordError = "password error.";
        /// <summary>
        /// 账号不存在
        /// </summary>
        public const string UserNotExist = "user does not exist.";
        /// <summary>
        /// cookie token名称
        /// </summary>
        public const string CookieTokenName = "taurus_token";
        /// <summary>
        /// cookie user名称
        /// </summary>
        public const string CookieUserName = "taurus_user";

        /// <summary>
        /// 配置则启用默认的Token机制 如 Taurus.Auth :{TableName:用户表名,UserName:用户名字段名,Password:密码字段名,TokenExpireTime:24}
        /// 可配置的映射字段：TableName,UserName,Password(这三个必填写，后面可选）,FullName,Status,PasswordExpireTime,Email,Mobile,RoleID,TokenExpireTime(这个是配置小时）
        /// 默认值：无
        /// </summary>
        public const string Auth = "Taurus.Auth";
    }
}
