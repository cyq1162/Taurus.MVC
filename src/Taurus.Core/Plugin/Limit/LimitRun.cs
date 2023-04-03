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
        /// <param name="localPath">请求的相对路径</param>
        /// <returns></returns>
        public static bool Check(string localPath)
        {
            if (LimitConfig.IsEnable)
            {
                string tip;
                return CheckRequestIsSafe(localPath, out tip);
            }
            return true;
        }
        /// <summary>
        /// 检测请求是否合法。
        /// </summary>
        /// <returns></returns>
        private static bool CheckRequestIsSafe(string localPath, out string tip)
        {
            tip = string.Empty;
            if (WebTool.IsTaurusSuffix(localPath))
            {
                string configPath = LimitConfig.AckCheckPath;
                if (localPath.Length > 0 && !string.IsNullOrEmpty(configPath))
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

        private static bool CheckAckIsSafe(out string tip)
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
                    tip = "ack has used.";
                    return false;
                }
            }
            return true;
        }
    }
}
