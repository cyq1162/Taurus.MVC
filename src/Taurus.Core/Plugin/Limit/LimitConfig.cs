using CYQ.Data;
using Taurus.MicroService;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// Limit 安全限制配置相关面
    /// </summary>
    public static class LimitConfig
    {

        public static class Ack
        {
            /// <summary>
            /// 配置是否启用 Ack Limit 安全限制
            /// 如 Limit.Ack.IsEnable ：true， 默认值：false
            /// </summary>
            public static bool IsEnable
            {
                get
                {
                    return AppConfig.GetAppBool("Limit.Ack.IsEnable", false);
                }
                set
                {
                    AppConfig.SetApp("Limit.Ack.IsEnable", value.ToString());
                }
            }
            /// <summary>
            /// 配置Ack的加密Key，默认2-3个字符。
            /// 如：Limit.Ack.Key ："abc"，默认值ts
            /// </summary>
            public static string Key
            {
                get
                {
                    return AppConfig.GetApp("Limit.Ack.Key", "ts");
                }
                set
                {
                    AppConfig.SetApp("Limit.Ack.Key", value);
                }
            }

            /// <summary>
            /// 配置：是否对Ack进行解码较验。
            /// 如：Limit.Ack.IsVerifyDecode：true，默认值：true
            /// </summary>
            public static bool IsVerifyDecode
            {
                get
                {
                    return AppConfig.GetAppBool("Limit.Ack.IsVerifyDecode", true);
                }
                set
                {
                    AppConfig.SetApp("Limit.Ack.IsVerifyDecode", value.ToString());
                }
            }
            /// <summary>
            /// 配置：是否限制Ack重复使用。
            /// 如：Limit.Ack.IsVerifyUsed，默认值：true
            /// </summary>
            public static bool IsVerifyUsed
            {
                get
                {
                    return AppConfig.GetAppBool("Limit.Ack.IsVerifyUsed", true);
                }
                set
                {
                    AppConfig.SetApp("Limit.Ack.IsVerifyUsed", value.ToString());
                }
            }
        }
        public static class IP
        {
            /// <summary>
            /// 配置是否启用 IP Limit 安全限制
            /// 如 Limit.IP.IsEnable ：true， 默认值：true
            /// </summary>
            public static bool IsEnable
            {
                get
                {
                    return AppConfig.GetAppBool("Limit.IP.IsEnable", true);
                }
                set
                {
                    AppConfig.SetApp("Limit.IP.IsEnable", value.ToString());
                }
            }
            /// <summary>
            /// 配置是否启用 IP Blackname List （和注册中心）同步
            /// 如 Limit.IP.IsSync ：true， 默认值：true
            /// </summary>
            public static bool IsSync
            {
                get
                {
                    return AppConfig.GetAppBool("Limit.IP.IsSync", MsConfig.IsServer && !MsConfig.IsRegCenterOfMaster);
                }
                set
                {
                    AppConfig.SetApp("Limit.IP.IsSync", value.ToString());
                }
            }
        }


       
    }
}
