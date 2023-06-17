using System;
using System.Net;
using CYQ.Data.Tool;
using Taurus.Mvc;
using Taurus.Plugin.Limit;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {
        #region IP 黑名单 同步

        /// <summary>
        /// 与服务端IP黑名单保持同步。
        /// </summary>
        /// <param name="serverIPTick"></param>
        internal static void SyncIPList(long serverIPTick)
        {
            if (serverIPTick > Server.SyncIPTime.Ticks && MsConfig.Server.IsAllowSyncIP)
            {
                SyncIPListFromRegisterCenter();
            }
        }
        private static void SyncIPListFromRegisterCenter()
        {
            string url = MsConfig.Server.RcUrl + MsConfig.Server.RcPath + "/getipsynclist";
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
                        Server.SyncIPTime = DateTime.Now;
                        IPLimit.ResetIPList(ipList, true);
                    }
                }
            }
            catch (Exception err)
            {
                MsLog.Write(err.Message, url, "GET");
            }
        }
        #endregion
    }

}
