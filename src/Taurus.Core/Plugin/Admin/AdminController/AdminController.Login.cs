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


        public void Logout()
        {
            AdminAPI.Auth.Logout(this.Context);
        }
        public void Login()
        {
            if (AdminAPI.Auth.IsOnline(this.Context, false))
            {
                string url = Query<string>("returnurl", "index");
                Response.Redirect(url);
            }

            if (!IsHttpPost)
            {
                string[] items = AppDataIO.Read(AdminConst.AccountPath).Split(',');
                string name = items.Length == 2 ? items[0] : AdminConfig.UserName;
                View.Set("uid", name);
            }
        }
        public void BtnLogin()
        {
            string uid = Query<string>("uid");
            string pwd = Query<string>("pwd");
            if (AdminAPI.Auth.Login(this.Context, uid, pwd))
            {
                string url = Query<string>("returnurl", "index");
                Response.Redirect(url);
                return;
            }
            View.Set("msg", "username or password is error.");
        }
    }
}
