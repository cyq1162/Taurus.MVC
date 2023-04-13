using CYQ.Data;

namespace Taurus.Plugin.Log
{
    /// <summary>
    /// Log（文件）信息查看配置
    /// </summary>
    public static class LogConfig
    {

        /// <summary>
        /// 配置是否启用 Log（文件）管理功能 
        /// 如 Log.IsEnable ：true， 默认值：true
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                return AppConfig.GetAppBool("Log.IsEnable", true);
            }
            set
            {
                AppConfig.SetApp("Log.IsEnable", value.ToString());
            }
        }
    }
}
