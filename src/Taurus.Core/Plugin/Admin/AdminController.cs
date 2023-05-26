using System;
using Taurus.Mvc;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Admin
{

    /// <summary>
    /// Taurus Admin Management Center。
    /// </summary>
    internal partial class AdminController : Controller
    {
        protected override string HtmlFolderName
        {
            get
            {
                return AdminConfig.HtmlFolderName;
            }
        }
        /// <summary>
        /// 账号检测是否登陆状态
        /// </summary>
        /// <returns></returns>
        public override bool BeforeInvoke()
        {
            string nameLower = MethodName.ToLower();
            switch (nameLower)
            {
                //无界面
                case "logout":
                case "btnsaveconfig":
                    break;
                default:
                    if (View == null)
                    {
                        return false;
                    }
                    break;
            }
            switch (nameLower)
            {
                case "login":
                    return true;
                default:
                    string loginName = Convert.ToString(Context.Session["login"]);
                    string isAdmin = Convert.ToString(Context.Session["isadmin"]);
                    if (string.IsNullOrEmpty(loginName) || string.IsNullOrEmpty(isAdmin))
                    {
                        if (nameLower == "btnsaveconfig")
                        {
                            Write("Login account has expired.", false);
                        }
                        else
                        {
                            //检测账号密码，跳转登陆页
                            Response.Redirect("login");
                        }
                        return false;
                    }
                    else if (View != null)
                    {
                        View.KeyValue.Set("LoginName", loginName);
                    }
                    break;
            }
            if (BtnName.StartsWith("btn") || nameLower.StartsWith("btn"))
            {
                if (Convert.ToString(Context.Session["isadmin"]) != "1")
                {
                    if (View != null)
                    {
                        //检测账号密码，跳转登陆页
                        View.Set("msg", "Account is readonly.");
                    }
                    else
                    {
                        Write("Account is readonly.", false);
                    }
                    return false;
                }
            }
            return true;
        }


        private string GetMsTypeText()
        {

            if (MsConfig.IsRegCenterOfMaster)
            {
                return "Register Center of Master" + (MsConfig.Server.IsEnable ? " ( Running )" : " ( Stopped )");
            }
            else if (MsConfig.IsRegCenter)
            {
                return "Register Center of Slave" + (MsConfig.Server.IsEnable ? " ( Running )" : " ( Stopped )") + (Server.IsLiveOfMasterRC ? "" : " - ( Master connection refused )");
            }
            else if (MsConfig.IsGateway)
            {
                return "Gateway" + (MsConfig.Server.IsEnable ? " ( Running )" : " ( Stopped )") + (Server.IsLiveOfMasterRC ? "" : " - ( Register center connection refused )");
            }
            else if (MsConfig.IsClient)
            {
                return "Client of MicroService" + (MsConfig.Client.IsEnable ? " ( Running )" : " ( Stopped )") + (Client.IsLiveOfMasterRC ? "" : " - ( Register center connection refused )");
            }
            else
            {
                return "None";
            }

        }

        private string GetRouteModeText()
        {
            switch (MvcConfig.RouteMode)
            {
                case 1:
                    return "1 【/controller/method】";
                case 2:
                    return "2 【/module/controller/method】";
            }
            return "0 【/method】 (code in DefaultController.cs)";
        }

        private string GetOnlineText(bool isAdminKey)
        {
            if (IsAdmin)
            {
                return isAdminKey ? " 【online】" : "";
            }
            else
            {
                return isAdminKey ? "" : " 【online】";
            }
        }
        protected bool IsAdmin
        {
            get
            {
                return Context.Session["isadmin"] != null;
            }
        }
    }
}
