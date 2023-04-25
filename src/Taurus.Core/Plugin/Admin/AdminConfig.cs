using CYQ.Data;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// Admin（文件）信息查看配置
    /// </summary>
    public static class AdminConfig
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
        /// 如 Admin.Admin ： "admin"， 默认值：admin
        /// </summary>
        public static string Path
        {
            get
            {
                return AppConfig.GetApp("Admin.Path", "admin").Trim('/');
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
