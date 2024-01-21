using CYQ.Data.Json;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Taurus.Mvc;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 提供一组对外API以使用Admin插件功能
    /// </summary>
    public class AdminAPI
    {
        #region 持久化配置
        public class Durable
        {
            /// <summary>
            /// 从文本中读取还原。
            /// </summary>
            internal static void Init(Dictionary<string, string> durableDic)
            {
                if (durableDic != null && durableDic.Count > 0)
                {
                    foreach (var item in durableDic)
                    {
                        durableConfig.Add(item.Key, item.Value);
                    }
                }
            }
            /// <summary>
            /// 独立的持久化配置。
            /// </summary>
            private static Dictionary<string, string> durableConfig = new Dictionary<string, string>();
            /// <summary>
            /// 临时修改的配置。
            /// </summary>
            private static Dictionary<string, string> tempConfig = new Dictionary<string, string>();
            public static void Add(string key, string value)
            {
                Add(key, value, true);
            }
            /// <summary>
            /// 添加持久化配置
            /// </summary>
            internal static void Add(string key, string value, bool isSaveToFile)
            {
                if (tempConfig.ContainsKey(key))
                {
                    tempConfig.Remove(key);
                }

                if (durableConfig.ContainsKey(key))
                {
                    durableConfig[key] = value;
                }
                else
                {
                    durableConfig.Add(key, value);
                }
                if (isSaveToFile)
                {
                    IO.Write(AdminConst.ConfigPath, JsonHelper.ToJson(durableConfig));
                }
            }

            /// <summary>
            /// 移除持久化，转为临时内存存储
            /// </summary>
            public static void Remove(string key)
            {
                Remove(key, null);
            }
            /// <summary>
            /// 移除持久化，转为临时内存存储
            /// </summary>
            /// <param name="key">移除的key</param>
            /// <param name="value">新的值</param>
            public static void Remove(string key, string value)
            {
                if (durableConfig.ContainsKey(key))
                {
                    if (value == null) { value = durableConfig[key]; }
                    durableConfig.Remove(key);
                    IO.Write(AdminConst.ConfigPath, JsonHelper.ToJson(durableConfig));
                }
                if (!tempConfig.ContainsKey(key) && value != null)
                {
                    tempConfig.Add(key, value);
                }
            }
            /// <summary>
            /// 检测是否包含持久化配置。
            /// </summary>
            public static bool ContainsKey(string key)
            {
                return durableConfig.ContainsKey(key);
            }
            /// <summary>
            /// 检测是否包含临时修改配置。
            /// </summary>
            public static bool ContainsTempKey(string key)
            {
                return tempConfig.ContainsKey(key);
            }
        }

        #endregion

        #region 扩展菜单存储

        public class ExtMenu
        {
            /// <summary>
            /// 内存菜单，不持久化。
            /// </summary>
            internal static MDictionary<string, string> menuList = new MDictionary<string, string>();
            /// <summary>
            /// 添加自定义菜单
            /// </summary>
            /// <param name="group">组名</param>
            /// <param name="name">菜单名</param>
            /// <param name="url">链接地址</param>
            public static void Add(string group, string name, string url)
            {
                string key = group + "," + name;
                if (!menuList.ContainsKey(key))
                {
                    menuList.Add(key, url);
                }
            }
            /// <summary>
            /// 移除自定义菜单
            /// </summary>
            /// <param name="group">组名</param>
            /// <param name="name">菜单名</param>
            public static void Remove(string group, string name)
            {
                string key = group + "," + name;
                menuList.Remove(key);
            }
        }

        #endregion

        #region 账号授权：登陆 - 退出

        public class Auth
        {
            public static bool Login(HttpContext context, string uid, string pwd)
            {
                bool isOK = uid == AdminConfig.UserName && pwd == AdminConfig.Password;

                if (!isOK)
                {
                    string[] items = IO.Read(AdminConst.AccountPath).Split(',');
                    isOK = items.Length == 2 && items[0] == uid && items[1] == pwd;
                    if (isOK)
                    {
                        SetValue(context, "isadmin", "0");
                    }
                }
                else
                {
                    SetValue(context, "isadmin", "1");
                }
                if (isOK)
                {
                    SetValue(context, "uid", uid);
                }
                return isOK;
            }
            public static void Logout(HttpContext context)
            {
                SetValue(context, "uid", null);
                SetValue(context, "isadmin", null);
                context.Response.Redirect("login");
            }
            /// <summary>
            /// 当前是否登陆状态
            /// </summary>
            /// <param name="isAutoGotoLogin">未登陆时，是否自动跳转到登陆界面</param>
            /// <returns></returns>
            public static bool IsOnline(HttpContext context, bool isAutoGotoLogin)
            {
                bool isOnline = true;
                if (string.IsNullOrEmpty(GetValue(context, "uid")))
                {
                    isOnline = false;
                    if (isAutoGotoLogin)
                    {
                        context.Response.Redirect("login?returnurl=" + context.Request.RawUrl);
                    }
                }
                if (string.IsNullOrEmpty(GetValue(context, "isadmin")))
                {
                    isOnline = false;
                    if (isAutoGotoLogin)
                    {
                        context.Response.Redirect("login?returnurl=" + context.Request.RawUrl);
                    }
                }
                return isOnline;
            }

            /// <summary>
            /// 获取当前登陆账号是否管理员
            /// </summary>
            public static bool GetIsAdmin(HttpContext context)
            {
                return GetValue(context, "isadmin") == "1";
            }

            /// <summary>
            /// 获取当前登陆账号
            /// </summary>
            public static string GetLoginName(HttpContext context)
            {
                return GetValue(context, "uid");
            }
            #region 内部实现
            /// <summary>
            /// VS2022 神奇的Session为Null，改Cookie兼容处理。
            /// </summary>
            internal static void SetValue(HttpContext context, string key, string value)
            {
                if (context.Session != null)
                {
                    context.Session[key] = value;
                }
                else
                {
                    key = "taurus.plugin." + key;
                    //使用Cookie，会共享端口，Cookie不提供端口隔离。
                    HttpCookie cookie = new HttpCookie(key, EncryptHelper.Encrypt(value, MvcConst.HostIP));
                    cookie.HttpOnly = true;
                    if (string.IsNullOrEmpty(value))
                    {
                        cookie.Expires = DateTime.Now.AddYears(-1);
                    }
                    else
                    {
                        cookie.Expires = DateTime.Now.AddDays(7);
                    }
                    context.Response.Cookies.Add(cookie);
                }
            }
            /// <summary>
            /// VS2022 神奇的Session为Null，做兼容处理。
            /// </summary>
            internal static string GetValue(HttpContext context, string key)
            {
                if (context.Session != null)
                {
                    return Convert.ToString(context.Session[key]);
                }
                else
                {
                    key = "taurus.plugin." + key;
                    HttpCookie cookie = context.Request.Cookies[key];
                    if (cookie != null)
                    {
                        return EncryptHelper.Decrypt(cookie.Value, MvcConst.HostIP);
                    }
                    return string.Empty;
                }

            }
            #endregion
        }

        #endregion
    }
}
