using Microsoft.AspNetCore.Http;
using Taurus.Mvc;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// 限制策略启动类
    /// </summary>
    internal static class LimitRun
    {
        /// <summary>
        /// 检测请求是否合法。
        /// </summary>
        /// <returns></returns>
        public static bool CheckRequestIsSafe(HttpContext context, out string tip)
        {
            tip = string.Empty;
            string localPath = context.Request.Path.Value;
            if (WebTool.IsTaurusSuffix(localPath))
            {
                string configPath = LimitConfig.AckCheckPath;
                if (localPath.Length > 1 && !string.IsNullOrEmpty(configPath))
                {
                    foreach (var item in configPath.Split(','))
                    {
                        if (localPath.IndexOf(item, System.StringComparison.OrdinalIgnoreCase) > -1)
                        {
                            return CheckAckIsSafe(out tip);
                        }
                    }
                }
                else
                {
                    return CheckAckIsSafe(out tip);
                }
            }
            return true;
        }

        public static bool CheckAckIsSafe(out string tip)
        {
            tip = string.Empty;
            if (LimitConfig.AckIsVerifyDecode || LimitConfig.AckIsVerifyUsed)
            {
                string ack = WebTool.Query<string>("ack");
                if (string.IsNullOrEmpty(ack))
                {
                    tip = "ack is empty.";
                    return false;
                }
                if (LimitConfig.AckIsVerifyDecode && !AckLimit.IsValid(ack))
                {
                    tip = "ack is invalid.";
                    return false;
                }
                if (LimitConfig.AckIsVerifyUsed && AckLimit.IsAckUsed(ack))
                {
                    tip = "ack is used.";
                    return false;
                }
            }
            return true;
        }
    }
}
