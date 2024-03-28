using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using CYQ.Data;
using CYQ.Data.Json;
using CYQ.Data.Table;
using CYQ.Data.Tool;
using Taurus.Mvc;
using Taurus.Mvc.Attr;

namespace Taurus.Plugin.Auth
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
        public static bool CheckToken(Controller controller)
        {
            if (!AuthHelper.TokenIsValid)
            {
                controller.Write(JsonHelper.OutResult("success", false, "msg", "token is invalid.", "code", 10001));
                return false;
            }
            return true;
        }
    }


    
  
}
