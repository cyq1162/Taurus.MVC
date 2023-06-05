using System.Web;

namespace Taurus.Plugin.CORS
{
    /// <summary>
    /// 跨域检测
    /// </summary>
    public static class CORSRun
    {
        #region 检测CORS跨域请求
        /// <summary>
        /// 跨域检测
        /// </summary>
        /// <param name="context">当前上下文</param>
        /// <returns></returns>
        public static bool Check(HttpContext context)
        {
            if (context.Request.HttpMethod == "OPTIONS")
            {
                context.Response.StatusCode = 204;
                context.Response.AppendHeader("Access-Control-Allow-Method", CORSConfig.Method);
                context.Response.AppendHeader("Access-Control-Allow-Origin", CORSConfig.Origin);
                if (CORSConfig.MaxAge > 0)
                {
                    context.Response.AppendHeader("Access-Control-Max-Age", CORSConfig.MaxAge.ToString());
                }
                if (CORSConfig.Credentials)
                {
                    context.Response.AppendHeader("Access-Control-Allow-Credentials", "true");
                }
                if (context.Request.Headers["Access-Control-Allow-Headers"] != null)
                {
                    context.Response.AppendHeader("Access-Control-Allow-Headers", context.Request.Headers["Access-Control-Allow-Headers"]);
                }
                else if (context.Request.Headers["Access-Control-Request-Headers"] != null)
                {
                    context.Response.AppendHeader("Access-Control-Allow-Headers", context.Request.Headers["Access-Control-Request-Headers"]);
                }
                context.Response.End();
                return false;
            }
            else if (context.Request.UrlReferrer != null && context.Request.Url.Authority != context.Request.UrlReferrer.Authority)
            {
                //跨域访问
                context.Response.AppendHeader("Access-Control-Allow-Origin", CORSConfig.Origin);
            }

            return true;
        }

        #endregion
    }
}
