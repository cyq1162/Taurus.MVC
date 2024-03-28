using System;
using CYQ.Data;
using CYQ.Data.Tool;
using Taurus.Mvc;
using System.Web;
using CYQ.Data.Json;
namespace Taurus.Plugin.Auth
{
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
            string config = AuthConfig.Auth;
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
                    if (action.Data.Columns.Contains(user.UserName))
                    {
                        other = DBTool.Keyword(user.UserName, action.DataBaseType) + "=@UserName";
                    }
                    if (action.Data.Columns.Contains(user.Mobile))
                    {
                        if (other != "") { other += " or "; }
                        other += DBTool.Keyword(user.Mobile, action.DataBaseType) + "=@UserName";
                    }
                    if (action.Data.Columns.Contains(user.Email))
                    {
                        if (other != "") { other += " or "; }
                        other += DBTool.Keyword(user.Email, action.DataBaseType) + "=@UserName";
                    }
                    where = status + string.Format("({0})", other);
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

                            if (!pwd.EndsWith("=2") && AppConfig.Tool.EncryptKey != "")
                            {
                                action.Set(user.Password, EncryptHelper.Encrypt(password));//重新加密密码
                                action.Update(where);//更新信息。
                            }
                            //获取角色名称
                            string roleIDs = action.Get<string>(user.RoleID, "").Replace(',', '_');
                            token = EncryptHelper.Encrypt(DateTime.Now + "," + userID + "," + userName + "," + fullName + "," + roleIDs);
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
            tokenCookie.Domain = AppConfig.XHtml.Domain;
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
                if (DateTime.TryParse(GetTokenValue(0), out d) && d.AddHours(user.TokenExpireTime) >= DateTime.Now)
                {
                    return true;
                }
                return false;
            }
        }
        /// <summary>
        /// Token 创建时间
        /// </summary>
        public static DateTime TokenCreateTime
        {
            get
            {
                DateTime d;
                DateTime.TryParse(GetTokenValue(0), out d);
                return d;
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
                var request = HttpContext.Current.Request;
                string token = request.GetHeader("token") ?? request.GetForm("token") ?? request.GetQueryString("token");
                if (string.IsNullOrEmpty(token))
                {
                    HttpCookie tokenCookie = request.Cookies[AuthConst.CookieTokenName];
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
}
