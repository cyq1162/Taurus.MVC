using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Taurus.Mvc.Attr
{
    /// <summary>
    /// 自动较验参是否必填、正则验证
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RequireAttribute : Attribute
    {
        internal string paraName, regex, emptyTip, regexTip;
        internal bool isRequired;

        /// <summary>
        /// 验证参数
        /// </summary>
        /// <param name="paraName">参数名称</param>
        public RequireAttribute(string paraName)
        {

            Init(paraName, true, null, paraName.Contains(",") ? "{0} is required." : null, null);
        }
        /// <summary>
        /// 验证参数
        /// </summary>
        /// <param name="paraName">参数名称</param>
        /// <param name="cnParaName">输出的参数提示名</param>
        public RequireAttribute(string paraName, string cnParaName)
        {
            //验证outParaName 是否中文

            if (cnParaName != paraName && !string.IsNullOrEmpty(cnParaName))
            {
                if (Regex.IsMatch(cnParaName, @"[\u4e00-\u9fbb]"))//中文
                {
                    emptyTip = cnParaName + "不能为空。";
                    regexTip = cnParaName + "格式错误。";
                }
                else
                {
                    emptyTip = cnParaName + " is required.";
                    regexTip = cnParaName + " is invalid.";
                }
            }
            Init(paraName, true, null, emptyTip, regexTip);
        }

        /// <param name="isRequired">是否必填</param>
        /// <param name="regex">正则</param>
        public RequireAttribute(string paraName, bool isRequired, string regex)
        {
            Init(paraName, isRequired, regex, null, null);
        }
        /// <param name="cnParaName">输出的参数提示名</param>
        public RequireAttribute(string paraName, bool isRequired, string regex, string cnParaName)
        {
            //验证outParaName 是否中文

            if (cnParaName != paraName && !string.IsNullOrEmpty(cnParaName))
            {
                if (Regex.IsMatch(cnParaName, @"[\u4e00-\u9fbb]"))//中文
                {
                    emptyTip = cnParaName + "不能为空。";
                    regexTip = cnParaName + "格式错误。";
                }
                else
                {
                    emptyTip = cnParaName + " is required.";
                    regexTip = cnParaName + " is invalid.";
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

   
}
