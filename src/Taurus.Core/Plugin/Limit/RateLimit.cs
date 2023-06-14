using CYQ.Data;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Threading;
using Taurus.Mvc;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// 对请求频繁进行限制
    /// </summary>
    public static class RateLimit
    {
        static RateLimit()
        {
            ThreadBreak.AddGlobalThread(new System.Threading.ParameterizedThreadStart(DoWhileTask));
        }

        /// <summary>
        /// 循环任务
        /// </summary>
        /// <param name="para"></param>
        private static void DoWhileTask(object para)
        {
            bool day = false, night = false;
            int count = 0;
            while (true)
            {
                count++;
                Thread.Sleep(1000);

                try
                {
                    #region 全局 限速
                    maxConcurrentConnections = LimitConfig.Rate.MaxConcurrentConnections;
                    #endregion

                    #region 根据IP 或 Token 限制
                    int period = LimitConfig.Rate.Period;
                    if (count % period == 0)
                    {
                        int limit = LimitConfig.Rate.Limit;
                        count = 0;

                        List<string> keys = rateKeyValue.GetKeys();
                        foreach (string key in keys)
                        {
                            if (limit <= 10 || rateKeyValue[key] > 0)
                            {
                                rateKeyValue[key] = limit;
                            }
                            else
                            {
                                rateKeyValue[key] = limit / 2;//降级
                            }

                        }
                        //每天清两次
                        if (DateTime.Now.Hour == 4 && !night)
                        {
                            night = true;
                            day = false;
                            rateKeyValue.Clear(); // 凌晨，清空一次缓存。
                        }
                        else if (DateTime.Now.Hour == 13 && !day)
                        {
                            night = false;
                            // 白天，缓存足够多才清空。
                            if (keys.Count > 20000)
                            {
                                day = true;
                                rateKeyValue.Clear();
                            }
                        }
                    }
                    #endregion
                }
                catch (Exception err)
                {
                    Log.WriteLogToTxt(err, LogType.Taurus);
                }
            }
        }

        /// <summary>
        /// 是否有效、合法。
        /// </summary>
        internal static bool IsValid()
        {
            if (IsOver())
            {
                return false;
            }
            System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
            string ip = string.Empty;
            if (LimitConfig.Rate.Key.ToUpper() != "IP")
            {
                ip = WebTool.Query<string>(LimitConfig.Rate.Key);
            }
            if (string.IsNullOrEmpty(ip))
            {
                if (LimitConfig.IsUseXRealIP)
                {
                    ip = request.Headers["X-Real-IP"];
                }
                if (string.IsNullOrEmpty(ip))
                {
                    ip = request.UserHostAddress;
                }
                if (LimitConfig.IsIgnoreLAN)
                {
                    if (ip[0] == ':' || ip.StartsWith("192.168.") || ip.StartsWith("10.") || ip.StartsWith("172.") || ip.StartsWith("127."))
                    {
                        return true;//内网不检测
                    }
                }
            }
            if (IsOver(ip))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 存储Token（IP）对应的可使用次数
        /// </summary>
        static MDictionary<string, int> rateKeyValue = new MDictionary<string, int>();

        /// <summary>
        /// 是否超过限定请求次数
        /// </summary>
        /// <param name="ipOrToken"></param>
        /// <returns></returns>
        internal static bool IsOver(string ipOrToken)
        {
            if (rateKeyValue.ContainsKey(ipOrToken))
            {
                int value = rateKeyValue[ipOrToken];
                if (value > 0)
                {
                    rateKeyValue[ipOrToken]--;
                }
                return value <= 0;
            }
            else
            {
                rateKeyValue.Add(ipOrToken, LimitConfig.Rate.Limit);
            }
            return false;
        }

        /// <summary>
        /// 全局：最大并数数
        /// </summary>
        static long maxConcurrentConnections = long.MaxValue;

        /// <summary>
        /// 全局限速：是否超过限定请求次数
        /// </summary>
        /// <returns></returns>
        internal static bool IsOver()
        {
            if (maxConcurrentConnections <= 0) { return true; }
            maxConcurrentConnections--;
            return false;
        }
    }
}
