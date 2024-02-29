using CYQ.Data;
using CYQ.Data.Json;
using CYQ.Data.Tool;
using System.Collections.Generic;
using System.IO;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// Admin（文件）信息查看配置
    /// </summary>
    public static partial class AdminConfig
    {
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
                        if (kv.Key.EndsWith("Conn"))
                        {
                            AppConfig.SetConn(kv.Key, kv.Value);
                        }
                        else
                        {
                            AppConfig.SetApp(kv.Key, kv.Value);
                        }
                    }
                    AdminAPI.Durable.Init(dic);
                }
            }

            if (!string.IsNullOrEmpty(IO.Read(AdminConst.ConfigSyncPath)))
            {
                Server.SyncConfigTime = IO.Info(AdminConst.ConfigSyncPath).LastWriteTime;
            }
        }
        /// <summary>
        /// 由静态构造函数初始化。
        /// </summary>
        internal static void Init()
        {
            string folder = AppConst.WebRootPath;// + "App_Data/admin";
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
        }

    }

    public static partial class AdminConfig
    {
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
        /// 如 Admin.Admin ： "/admin"， 默认值：/admin
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
        /// 如 Admin.HtmlFolderName ： "Admin"， 默认值：Admin
        /// </summary>
        public static string HtmlFolderName
        {
            get
            {
                return AppConfig.GetApp("Admin.HtmlFolderName", "Admin").Trim('/');
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
