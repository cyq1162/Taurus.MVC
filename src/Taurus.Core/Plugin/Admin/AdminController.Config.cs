using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using Taurus.Plugin.Doc;
using System.Configuration;


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
            string oldValue = string.Empty;

            //需要特殊处理的值
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
                case "Taurus.Views":
                    ViewEngine.ViewsPath = null;
                    break;
            }
            if (!string.IsNullOrEmpty(oldValue))
            {
                ControllerCollector.ChangePath(oldValue, value);
            }
            AppConfig.SetApp(key, value);
            if (isDurable)
            {
                AdminConfig.AddDurableConfig(key, value);
            }
            else
            {
                AdminConfig.RemoveDurableConfig(key, value);
            }
            Write("Save success.", true);
        }
    }
}
