using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using System.IO;
using System.Web.UI;
using System.Net;
using System.Data;
using CYQ.Data;

namespace Taurus.Core
{
    internal static class QueryTool
    {
        /// <summary>
        /// 是否使用子目录部署网站
        /// </summary>
        public static bool IsUseUISite
        {
            get
            {
                string ui = AppConfig.GetApp("UI", string.Empty).ToLower();
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
            if (HttpContext.Current.Request[key] == null) { return defaultValue; }
            object result;
            if (typeof(T).Name == "Int32")
            {
                int _result = 0;
                if (!int.TryParse(HttpContext.Current.Request[key], out _result))
                {
                    return defaultValue;
                }
                result = _result;
            }
            else
            {
                if (filter)
                {
                    result = FilterValue(HttpContext.Current.Request[key]);
                }
                else
                {
                    string reKey = "[#{@!}#]";
                    string text = HttpContext.Current.Request[key].Trim().Replace("+", reKey);//
                    result = HttpContext.Current.Server.UrlDecode(text).Replace(reKey, "+");
                }
            }
            return (T)result;
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
