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
            if (CORSConfig.IsEnable)
            {
                var req = context.Request;
                if (req.HttpMethod == "OPTIONS")
                {
                    var res = context.Response;
                    res.StatusCode = 204;
                    if (CORSConfig.IsEnable)
                    {
                        res.AppendHeader("Access-Control-Allow-Methods", CORSConfig.Methods);
                        string origin = CORSConfig.Origin;
                        if (origin == "*")
                        {
                            origin = req.GetHeader("Origin");
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
                        if (req.GetHeader("Access-Control-Allow-Headers") != null)
                        {
                            res.AppendHeader("Access-Control-Allow-Headers", req.GetHeader("Access-Control-Allow-Headers"));
                        }
                        else if (req.GetHeader("Access-Control-Request-Headers") != null)
                        {
                            res.AppendHeader("Access-Control-Allow-Headers", req.GetHeader("Access-Control-Request-Headers"));
                        }
                    }
                    res.End();
                    return false;
                }

                if (req.UrlReferrer != null && req.Url.Authority != req.UrlReferrer.Authority)
                {
                    string origin = req.GetHeader("Origin");
                    if (!string.IsNullOrEmpty(origin))
                    {
                        var res = context.Response;
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
