using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Net;
using System.Data;
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

        public static string GetLocalPath()
        {
            string localPath = HttpContext.Current.Request.Url.LocalPath;
            string suffix = AppConfig.GetApp(AppSettings.Suffix, "");
            if (suffix != "" && localPath.EndsWith(suffix))
            {
                return localPath.Replace(suffix, "");
            }
            return localPath;
        }
        public static bool IsTaurusSuffix()
        {
            string localPath = HttpContext.Current.Request.Url.LocalPath;
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
        public static bool IsSubAppSite
        {
            get
            {
                string ui = AppConfig.GetApp(AppSettings.SubAppName, string.Empty).ToLower();
                if (ui != string.Empty)
                {
                    ui = ui.Trim('/');
                    string localPath = HttpContext.Current.Request.Url.LocalPath.Trim('/').ToLower();
                    return localPath == ui || localPath.StartsWith(ui + "/");
                }
                return false;
            }
        }

        public static T Query<T>(string key)
        {
            return Query<T>(key, default(T), false);
        }
        public static T Query<T>(string key, T defaultValue, bool filter)
        {
            string value = HttpContext.Current.Request[key];
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
