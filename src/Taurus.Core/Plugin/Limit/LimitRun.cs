using System;
using Taurus.Mvc;
using static Microsoft.AspNetCore.Hosting.Internal.HostingApplication;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// 限制策略启动类
    /// </summary>
    internal static class LimitRun
    {
        /// <summary>
        /// 该地址是否被忽略
        /// </summary>
        /// <returns></returns>
        public static bool IsIgnoreUrl(Uri uri, Uri referrerUri)
        {
            if (LimitConfig.IsIgnoreAdmin && (WebTool.IsCallAdmin(uri) || WebTool.IsCallAdmin(referrerUri)))
            {
                return true;
            }
            if (LimitConfig.IsIgnoreMicroService && WebTool.IsCallMicroService(uri))
            {
                return true;
            }
            if (LimitConfig.IsIgnoreDoc && (WebTool.IsCallDoc(uri) || WebTool.IsCallDoc(referrerUri)))
            {
                return true;
            }
            return false;
        }

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
