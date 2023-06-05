using CYQ.Data;

namespace Taurus.Plugin.CORS
{
    /// <summary>
    /// 跨域相关配置
    /// </summary>
    public static class CORSConfig
    {
        /// <summary>
        /// 配置是否启用Mvc CORS 跨域功能 
        /// 如 Mvc.CORS.IsEnable ：true
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                return AppConfig.GetAppBool("Mvc.CORS.IsEnable", true);
            }
            set
            {
                AppConfig.SetApp("Mvc.CORS.IsEnable", value.ToString());
            }
        }
        /// <summary>
        /// 配置CORS 请求头：Access-Control-Allow-Method
        /// 如 Mvc.CORS.Method ： "GET,POST,PUT,DELETE"
        /// </summary>
        public static string Method
        {
            get
            {
                return AppConfig.GetApp("Mvc.CORS.Method", "GET,POST,HEAD,PUT,DELETE");
            }
            set
            {
                AppConfig.SetApp("Mvc.CORS.Method", value);
            }
        }
        /// <summary>
        /// 配置CORS 请求头：Access-Control-Allow-Origin
        /// 如 Mvc.CORS.Origin ： "*"
        /// </summary>
        public static string Origin
        {
            get
            {
                return AppConfig.GetApp("Mvc.CORS.Origin", "*");
            }
            set
            {
                AppConfig.SetApp("Mvc.CORS.Origin", value);
            }
        }
        /// <summary>
        /// 配置CORS 请求头：Access-Control-Allow-Credentials
        /// 如 Mvc.CORS.Credentials ：false
        /// </summary>
        public static bool Credentials
        {
            get
            {
                return AppConfig.GetAppBool("Mvc.CORS.Credentials", false);
            }
            set
            {
                AppConfig.SetApp("Mvc.CORS.Credentials", value.ToString());
            }
        }
        /// <summary>
        /// 配置CORS 请求头： Access-Control-Max-Age
        /// 如 Mvc.CORS.MaxAge ：10 (s)
        /// </summary>
        public static int MaxAge
        {
            get
            {
                return AppConfig.GetAppInt("Mvc.CORS.MaxAge", 10);
            }
            set
            {
                AppConfig.SetApp("Mvc.CORS.MaxAge", value.ToString());
            }
        }

    }
}
