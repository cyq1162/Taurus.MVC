using CYQ.Data;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// Limit 安全限制配置相关面
    /// </summary>
    public static class LimitConfig
    {
        /// <summary>
        /// 配置是否启用 WebAPI 安全限制
        /// 如 Limit.IsEnable ：true， 默认值：false
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                return AppConfig.GetAppBool("Limit.IsEnable", true);
            }
        }
        /// <summary>
        /// 配置Ack的加密Key，默认2-3个字符。
        /// 如：Limit.AckKey ："abc"，默认值ts
        /// </summary>
        public static string AckKey
        {
            get
            {
                return AppConfig.GetApp("Limit.AckKey", "ts");
            }
            set
            {
                AppConfig.SetApp("Limit.AckKey", value);
            }
        }
        /// <summary>
        /// 配置：Ack检测路径，默认拦截Mvc请求（即无后缀）。
        /// 如：Limit.AckCheckPath："/web/,/"
        /// </summary>
        public static string AckCheckPath
        {
            get
            {
                return AppConfig.GetApp("Limit.AckCheckPath", "");
            }
            set
            {
                AppConfig.SetApp("Limit.AckCheckPath", value);
            }
        }


        /// <summary>
        /// 配置：是否对Ack进行解码较验。
        /// 如：Limit.AckIsVerifyDecode：true，默认值：true
        /// </summary>
        public static bool AckIsVerifyDecode
        {
            get
            {
                return AppConfig.GetAppBool("Limit.AckIsVerifyDecode", true);
            }
        }
        /// <summary>
        /// 配置：是否限制Ack重复使用。
        /// 如：Limit.AckIsVerifyUsed，默认值：true
        /// </summary>
        public static bool AckIsVerifyUsed
        {
            get
            {
                return AppConfig.GetAppBool("Limit.AckIsVerifyUsed", true);
            }
        }
    }
}
