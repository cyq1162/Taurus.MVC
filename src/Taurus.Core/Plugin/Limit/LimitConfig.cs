using CYQ.Data;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// Limit 安全限制配置相关面
    /// </summary>
    public static class LimitConfig
    {
        /// <summary>
        /// 配置Ack的加密Key，默认2-3个字符。
        /// </summary>
        public static string AckKey
        {
            get
            {
                return AppConfig.GetApp(LimitConst.AckKey,"ts");
            }
            set
            {
                AppConfig.SetApp(LimitConst.AckKey, value);
            }
        }
        /// <summary>
        /// 配置：Ack检测路径，默认拦截Mvc请求（即无后缀）
        /// </summary>
        public static string AckCheckPath
        {
            get
            {
                return AppConfig.GetApp(LimitConst.AckCheckPath, "");
            }
            set
            {
                AppConfig.SetApp(LimitConst.AckCheckPath, value);
            }
        }
        

        /// <summary>
        /// 配置：是否对Ack进行解码较验。
        /// 默认值：true
        /// </summary>
        public static bool AckIsVerifyDecode
        {
            get
            {
                return AppConfig.GetAppBool(LimitConst.AckIsVerifyDecode, true);
            }
        }
        /// <summary>
        /// 配置：是否限制Ack重复使用。
        /// 默认值：true
        /// </summary>
        public static bool AckIsVerifyUsed
        {
            get
            {
                return AppConfig.GetAppBool(LimitConst.AckIsVerifyUsed, true);
            }
        }
    }
}
