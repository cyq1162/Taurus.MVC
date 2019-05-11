using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using CYQ.Data;
using CYQ.Data.Table;
using CYQ.Data.Tool;

namespace Taurus.Core
{
    /// <summary>
    /// 用户授权 WebAPI端
    /// </summary>
    public partial class AuthController : Controller
    {
        /// <summary>
        /// 获取Token
        /// </summary>
        [Require("uid"), Require("pwd")]
        public void GetToken(string uid, string pwd)
        {
            string err;
            string token = AuthHelper.GetAuthToken(uid, pwd, out err);
            if (!string.IsNullOrEmpty(token))
            {
                Write(token, true);
            }
            else
            {
                Write(JsonHelper.OutResult("success", false, "msg", err, "code", 10000));
            }
        }

        /// <summary>
        /// 默认检测Token的方法。
        /// </summary>
        /// <param name="controller"></param>
        /// <returns></returns>
        public static bool CheckToken(IController controller)
        {
            if (!AuthHelper.TokenIsValid)
            {
                controller.Write(JsonHelper.OutResult("success", false, "msg", "token is invalid.", "code", 10001));
            }
            return true;
        }
    }
    /// <summary>
    /// 用户授权 MVC端的全局方法
    /// </summary>
    public partial class AuthHelper
    {
        static AuthUsers user;
        /// <summary>
        /// 初始化配置
        /// </summary>
        static AuthHelper()
        {
            string config = AppConfig.GetApp(AppSettings.Auth, "");
            if (!string.IsNullOrEmpty(config))
            {
                user = JsonHelper.ToEntity<AuthUsers>(config);
            }
            else
            {
                user = new AuthUsers();
            }
        }

        /// <summary>
        /// 获取授权Token（手机APP登陆调用此方法获取Token为登陆凭证）
        /// </summary>
        internal static string GetAuthToken(string userName, string password, out string errMsg)
        {
            string token = string.Empty;
            errMsg = "";
            using (MAction action = new MAction(user.TableName))
            {
                string status = "";
                if (action.Data.Columns.Contains(user.Status))
                {
                    status += user.Status + "=1 and ";
                }
                string where = string.Empty;
                if (action.DataBaseType == DataBaseType.Txt || action.DataBaseType == DataBaseType.Xml)
                {
                    where = status + user.UserName + string.Format("='{0}'", userName);
                }
                else
                {
                    action.SetPara("UserName", userName, System.Data.DbType.String);
                    string other = "";
                    if (action.Data.Columns.Contains(user.Mobile))
                    {
                        other += " or " + user.Mobile + "=@UserName";
                    }
                    if (action.Data.Columns.Contains(user.Email))
                    {
                        other += " or " + user.Email + "=@UserName";
                    }
                    where = status + string.Format("({0}=@UserName {1})", user.UserName, other);
                }
                if (action.Fill(where))
                {
                    if (action.Data.Columns.Contains(user.PasswordExpireTime) && action.Get<DateTime>(user.PasswordExpireTime, DateTime.MaxValue) < DateTime.Now)
                    {
                        errMsg = AuthConst.PasswordExpired;
                    }
                    else
                    {

                        string pwd = action.Get<string>(user.Password);
                        if (password == EncryptHelper.Decrypt(pwd))
                        {
                            string userID = action.Get<string>(action.Data.PrimaryCell.ColumnName);
                            string fullName = action.Get<string>(user.FullName, userName);

                            if (!pwd.EndsWith("=2") && AppConfig.EncryptKey != "")
                            {
                                action.Set(user.Password, EncryptHelper.Encrypt(password));//重新加密密码
                                action.Update(where);//更新信息。
                            }
                            //获取角色名称
                            string roleIDs = action.Get<string>(user.RoleID, "").Replace(',', '_');
                            token = EncryptHelper.Encrypt(DateTime.Now.AddHours(user.TokenExpireTime) + "," + userID + "," + userName + "," + fullName + "," + roleIDs);
                        }
                        else
                        {
                            errMsg = AuthConst.PasswordError;
                        }
                    }

                }
                else
                {
                    errMsg = AuthConst.UserNotExist;
                }
            }
            return token;
        }


        /// <summary>
        /// 用户登陆
        /// </summary>
        public static bool Login(string userName, string password, out string errMsg)
        {
            string token = GetAuthToken(userName, password, out errMsg);
            if (string.IsNullOrEmpty(token))
            {
                return false;
            }
            WriteCookie(token, userName);
            return true;
        }
        /// <summary>
        /// 用户注册
        /// </summary>
        /// <param name="msg">成功时返回主键ID，失败时返回错误信息</param>
        /// <returns></returns>
        public static bool Reg(string userName, string password, out string msg)
        {
            bool result = false;
            msg = "";
            using (MAction action = new MAction(user.TableName))
            {
                action.Set(user.UserName, userName);
                action.Set(user.Password, password);
                try
                {
                    result = action.Insert(true, InsertOp.ID);
                    if (result)
                    {
                        msg = action.Get<string>(action.Data.PrimaryCell.ColumnName);
                    }
                    else
                    {
                        msg = action.DebugInfo;
                    }
                }
                catch (Exception err)
                {
                    msg = err.Message;
                }

            }
            return result;
        }

