using System;
using System.Web;
using System.Collections.Generic;
using System.IO;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Admin;
using Taurus.Mvc;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// IP类【负责IP黑名单检测】
    /// </summary>
    internal static partial class IPLimit
    {
        /// <summary>
        /// 用于直接比较
        /// </summary>
        private static Dictionary<string, byte> ipBlackDic = new Dictionary<string, byte>();
        /// <summary>
        /// 用于带*号的比较
        /// </summary>
        private static List<string> ipBlackList = new List<string>();
        internal static void ResetIPList(string ipList, bool isSaveToFile)
        {
            if (isSaveToFile)
            {
                AppDataIO.Write(AdminConst.IPSyncPath, ipList);
            }
            Dictionary<string, byte> dic = new Dictionary<string, byte>();
            List<string> list = new List<string>();
            string[] items = ipList.Split(new char[] { ',', '\n', '\r' });
            foreach (string item in items)
            {
                if (string.IsNullOrEmpty(item) || item.StartsWith("//") || item.StartsWith("#"))
                {
                    continue;
                }
                string key = item.Trim();
                if (key.Contains("*"))
                {
                    key = key.Split('*')[0];
                }
                if (!dic.ContainsKey(key))
                {
                    dic.Add(key, 1);
                }
            }
            //重新赋值，修改引用
            ipBlackDic = dic;
            ipBlackList = list;
        }
        private static bool IsBlack(string ip)
        {
            Dictionary<string, byte> dic = ipBlackDic;//获得引用
            if (dic.Count > 0 && dic.ContainsKey(ip))
            {
                return true;
            }
            List<string> list = ipBlackList;//获得引用
            if (list.Count > 0)
            {
                foreach (string item in list)
                {
                    if (ip.StartsWith(item))
                    {
                        return true;
                    }
                }
            }
            return false;
        }
        /// <summary>
        /// 检测ip是否有效。
        /// </summary>
        /// <returns></returns>
        public static bool IsValid(HttpContext context)
        {
            if (ipBlackDic.Count > 0 || ipBlackList.Count > 0)
            {
                var request = context.Request;
                string ip = null;
                if (LimitConfig.IsUseXRealIP)
                {
                    ip = request.GetHeader("X-Real-IP");
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
                if (IsBlack(ip))
                {
                    return false;
                }

            }
            return true;
        }
    }

    internal static partial class IPLimit
    {
        static IPLimit()
        {
            if (ipBlackDic.Count == 0 && ipBlackList.Count == 0)
            {
                string ipList = AppDataIO.Read(AdminConst.IPSyncPath);
                if (!string.IsNullOrEmpty(ipList))
                {
                    Server.SyncIPTime = AppDataIO.Info(AdminConst.IPSyncPath).LastWriteTime;
                    ResetIPList(ipList, false);
                }
            }
        }
    }
}
