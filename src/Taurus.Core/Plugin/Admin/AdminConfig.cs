using CYQ.Data;
using CYQ.Data.Tool;
using System.Collections.Generic;
using Taurus.MicroService;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// Admin（文件）信息查看配置
    /// </summary>
    public static class AdminConfig
    {
        /// <summary>
        /// 独立的持久化配置。
        /// </summary>
        private static Dictionary<string, string> durableConfig = new Dictionary<string, string>();
        /// <summary>
        /// 临时修改的配置。
        /// </summary>
        private static Dictionary<string, string> tempConfig = new Dictionary<string, string>();
        static AdminConfig()
        {
            string config = IO.Read(AdminConst.ConfigPath);
            if (!string.IsNullOrEmpty(config))
            {
                var dic = JsonHelper.Split(config);
                if (dic != null && dic.Count > 0)
                {
                    foreach (var kv in dic)
                    {
                        AppConfig.SetApp(kv.Key, kv.Value);
                    }
                    durableConfig = dic;
                }
            }
        }
        /// <summary>
        /// 添加持久化配置
        /// </summary>
        internal static void AddDurableConfig(string key, string value)
        {
            if (tempConfig.ContainsKey(key))
            {
                tempConfig.Remove(key);
            }

            if (durableConfig.ContainsKey(key))
            {
                durableConfig[key] = value;
            }
            else
            {
                durableConfig.Add(key, value);
            }
            IO.Write(AdminConst.ConfigPath, JsonHelper.ToJson(durableConfig));
        }
        internal static void RemoveDurableConfig(string key, string value)
        {
            if (!tempConfig.ContainsKey(key))
            {
                tempConfig.Add(key, value);
            }

            if (durableConfig.ContainsKey(key))
            {
                durableConfig.Remove(key);
                IO.Write(AdminConst.ConfigPath, JsonHelper.ToJson(durableConfig));
            }
        }
        /// <summary>
        /// 检测是否包含持久化配置。
        /// </summary>
        internal static bool IsContainsDurableKey(string key)
        {
            return durableConfig.ContainsKey(key);
        }
        /// <summary>
        /// 检测是否包含临时修改配置。
        /// </summary>
        internal static bool IsContainsTempKey(string key)
        {
            return tempConfig.ContainsKey(key);
        }
        /// <summary>
        /// 配置是否启用Admin 后台管理功能
        /// 如 Admin.IsEnable ：true， 默认值：true
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                return AppConfig.GetAppBool("Admin.IsEnable", true);
            }
            set
            {
                AppConfig.SetApp("Admin.IsEnable", value.ToString());
            }
        }

        /// <summary>
        /// 配置Mvc的Admin后台管理访问路径
        /// 如 Admin.Admin ： "admin"， 默认值：admin
        /// </summary>
        public static string Path
        {
            get
            {
                return AppConfig.GetApp("Admin.Path", "/admin");
            }
            set
            {
                AppConfig.SetApp("Admin.Path", value);
            }
        }
        /// <summary>
        /// 配置Admin的html加载文件夹名称
        /// 如 Admin.HtmlFolderName ： "admin"， 默认值：admin
        /// </summary>
        internal static string HtmlFolderName
        {
            get
            {
                return AppConfig.GetApp("Admin.HtmlFolderName", "admin").Trim('/');
            }
        }
        /// <summary>
        /// 应用配置：配置管理后台访问账号【账号默认admin】
        /// </summary>
        public static string UserName
        {
            get
            {
                return AppConfig.GetApp("Admin.UserName", "admin");
            }
            set
            {
                AppConfig.SetApp("Admin.UserName", value);
            }
        }
        /// <summary>
        /// 应用配置：配置管理后台访问密码【默认空】
        /// </summary>
        public static string Password
        {
            get
            {
                return AppConfig.GetApp("Admin.Password", "");
            }
            set
            {
                AppConfig.SetApp("Admin.Password", value);
            }
        }
    }
}