        /// <summary>
        /// 修改密码
        /// </summary>
        /// <param name="password"></param>
        /// <returns></returns>
        public static bool ChangePassword(string password)
        {
            using (MAction action = new MAction(user.TableName))
            {
                action.Set(user.Password, EncryptHelper.Encrypt(password));
                return action.Update(UserID);
            }
        }

        /// <summary>
        /// 退出
        /// </summary>
        public static void Logout()
        {
            HttpResponse response = HttpContext.Current.Response;
            //清除token
            HttpCookie tokenCookie = new HttpCookie(AuthConst.CookieTokenName, "");
            // HttpCookie userNameCookie = new HttpCookie("aries_user");//为了保留记住用户名功能，不清用户名Cookie
            tokenCookie.Expires = DateTime.Now.AddYears(-1);
            response.Cookies.Add(tokenCookie);
        }

        private static void WriteCookie(string token, string userName)
        {
            SetCookie(AuthConst.CookieTokenName, token, 24);
            SetCookie(AuthConst.CookieUserName, userName, 24 * 7);
        }
        public static void SetCookie(string name, string value, int hours)
        {
            HttpCookie cookie = new HttpCookie(name, value);// { HttpOnly = !local };
            cookie.Domain = AppConfig.XHtml.Domain;
            cookie.Expires = DateTime.Now.AddHours(hours);
            if (HttpContext.Current != null)
            {
                HttpContext.Current.Response.Cookies.Add(cookie);
            }
        }
        public static string GetCookieValue(string name)
        {
            HttpCookie cookie = HttpContext.Current.Request.Cookies[name];
            if (cookie != null)
            {
                return cookie.Value;
            }
            return string.Empty;
        }





    }

    public partial class AuthHelper
    {
        #region 对外属性
        /// <summary>
        /// Token是否有效
        /// </summary>
        public static bool TokenIsValid
        {
            get
            {
                DateTime d;
                if (DateTime.TryParse(GetTokenValue(0), out d) && d >= DateTime.Now)
                {
                    return true;
                }
                return false;
            }
        }
        public static string UserID
        {
            get
            {
                return GetTokenValue(1);
            }
        }
        /// <summary>
        /// 获取当前登陆账号的登陆ID
        /// </summary>
        public static string UserName
        {
            get
            {
                string loginID = GetTokenValue(2);
                if (string.IsNullOrEmpty(loginID))
                {
                    loginID = GetCookieValue(AuthConst.CookieUserName);
                }
                return loginID;
            }
        }
        /// <summary>
        /// 获取当前登陆账号的用户名称
        /// </summary>
        public static string FullName
        {
            get
            {
                return GetTokenValue(3);
            }
        }
        /// <summary>
        /// 用户的角色IDs
        /// </summary>
        public string Role
        {
            get
            {
                return GetTokenValue(4).Replace('_', ',');
            }
        }

        /// <summary>
        /// 获取当前登陆账号的Token（先取Request，再取Cookie值）
        /// </summary>
        public static string Token
        {
            get
            {
                string token = QueryTool.Query<string>("token");
                if (string.IsNullOrEmpty(token))
                {
                    HttpCookie tokenCookie = HttpContext.Current.Request.Cookies[AuthConst.CookieTokenName];
                    if (tokenCookie != null)
                    {
                        token = tokenCookie.Value;
                    }
                }
                return token;
            }
            private set
            {
                HttpCookie tokenCookie = HttpContext.Current.Request.Cookies[AuthConst.CookieTokenName];
                if (tokenCookie != null)
                {
                    tokenCookie.HttpOnly = false;
                    tokenCookie.Expires = DateTime.Now.AddDays(-1);
                    tokenCookie.Domain = AppConfig.XHtml.Domain;
                    HttpContext.Current.Response.Cookies.Add(tokenCookie);
                }
            }
        }

        #endregion

        private static string GetTokenValue(int index)
        {
            string token = Token;
            if (!string.IsNullOrEmpty(token))
            {
                string text = EncryptHelper.Decrypt(token);
                if (!string.IsNullOrEmpty(text))
                {
                    string[] items = text.Split(',');
                    if (items.Length > index)
                    {
                        return items[index];
                    }
                }
            }
            return string.Empty;
        }
    }

    internal class AuthUsers
    {
        public string TableName = "";
        public string UserName = "UserName";
        public string FullName = "FullName";
        public string Password = "Password";
        public string Status = "Status";
        public string PasswordExpireTime = "PasswordExpireTime";
        public string Email = "Email";
        public string Mobile = "Mobile";
        public string RoleID = "RoleID";
        public int TokenExpireTime = 24;
    }
    /// <summary>
    /// 授权相关的信息
    /// </summary>
    public class AuthConst
    {
        /// <summary>
        /// 提示密码已过期
        /// </summary>
        public static string PasswordExpired = "password has expired.";
        /// <summary>
        /// 密码错误
        /// </summary>
        public static string PasswordError = "password error.";
        /// <summary>
        /// 账号不存在
        /// </summary>
        public static string UserNotExist = "user does not exist.";
        /// <summary>
        /// cookie token名称
        /// </summary>
        public static string CookieTokenName = "taurus_token";
        /// <summary>
        /// cookie user名称
        /// </summary>
        public static string CookieUserName = "taurus_user";
    }
}
