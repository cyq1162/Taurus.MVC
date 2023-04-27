using Taurus.Mvc;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// 限制策略启动类
    /// </summary>
    internal static class LimitRun
    {
        /// <summary>
        /// 限制策略启动检测
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
        /// 限制策略启动检测
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
    }
}
