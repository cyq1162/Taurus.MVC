using CYQ.Data;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// Limit 安全限制配置相关面
    /// </summary>
    public static class LimitConfig
    {
        /// <summary>
        /// 配置是否启用 X-Real-IP 来获取客户端IP
        /// 如 Limit.IsXRealIP ：false
        /// </summary>
        public static bool IsUseXRealIP
        {
            get
            {
                return AppConfig.GetAppBool("Limit.IsUseXRealIP", !MsConfig.IsServer);
            }
            set
            {
                AppConfig.SetApp("Limit.IsUseXRealIP", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否忽略内网IP的请求
        /// 如 Limit.IsIgnoreLAN ：true， 默认值：true
        /// </summary>
        public static bool IsIgnoreLAN
        {
            get
            {
                return AppConfig.GetAppBool("Limit.IsIgnoreLAN", true);
            }
            set
            {
                AppConfig.SetApp("Limit.IsIgnoreLAN", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否忽略管理后台的请求
        /// 如 Limit.IsIgnoreAdmin ：true， 默认值：true
        /// </summary>
        public static bool IsIgnoreAdmin
        {
            get
            {
                return AppConfig.GetAppBool("Limit.IsIgnoreAdmin", true);
            }
            set
            {
                AppConfig.SetApp("Limit.IsIgnoreAdmin", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否忽略微服务的请求
        /// 如 Limit.IsIgnoreMicroService ：true， 默认值：true
        public static bool IsIgnoreMicroService
        {
            get
            {
                return AppConfig.GetAppBool("Limit.IsIgnoreMicroService", true);
            }
            set
            {
                AppConfig.SetApp("Limit.IsIgnoreMicroService", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否忽略测试接口的请求
        /// 如 Limit.IsIgnoreDoc ：true， 默认值：true
        public static bool IsIgnoreDoc
        {
            get
            {
                return AppConfig.GetAppBool("Limit.IsIgnoreDoc", true);
            }
            set
            {
                AppConfig.SetApp("Limit.IsIgnoreDoc", value.ToString());
            }
        }
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
        public static class Rate
        {
            /// <summary>
            /// 配置是否启用 Rate Limit 安全限制请求频繁
            /// 如 Limit.Rate.IsEnable ：true， 默认值：false
            /// </summary>
            public static bool IsEnable
            {
                get
                {
                    return AppConfig.GetAppBool("Limit.Rate.IsEnable", true);
                }
                set
                {
                    AppConfig.SetApp("Limit.Rate.IsEnable", value.ToString());
                }
            }
            /// <summary>
            /// 配置是否使用 Token 做为限制请求频繁的key
            /// 如 Limit.Rate.Key ：IP， 默认值：IP
            /// </summary>
            public static string Key
            {
                get
                {
                    return AppConfig.GetApp("Limit.Rate.Key", "IP");
                }
                set
                {
                    AppConfig.SetApp("Limit.Rate.Key", value.ToString());
                }
            }
            /// <summary>
            /// 配置时间段
            /// 如 Limit.Rate.Period ：5（s）， （单位秒）
            /// </summary>
            public static int Period
            {
                get
                {
                    int period = AppConfig.GetAppInt("Limit.Rate.Period", 5);
                    if (period <= 0)
                    {
                        return 1;
                    }
                    return period;
                }
                set
                {
                    AppConfig.SetApp("Limit.Rate.Period", value.ToString());
                }
            }
            /// <summary>
            /// 配置时间段内允许的最大请求数
            /// 如 Limit.Rate.Limit ：500
            /// </summary>
            public static int Limit
            {
                get
                {
                    int limit = AppConfig.GetAppInt("Limit.Rate.Limit", 500);
                    if (limit <= 0)
                    {
                        return 1;
                    }
                    return limit;
                }
                set
                {
                    AppConfig.SetApp("Limit.Rate.Limit", value.ToString());
                }
            }
        }
    }
}
