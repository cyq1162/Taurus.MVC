using System;
using System.Collections.Generic;
using System.Web;
using CYQ.Data;
using CYQ.Data.Tool;

namespace Taurus.Mvc
{
    /// <summary>
    /// 对外提供基本的参数获取功能。
    /// </summary>
    public static partial class WebTool
    {
        #region 增加扩展后缀支持
        internal static string GetLocalPath(Uri uri)
        {
            string localPath = uri.LocalPath;
            string suffix = MvcConfig.Suffix;
            if (suffix != "" && localPath.EndsWith(suffix))
            {
                return localPath.Replace(suffix, "");
            }
            return localPath;
        }
        internal static bool IsRunToEnd(HttpContext context)
        {
            if (context != null)
            {
                return context.Items.Contains("IsRunToEnd");
            }
            return false;
        }
        internal static void SetRunToEnd(HttpContext context)
        {
            if (context!=null && !context.Items.Contains("IsRunToEnd"))
            {
                context.Items.Add("IsRunToEnd", 1);
            }

        }

        /// <summary>
        /// 是否请求微服务注册中心
        /// </summary>
        /// <returns></returns>
        internal static bool IsCallMicroServiceReg(Uri uri)
        {
            return uri.LocalPath.ToLower().Contains("/microservice/");
        }
        /// <summary>
        /// 是否常规走MVC调用流程
        /// </summary>
        /// <returns></returns>
        internal static bool IsCallMvc(Uri uri)
        {
            return !string.IsNullOrEmpty(MvcConfig.Controllers) || IsCallMicroServiceReg(uri);//有配置时才启动MVC，否则默认仅启动微服务。
        }

        /// <summary>
        /// 当前请求是否Mvc处理范围。
        /// </summary>
        internal static bool IsTaurusSuffix(Uri uri)
        {
            string localPath = uri.LocalPath;
            return IsTaurusSuffix(localPath);
        }
        internal static bool IsTaurusSuffix(string localPath)
        {
            string suffix = MvcConfig.Suffix;
            if (suffix != "" && localPath.EndsWith(suffix))
            {
                return true;
            }
            return localPath.IndexOf('.') == -1;
        }
            #endregion
            /// <summary>
            /// 是否使用子目录部署网站
            /// </summary>
            internal static bool IsSubAppSite(Uri uri)
        {
            string ui =MvcConfig.SubAppName.ToLower();
            if (ui != string.Empty)
            {
                ui = ui.Trim('/');
                string localPath = uri.LocalPath.Trim('/').ToLower();
                return localPath == ui || localPath.StartsWith(ui + "/");
            }
            return false;
        }

       

        /// <summary>
        /// 过滤一般的字符串
        /// </summary>
        /// <param name="strFilter"></param>
        /// <returns></returns>
        internal static string FilterValue(string strFilter)
        {
            if (strFilter == null)
                return "";
            string returnValue = strFilter;
            string[] filterChar = new string[] { "\'", ",", "(", ")", ";", "\"" };// ">", "<", "=",
            for (int i = 0; i < filterChar.Length; i++)
            {
                returnValue = returnValue.Replace(filterChar[i], "");
            }
            return returnValue.Trim(' ');
        }
    }


    public static partial class WebTool
    {
        /// <summary>
        /// 获取 Web 请求参数
        /// </summary>
        public static T Query<T>(string key)
        {
            return Query<T>(key, default(T), false);
        }
        /// <summary>
        /// 获取 Web 请求参数
        /// </summary>
        public static T Query<T>(string key, T defaultValue, bool filter)
        {
            var files = HttpContext.Current.Request.Files;
            string value = HttpContext.Current.Request[key] ?? HttpContext.Current.Request.QueryString[key] ?? HttpContext.Current.Request.Headers[key];
            if (value == null && files != null && files[key] != null)
            {
                object file = files[key];
                if (typeof(T) == typeof(string))
                {
                    file = ((HttpPostedFile)file).FileName;
                }
                return (T)file;
            }
            return ChangeValueType<T>(value, defaultValue, filter);
        }
        internal static T ChangeValueType<T>(string value, T defaultValue, bool filter)
        {

            if (value == null) { return defaultValue; }
            value = value.Trim();
            object result = null;
            Type t = typeof(T);
            if (t.Name == "String")
            {
                if (filter)
                {
                    result = FilterValue(value);
                }
                else
                {
                    if (value.IndexOf('+') > -1)
                    {
                        string reKey = "[#{@!}#]";
                        string text = value.Replace("+", reKey);//
                        result = HttpContext.Current.Server.UrlDecode(text).Replace(reKey, "+");
                    }
                    else
                    {
                        result = HttpContext.Current.Server.UrlDecode(value);
                    }

                }
            }
            else
            {
                try
                {
                    result = ConvertTool.ChangeType(value, t);
                }
                catch
                {
                    return defaultValue;
                }

            }
            return (T)result;
        }
    }
}
