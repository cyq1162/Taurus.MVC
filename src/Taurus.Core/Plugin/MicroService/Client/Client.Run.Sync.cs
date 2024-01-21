using System;
using System.Net;
using CYQ.Data;
using CYQ.Data.Json;
using CYQ.Data.Tool;
using Taurus.Mvc;
using Taurus.Plugin.Admin;
using Taurus.Plugin.Limit;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 运行中心
    /// </summary>
    internal partial class MsRun
    {
        #region 配置 同步

        /// <summary>
        /// 从服务端同步配置。
        /// </summary>
        /// <param name="configTick"></param>
        internal static void SyncConfig(long configTick)
        {
            if (configTick > Client.SyncConfigTime.Ticks && MsConfig.Client.IsAllowSyncConfig)
            {
                SyncConfigFromRegistryCenter();
            }
        }
        private static void SyncConfigFromRegistryCenter()
        {
            string url = MsConfig.Client.RcUrl + MsConfig.Client.RcPath + "/getconfigsynclist";

            try
            {
                using (WebClient wc = new WebClient())
                {
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Client.RcKey);
                    wc.Headers.Add("ack", AckLimit.CreateAck());
                    wc.Headers.Add("Referer", MvcConfig.RunUrl);
                    string result = wc.DownloadString(url);
                    if (!string.IsNullOrEmpty(result) && JsonHelper.IsSuccess(result))
                    {
                        string configList = JsonHelper.GetValue<string>(result, "msg");
                        SetConfigSync(configList);
                    }
                }
            }
            catch (Exception err)
            {
                MsLog.Write(err.Message, url, "GET");
            }
        }

        /// <summary>
        /// 批量添加持久化
        /// </summary>
        private static void SetConfigSync(string configList)
        {
            Client.SyncConfigTime = DateTime.Now;
            IO.Write(AdminConst.ConfigSyncPath, configList);
            if (string.IsNullOrEmpty(configList) || !configList.Contains("="))
            {
                return;
            }
            bool isDurable = configList.StartsWith("#durable");
            string[] configs = configList.Trim().Split('\n');
            for (int i = 0; i < configs.Length; i++)
            {
                string config = configs[i];
                if (string.IsNullOrEmpty(config) || !config.Contains("=")) { continue; }
                string keyValue = config.Trim();
                if (keyValue.StartsWith("//") || keyValue.StartsWith("#")) { continue; }
                int k = keyValue.IndexOf('=');
                string key = keyValue.Substring(0, k).Trim();
                string value = keyValue.Substring(k + 1).Trim();

                if (key.EndsWith("Conn"))
                {
                    AppConfig.SetConn(key, value);
                }
                else
                {
                    AppConfig.SetApp(key, value);
                }
                if (isDurable)
                {
                    AdminAPI.Durable.Add(key, value, i == configs.Length - 1);
                }
            }
        }

        #endregion
    }

}
