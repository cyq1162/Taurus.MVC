using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Taurus.Core
{
    /// <summary>
    /// 自动较验参是否必填、正则验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireAttribute : Attribute
    {
        public string paraName, regex, emptyTip, regexTip;
        public bool isRequired, isValidated;

        /// <summary>
        /// 验证参数
        /// </summary>
        /// <param name="paraName">参数名称</param>
        public RequireAttribute(string paraName)
        {

            Init(paraName, true, null, paraName.Contains(",") ? "{0} is required." : null, null);
        }
        /// <param name="isRequired">是否必填</param>
        /// <param name="regex">正则</param>
        public RequireAttribute(string paraName, bool isRequired, string regex)
        {
            Init(paraName, isRequired, regex, null, null);
        }
        /// <param name="outParaName">输出的参数提示名</param>
        public RequireAttribute(string paraName, bool isRequired, string regex, string outParaName)
        {
            //验证outParaName 是否中文

            if (outParaName != paraName && !string.IsNullOrEmpty(outParaName))
            {
                if (Regex.IsMatch(outParaName, @"[\u4e00-\u9fbb]"))//中文
                {
                    emptyTip = outParaName + "不能为空。";
                    regexTip = outParaName + "格式错误。";
                }
                else
                {
                    emptyTip = outParaName + " is required.";
                    regexTip = outParaName + " is invalid.";
                }
            }
            Init(paraName, isRequired, regex, emptyTip, regexTip);
        }

        /// <param name="emptyTip">为空时的提示</param>
        /// <param name="regexTip">正则验证失败时的提示</param>
        public RequireAttribute(string paraName, bool isRequired, string regex, string emptyTip, string regexTip)
        {
            Init(paraName, isRequired, regex, emptyTip, regexTip);
        }
        private void Init(string paraName, bool isRequired, string regex, string emptyTip, string regexTip)
        {
            this.paraName = paraName;
            this.isRequired = isRequired;
            this.regex = regex;
            if (string.IsNullOrEmpty(emptyTip))
            {
                emptyTip = paraName + " is required.";
            }
            if (string.IsNullOrEmpty(regexTip))
            {
                regexTip = paraName + " is invalid.";
            }
            this.emptyTip = emptyTip;
            this.regexTip = regexTip;
        }
    }

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
        public const string Mobile = @"^1([38][0-9]|4[579]|5[0-3,5-9]|6[6]|7[0135678]|9[89])\d{8}$";
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
