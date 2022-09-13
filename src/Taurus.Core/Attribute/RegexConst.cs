using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Mvc.Attr
{
    /// <summary>
    /// 常用正则表达式
    /// </summary>
    public partial class RegexConst
    {
        /// <summary>
        /// 账号
        /// </summary>
        public const string UserName = @"^[a-zA-Z]\w{5,15}$";
        /// <summary>
        /// 手机号
        /// </summary>
        public const string Mobile = @"^1([38][0-9]|4[579]|5[0-3,5-9]|6[124567]|7[0135678]|9[13589])\d{8}$";
        /// <summary>
        /// 手机号或者手机后4位
        /// </summary>
        public const string MobileOrLen4 = @"(^1([38][0-9]|4[579]|5[0-3,5-9]|6[124567]|7[0135678]|9[13589])\d{8})|(^[0-9]{4})$";
        public const string Email = @"^\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*$";
        /// <summary>
        /// 中文
        /// </summary>
        public const string Chinese = "^[\u4e00-\u9fa5]{0,}$";
        /// <summary>
        /// 身份证
        /// </summary>
        public const string IDCard = @"^\d{15}|\d{18}$";
        /// <summary>
        /// 邮编
        /// </summary>
        public const string PostalCode = @"^\d{6}$";
        /// <summary>
        /// IP4地址
        /// </summary>
        public const string IP = @"^((25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))\.){3}(25[0-5]|2[0-4]\d|((1\d{2})|([1-9]?\d)))$";
        /// <summary>
        /// 验证码
        /// </summary>
        public const string VerifyCode = @"^[0-9]{4,6}$";
    }
}
