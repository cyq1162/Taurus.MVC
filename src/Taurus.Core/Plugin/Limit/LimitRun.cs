using Taurus.Mvc;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// 限制策略启动类
    /// </summary>
    internal static class LimitRun
    {
        /// <summary>
        /// 限制策略启动检测：IP黑名单检测
        /// </summary>
        /// <returns></returns>
        public static bool CheckIP()
        {
            if (LimitConfig.IP.IsEnable)
            {
                return IPLimit.IsValid();
            }
            return true;
        }
        /// <summary>
        /// 限制策略启动检测：Ack检测
        /// </summary>
        /// <returns></returns>
        public static bool CheckAck()
        {
            if (LimitConfig.Ack.IsEnable)
            {
                return AckLimit.IsValid(WebTool.Query<string>("ack"));
            }
            return true;
        }

        /// <summary>
        /// 限制策略启动检测：限制请求频率
        /// </summary>
        /// <returns></returns>
        public static bool CheckRate()
        {
            if (LimitConfig.Rate.IsEnable)
            {
                return RateLimit.IsValid();
            }
            return true;
        }
    }
}
