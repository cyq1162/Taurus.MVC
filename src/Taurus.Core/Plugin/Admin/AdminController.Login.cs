using CYQ.Data.Tool;
using System;
using System.Web;
using Taurus.Mvc;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 登陆退出
    /// </summary>
    internal partial class AdminController
    {
        /// <summary>
        /// VS2022 神奇的Session为Null，改Cookie兼容处理。
        /// </summary>
        private void SetLoginValue(string key, string value)
        {
            key = "taurus.plugin." + key;
            if (Context.Session != null)
            {
                Context.Session[key] = null;
            }
            else
            {
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
                Context.Response.Cookies.Add(cookie);
            }
        }
        /// <summary>
        /// VS2022 神奇的Session为Null，做兼容处理。
        /// </summary>
        private string GetLoginValue(string key)
        {
            key = "taurus.plugin." + key;
            if (Context.Session != null)
            {
                Convert.ToString(Context.Session[key]);
            }
            else
            {
                HttpCookie cookie = Context.Request.Cookies[key];
                if (cookie != null)
                {
                    return EncryptHelper.Decrypt(cookie.Value, MvcConst.HostIP);
                }
            }
            return string.Empty;
        }

        public void Logout()
        {
            SetLoginValue("islogin", null);
            SetLoginValue("isadmin", null);
            Response.Redirect("login");
        }
        public void Login()
        {
            if (!string.IsNullOrEmpty(GetLoginValue("islogin")) && !string.IsNullOrEmpty(GetLoginValue("isadmin")))
            {
                Response.Redirect("index");
            }
            if (!IsHttpPost)
            {
                string[] items = IO.Read(AdminConst.AccountPath).Split(',');
                string name = items.Length == 2 ? items[0] : AdminConfig.UserName;
                View.Set("uid", name);
            }
        }
        public void BtnLogin()
        {
            string uid = Query<string>("uid");
            string pwd = Query<string>("pwd");
            bool isOK = uid == AdminConfig.UserName && pwd == AdminConfig.Password;

            if (!isOK)
            {
                string[] items = IO.Read(AdminConst.AccountPath).Split(',');
                isOK = items.Length == 2 && items[0] == uid && items[1] == pwd;
                if (isOK)
                {
                    SetLoginValue("isadmin", "0");
                }
            }
            else
            {
                SetLoginValue("isadmin", "1");
            }
            if (isOK)
            {
                SetLoginValue("islogin", uid);
                Response.Redirect("index");
                return;
            }
            View.Set("msg", "user or password is error.");
        }
    }
}
