using System;
using System.Collections.Generic;
using System.Web;
using CYQ.Data;
using CYQ.Data.Tool;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Admin;
using Taurus.Plugin.Doc;
using System.Text;

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
        /// <summary>
        /// 是否系统内部Url（不转发）
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal static bool IsSysInternalUrl(Uri uri, Uri referrerUrl)
        {
            return IsCallMicroService(uri) || IsCallAdmin(uri, referrerUrl) || IsCallDoc(uri, referrerUrl);
        }

        /// <summary>
        /// 是否请求微服务
        /// </summary>
        /// <returns></returns>
        internal static bool IsCallMicroService(Uri uri)
        {
            return uri != null && IsCallMicroService(uri.LocalPath);
        }
        internal static bool IsCallMicroService(string localPath)
        {
            if (MsConfig.IsServer && MsConfig.Server.IsEnable)
            {
                return localPath.ToLower().Contains("/" + MsConfig.Server.RcPath.Trim('/', '\\') + "/");
            }
            if (MsConfig.IsClient && MsConfig.Client.IsEnable)
            {
                return localPath.ToLower().Contains("/" + MsConfig.Client.RcPath.Trim('/', '\\') + "/");
            }
            return false;
        }
        /// <summary>
        /// 是否请求后台管理中心
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal static bool IsCallAdmin(Uri uri, Uri referrerUrl)
        {
            return (uri != null && IsCallAdmin(uri.LocalPath)) || (referrerUrl != null && IsCallAdmin(referrerUrl.LocalPath));
        }
        internal static bool IsCallAdmin(string localPath)
        {
            return AdminConfig.IsEnable && localPath.ToLower().Contains("/" + AdminConfig.Path.Trim('/', '\\') + "/");
        }

        /// <summary>
        /// 是否请求Doc接口测试
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal static bool IsCallDoc(Uri uri, Uri referrerUrl)
        {
            return (uri != null && IsCallDoc(uri.LocalPath)) || (referrerUrl != null && IsCallDoc(referrerUrl.LocalPath));
        }
        internal static bool IsCallDoc(string localPath)
        {
            return DocConfig.IsEnable && localPath.ToLower().Contains("/" + DocConfig.Path.Trim('/', '\\') + "/");
        }

        /// <summary>
        /// 是否常规走MVC调用流程
        /// </summary>
        /// <returns></returns>
        internal static bool IsCallMvc(Uri uri)
        {
            return !string.IsNullOrEmpty(MvcConfig.Controllers) || IsCallMicroService(uri);//有配置时才启动MVC，否则默认仅启动微服务。
        }

        /// <summary>
        /// 当前请求是否Mvc处理范围。
        /// </summary>
        internal static bool IsMvcSuffix(Uri uri)
        {
            string localPath = uri.LocalPath;
            return IsMvcSuffix(localPath);
        }
        internal static bool IsMvcSuffix(string localPath)
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
            string ui = MvcConfig.SubAppName.ToLower();
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
        private static char[] startingChars = new char[2] { '<', '&' };

        /// <summary>
        /// 安全检测（防脚本注入）
        /// </summary>
        /// <param name="s">被检测的字符串</param>
        /// <returns></returns>
        public static bool IsDangerousString(string s, out int matchIndex)
        {
            matchIndex = 0;
            int startIndex = 0;
            while (true)
            {
                int num = s.IndexOfAny(startingChars, startIndex);
                if (num < 0)
                {
                    return false;
                }
                if (num == s.Length - 1)
                {
                    break;
                }
                matchIndex = num;
                switch (s[num])
                {
                    case '<':
                        if (IsAtoZ(s[num + 1]) || s[num + 1] == '!' || s[num + 1] == '/' || s[num + 1] == '?')
                        {
                            return true;
                        }
                        break;
                    case '&':
                        if (s[num + 1] == '#')
                        {
                            return true;
                        }
                        break;
                }
                startIndex = num + 1;
            }
            return false;
        }
        private static bool IsAtoZ(char c)
        {
            if (c < 'a' || c > 'z')
            {
                if (c >= 'A')
                {
                    return c <= 'Z';
                }
                return false;
            }
            return true;
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
            var request = HttpContext.Current.Request;
            string value = request[key] ?? request.QueryString[key] ?? request.Headers[key];
            if (value == null)
            {
                var files = request.Files;
                if (files != null && files[key] != null)
                {
                    object file = files[key];
                    if (typeof(T) == typeof(string))
                    {
                        file = ((HttpPostedFile)file).FileName;
                    }
                    return (T)file;
                }
                return defaultValue;
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

    public static partial class WebTool
    {
        internal static void PrintRequestLog(HttpContext context, Exception err)
        {
            HttpRequest request = context.Request;
            StringBuilder sb = new StringBuilder();
            if (err != null)
            {
                sb.AppendLine(Log.GetExceptionMessage(err));
                sb.AppendLine("");
            }
            var headers = request.Headers;
            if (headers.Count > 0)
            {
                sb.AppendLine("-----------Headers-----------");
                foreach (string key in headers.AllKeys)
                {
                    if (key[0] != ':')
                    {
                        sb.AppendLine(key + " : " + headers[key]);
                    }
                }
            }
            var form = request.Form;
            if (form.Count > 0)
            {
                sb.AppendLine("-----------Forms-----------");
                foreach (string key in form.AllKeys)
                {
                    sb.AppendLine(key + " : " + form[key]);
                }
            }
            Log.WriteLogToTxt(sb.ToString(), err != null ? LogType.Taurus : LogType.Debug + "_PrintRequestLog");
        }
    }
}
