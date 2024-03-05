using CYQ.Data;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Metric
{
    /// <summary>
    /// 接口访问统计相关配置
    /// </summary>
    public static class MetricConfig
    {
        /// <summary>
        /// 配置是否启用 接口访问统计 功能 
        /// 如 Metric.IsEnable ：false
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                return AppConfig.GetAppBool("Metric.IsEnable", false);
            }
            set
            {
                AppConfig.SetApp("Metric.IsEnable", value.ToString());
            }
        }

        /// <summary>
        /// 配置是否 仅统计Mvc接口 
        /// 如 Metric.IsMvcOnly ：true
        /// </summary>
        public static bool IsMvcOnly
        {
            get
            {
                return AppConfig.GetAppBool("Metric.IsMvcOnly", true);
            }
            set
            {
                AppConfig.SetApp("Metric.IsMvcOnly", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否 忽略统计内部插件Url数据  
        /// 如 Metric.IsIgnorePluginUrl ：true
        /// </summary>
        public static bool IsIgnorePluginUrl
        {
            get
            {
                return AppConfig.GetAppBool("Metric.IsIgnorePluginUrl", true);
            }
            set
            {
                AppConfig.SetApp("Metric.IsIgnorePluginUrl", value.ToString());
            }
        }
        /// <summary>
        /// 配置是否 持久化【写文件】
        /// 如 Metric.IsDurable ：true
        /// </summary>
        public static bool IsDurable
        {
            get
            {
                return AppConfig.GetAppBool("Metric.IsDurable", false);
            }
            set
            {
                AppConfig.SetApp("Metric.IsDurable", value.ToString());
            }
        }
        /// <summary>
        /// 配置 持久化【写文件】 秒数(s) 
        /// 如 Metric.DurableInterval ：5
        /// </summary>
        public static int DurableInterval
        {
            get
            {
                return AppConfig.GetAppInt("Metric.DurableInterval", 5);
            }
            set
            {
                AppConfig.SetApp("Metric.DurableInterval", value.ToString());
            }
        }
        /// <summary>
        /// 配置持久化【写文件】路径
        /// 如 Metric.DurablePath ： "doc"， 默认值：doc
        /// </summary>
        public static string DurablePath
        {
            get
            {
                return AppConfig.GetApp("Metric.DurablePath", "/App_Data/metric");
            }
            set
            {
                AppConfig.SetApp("Metric.DurablePath", value);
            }
        }
    }
}
