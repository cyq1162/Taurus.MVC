using System;
using System.Collections.Generic;
using System.Web;
using CYQ.Data;
using CYQ.Data.Tool;

namespace Taurus.Core
{
    internal static class QueryTool
    {
        #region 增加扩展后缀支持
        public static string GetDefaultUrl()
        {
            return AppConfig.GetApp(AppSettings.DefaultUrl, "");
        }

        public static string GetLocalPath(Uri uri)
        {
            string localPath = uri.LocalPath;
            string suffix = AppConfig.GetApp(AppSettings.Suffix, "");
            if (suffix != "" && localPath.EndsWith(suffix))
            {
                return localPath.Replace(suffix, "");
            }
            return localPath;
        }
        public static bool IsRunProxySuccess
        {
            get
            {
                if (HttpContext.Current != null)
                {
                    return HttpContext.Current.Items.Contains("IsRunProxySuccess");
                }
                return false;
            }
            set
            {

                if (value)
                {
                    if (!HttpContext.Current.Items.Contains("IsRunProxySuccess"))
                    {
                        HttpContext.Current.Items.Add("IsRunProxySuccess", 1);
                    }
                }
                else if (HttpContext.Current.Items.Contains("IsRunProxySuccess"))
                {
                    HttpContext.Current.Items.Remove("IsRunProxySuccess");
                }
            }
        }
        /// <summary>
        /// 是否请求微服务注册中心
        /// </summary>
        /// <returns></returns>
        public static bool IsCallMicroServiceReg(Uri uri)
        {
            return uri.LocalPath.ToLower().Contains("/microservice/");
        }
        /// <summary>
        /// 是否常规走MVC调用流程
        /// </summary>
        /// <returns></returns>
        public static bool IsCallMvc(Uri uri)
        {
            return !string.IsNullOrEmpty(AppConfig.GetApp(AppSettings.Controllers)) || IsCallMicroServiceReg(uri);//有配置时才启动MVC，否则默认仅启动微服务。
        }
        public static bool IsTaurusSuffix(Uri uri)
        {
            string localPath = uri.LocalPath;
            string suffix = AppConfig.GetApp(AppSettings.Suffix, "");
            if (suffix != "" && localPath.EndsWith(suffix))
            {
                return true;
            }
            return localPath.IndexOf('.') == -1;
        }
        public static bool IsAllowCORS()
        {
            return AppConfig.GetAppBool(AppSettings.IsAllowCORS, true);
        }
        #endregion
        /// <summary>
        /// 是否使用子目录部署网站
        /// </summary>
        public static bool IsSubAppSite(Uri uri)
        {
            string ui = AppConfig.GetApp(AppSettings.SubAppName, string.Empty).ToLower();
            if (ui != string.Empty)
            {
                ui = ui.Trim('/');
                string localPath = uri.LocalPath.Trim('/').ToLower();
                return localPath == ui || localPath.StartsWith(ui + "/");
            }
            return false;
        }

        public static T Query<T>(string key)
        {
            return Query<T>(key, default(T), false);
        }
        public static T Query<T>(string key, T defaultValue, bool filter)
        {
            string value = HttpContext.Current.Request[key] ?? HttpContext.Current.Request.QueryString[key] ?? HttpContext.Current.Request.Headers[key];
            if (value == null && HttpContext.Current.Request.Files != null && HttpContext.Current.Request.Files[key] != null)
            {
                object o = HttpContext.Current.Request.Files[key];
                return (T)o;
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
                    result = ChangeType(value, t);
                }
                catch
                {
                    return defaultValue;
                }

            }
            return (T)result;
        }

        internal static object ChangeType(object value, Type t)
        {
            return ConvertTool.ChangeType(value, t);
        }
        /// <summary>
        /// 过滤一般的字符串
        /// </summary>
        /// <param name="strFilter"></param>
        /// <returns></returns>
        public static string FilterValue(string strFilter)
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
}
