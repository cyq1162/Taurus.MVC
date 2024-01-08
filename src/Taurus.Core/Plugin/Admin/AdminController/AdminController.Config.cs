using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using Taurus.Plugin.Doc;
using System.Configuration;
using CYQ.Data.Cache;
using Taurus.Mvc.Reflect;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 应用配置信息
    /// </summary>
    internal partial class AdminController
    {
        /// <summary>
        /// AppSetting 基础配置信息
        /// </summary>
        public void Config()
        {
            if (!AppConfig.IsNetCore)
            {
                View.Set("kestrelNode", CYQ.Data.Xml.SetType.ClearFlag, "1");
            }
            View.KeyValue.Set("IsNetCore", AppConfig.IsNetCore.ToString().ToLower());
            View.KeyValue.Set("Version", MvcConst.Version);
            string type = Query<string>("t", "mvc").ToLower();
            if (type.StartsWith("kestrel"))
            {
                ConfigKestrel();
            }
            else if (type.StartsWith("mvc"))
            {
                ConfigMvc();
            }
            else if (type.StartsWith("plugin"))
            {
                ConfigPlugin();
            }
            else if (type.StartsWith("cyq.data"))
            {
                ConfigCyq();
            }
        }
        private string HideConnPassword(string conn)
        {
            if (!string.IsNullOrEmpty(conn))
            {
                int i = conn.IndexOf("pwd=");
                if (i > 0)
                {
                    int end = conn.IndexOf(";", i);
                    if (end > 0)
                    {
                        return conn.Substring(0, i + 4) + "******" + conn.Substring(end);
                    }
                    else
                    {
                        return conn.Substring(0, i + 4) + "******";
                    }
                }
            }

            return conn;
        }
        private void Sets(MDataTable dt, string key, object objValue, string description)
        {

            string value = Convert.ToString(objValue);
            if (objValue is Boolean)
            {
                value = value == "True" ? "√" : "×";
            }
            if (AdminConfig.IsContainsDurableKey(key))
            {
                value = value + " 【durable】";
            }
            else if (AdminConfig.IsContainsTempKey(key))
            {
                value = value + " 【temp modify】";
            }
            dt.NewRow(true).Sets(0, key, value, description);
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void BtnSaveConfig()
        {
            string key = Query<string>("key");
            string value = Query<string>("value");
            bool isDurable = Query<bool>("durable");

            #region 先处理，再赋值 - 1

            string oldValue = string.Empty;

            switch (key)
            {
                case "Admin.Path":
                    oldValue = AdminConfig.Path; break;
                case "Doc.Path":
                    oldValue = DocConfig.Path; break;
                case "MicroService.Server.RcPath":
                    oldValue = MsConfig.Server.RcPath; break;
                case "MicroService.Client.RcPath":
                    oldValue = MsConfig.Client.RcPath; break;
                case "Mvc.Views":
                    ViewEngine.ViewsPath = null;
                    break;
                case "Redis.Servers":
                    DistributedCache.Redis.RefleshConfig(value);
                    break;
                case "MemCache.Servers":
                    DistributedCache.MemCache.RefleshConfig(value);
                    break;

            }
            if (!string.IsNullOrEmpty(oldValue))
            {
                ControllerCollector.ChangePath(oldValue, value);
            }

            #endregion
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
                AdminConfig.AddDurableConfig(key, value, true);
            }
            else
            {
                AdminConfig.RemoveDurableConfig(key, value);
            }

            #region 先赋值，再处理 - 2

            if (key.StartsWith("Kestrel."))
            {
                KestrelExtenstions.RefleshOptions();
            }

            #endregion

            Write("Save success.", true);
        }
    }
}
