using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Threading;
using Taurus.Mvc;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Admin;
using CYQ.Data;

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
        internal static void ResetIPList(string ipList)
        {
            IO.Write(AdminConst.IPBlacknamePath, ipList);
            LastUpdateTime = DateTime.Now;
            Dictionary<string, byte> dic = new Dictionary<string, byte>();
            List<string> list = new List<string>();
            string[] items = ipList.Split(new char[] { ',', '\n', '\r' });
            foreach (string item in items)
            {
                if (string.IsNullOrEmpty(item))
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
        public static bool IsValid()
        {
            if (ipBlackDic.Count > 0 || ipBlackList.Count > 0)
            {
                System.Web.HttpRequest request = System.Web.HttpContext.Current.Request;
                string ip = null;
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
        /// <summary>
        /// IP 黑名单最后一次更新时间
        /// </summary>
        public static DateTime LastUpdateTime { get; set; }
        static IPLimit()
        {
            if (ipBlackDic.Count == 0 && ipBlackList.Count == 0)
            {
                string ipList = IO.Read(AdminConst.IPBlacknamePath);
                if (!string.IsNullOrEmpty(ipList))
                {
                    FileInfo info = new FileInfo(IO.Path(AdminConst.IPBlacknamePath));
                    LastUpdateTime = info.LastWriteTime;
                    ResetIPList(ipList);
                }
                else if (LimitConfig.IP.IsSync)
                {
                    if (MsConfig.IsServer && !MsConfig.IsRegCenterOfMaster)
                    {
                        SyncIPListWithRegisterCenter();
                    }
                }
            }
        }
        /// <summary>
        /// 与服务端IP黑名单保持同步。
        /// </summary>
        /// <param name="serverIPTick"></param>
        internal static void SyncIPList(long serverIPTick)
        {
            if (serverIPTick > LastUpdateTime.Ticks && LimitConfig.IP.IsSync)
            {
                SyncIPListWithRegisterCenter();
            }
        }
        private static void SyncIPListWithRegisterCenter()
        {
            Thread thread = new Thread(new ThreadStart(GetIPList));
            thread.IsBackground = true;
            thread.Start();
        }
        private static void GetIPList()
        {
            string url = MsConfig.Server.RcUrl + MsConfig.Server.RcPath + "/getiplist";
            if (MsConfig.IsGateway)
            {
                url += "?isGateway=1";
            }
            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Server.RcKey);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    string result = wc.DownloadString(url);
                    if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
                    {
                        string ipList = JsonHelper.GetValue<string>(result, "msg");
                        ResetIPList(ipList);
                    }
                }
            }
            catch (Exception err)
            {
                MsLog.Write(err.Message, url, "GET");
            }
        }


    }
}
