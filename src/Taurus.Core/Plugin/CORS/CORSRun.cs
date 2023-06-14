using System.Web;

namespace Taurus.Plugin.CORS
{
    /// <summary>
    /// 跨域检测
    /// </summary>
    internal static class CORSRun
    {
        #region 检测CORS跨域请求
        /// <summary>
        /// 跨域检测
        /// </summary>
        /// <param name="context">当前上下文</param>
        /// <returns></returns>
        public static bool Check(HttpContext context)
        {
            var req=context.Request;
            var res=context.Response;
            if (req.HttpMethod == "OPTIONS")
            {
                res.StatusCode = 204;
                if (CORSConfig.IsEnable)
                {
                    res.AppendHeader("Access-Control-Allow-Methods", CORSConfig.Methods);
                    string origin = CORSConfig.Origin;
                    if(origin == "*")
                    {
                        origin = req.Headers["Origin"];
                    }
                    res.AppendHeader("Access-Control-Allow-Origin", origin);
                    if (!string.IsNullOrEmpty(CORSConfig.Expose))
                    {
                        res.AppendHeader("Access-Control-Expose-Headers", CORSConfig.Expose);
                    }
                    if (CORSConfig.MaxAge > 0)
                    {
                        res.AppendHeader("Access-Control-Max-Age", CORSConfig.MaxAge.ToString());
                    }
                    if (CORSConfig.Credentials)
                    {
                        res.AppendHeader("Access-Control-Allow-Credentials", "true");
                    }
                    if (req.Headers["Access-Control-Allow-Headers"] != null)
                    {
                        res.AppendHeader("Access-Control-Allow-Headers", req.Headers["Access-Control-Allow-Headers"]);
                    }
                    else if (req.Headers["Access-Control-Request-Headers"] != null)
                    {
                        res.AppendHeader("Access-Control-Allow-Headers", req.Headers["Access-Control-Request-Headers"]);
                    }
                }
                res.End();
                return false;
            }
            if (CORSConfig.IsEnable)
            {
                if (req.UrlReferrer != null && req.Url.Authority != req.UrlReferrer.Authority)
                {
                    string origin = req.Headers["Origin"];
                    if (!string.IsNullOrEmpty(origin))
                    {
                        //res.AppendHeader("Access-Control-Allow-Method", CORSConfig.Method);
                        if (CORSConfig.Origin != "*")
                        {
                            origin = CORSConfig.Origin;
                        }
                        //跨域访问
                        res.AppendHeader("Access-Control-Allow-Origin", origin);//必须
                        if (!string.IsNullOrEmpty(CORSConfig.Expose))
                        {
                            res.AppendHeader("Access-Control-Expose-Headers", CORSConfig.Expose);//必须
                        }
                        if (CORSConfig.Credentials)
                        {
                            if (req.Cookies.Count > 0)
                            {
                                res.AppendHeader("Access-Control-Allow-Credentials", "true");//必须
                            }
                        }
                    }
                }
            }
            return true;
        }

        #endregion
    }
}
