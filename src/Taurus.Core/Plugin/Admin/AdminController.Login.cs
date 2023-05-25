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
            Context.Session["login"] = null;
            Context.Session["isadmin"] = null;
            Response.Redirect("login");
        }
        public void Login()
        {
            if (Context.Session["login"] != null && Context.Session["isadmin"] != null)
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
                    Context.Session["isadmin"] = "0";
                }
            }
            else
            {
                Context.Session["isadmin"] = "1";
            }
            if (isOK)
            {
                Context.Session["login"] = uid;
                Response.Redirect("index");
                return;
            }
            View.Set("msg", "user or password is error.");
        }
    }
}
