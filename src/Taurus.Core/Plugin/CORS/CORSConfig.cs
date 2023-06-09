using CYQ.Data;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.CORS
{
    /// <summary>
    /// 跨域相关配置
    /// </summary>
    public static class CORSConfig
    {
        /// <summary>
        /// 配置是否启用Mvc CORS 跨域功能 
        /// 如 CORS.IsEnable ：true
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                return AppConfig.GetAppBool("CORS.IsEnable", !MsConfig.IsClient);
            }
            set
            {
                AppConfig.SetApp("CORS.IsEnable", value.ToString());
            }
        }
        /// <summary>
        /// 配置CORS 请求头：Access-Control-Allow-Methods
        /// 如 CORS.Method ： "GET,POST,PUT,DELETE"
        /// </summary>
        public static string Methods
        {
            get
            {
                return AppConfig.GetApp("CORS.Methods", "GET,POST,HEAD,PUT,DELETE");
            }
            set
            {
                AppConfig.SetApp("CORS.Methods", value);
            }
        }
        /// <summary>
        /// 配置CORS 请求头：Access-Control-Allow-Origin
        /// 如 CORS.Origin ： "*"
        /// </summary>
        public static string Origin
        {
            get
            {
                return AppConfig.GetApp("CORS.Origin", "*");
            }
            set
            {
                AppConfig.SetApp("CORS.Origin", value);
            }
        }

        /// <summary>
        /// 配置CORS 请求头：Access-Control-Expose-Headers
        /// 如 CORS.Expose ： "*"
        /// </summary>
        public static string Expose
        {
            get
            {
                return AppConfig.GetApp("CORS.Expose", "*");
            }
            set
            {
                AppConfig.SetApp("CORS.Expose", value);
            }
        }

        
        /// <summary>
        /// 配置CORS 请求头：Access-Control-Allow-Credentials
        /// 如 CORS.Credentials ：false
        /// </summary>
        public static bool Credentials
        {
            get
            {
                return AppConfig.GetAppBool("CORS.Credentials", false);
            }
            set
            {
                AppConfig.SetApp("CORS.Credentials", value.ToString());
            }
        }
        /// <summary>
        /// 配置CORS 请求头： Access-Control-Max-Age
        /// 如 CORS.MaxAge ：10 (s)
        /// </summary>
        public static int MaxAge
        {
            get
            {
                return AppConfig.GetAppInt("CORS.MaxAge", 10);
            }
            set
            {
                AppConfig.SetApp("CORS.MaxAge", value.ToString());
            }
        }


    }
}
