using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;

namespace Taurus.Controllers
{

    /// <summary>
    /// API 接口
    /// </summary>
    [Token]
    public partial class APIController : Controller
    {
        public override bool BeforeInvoke(string methodName)
        {
            CancelLoadHtml = true;
            return true;
        }
        /// <summary>
        /// 重写此方法时，此CheckToken的优先级>DefaultController中的静态方法CheckToken
        /// </summary>
        /// <remarks>aaaa</remarks>
        /// <returns></returns>
        public override bool CheckToken()
        {
            string token = Query<string>("token");
            bool result = false;
            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    byte[] data = Convert.FromBase64String(token);
                    string text = System.Text.Encoding.UTF8.GetString(data);
                    result = text.StartsWith("Taurus:");
                }
                catch
                {
                    Write("token error!", false);
                    return false;

                }

            }
            if (!result)
            {
                Write("can't find token!", false);
            }
            return result;
        }

        /// <summary>
        /// 获取Token
        /// </summary>
        /// <param name="un">用户名</param>
        /// <param name="pwd">密码</param>
        /// <returns>{success:true:msg:"tokenString..."}</returns>
        [HttpGet, Require("un", true, RegexConst.Mobile), Require("pwd")]
        public void GetToken(string un, string pwd)
        {
            //is required. is invalid. 判断 是否：中文
            //CheckFormat("{0}不能为空&{0}格式错误", @"un&用户名&^1[3|4|5|8][0-9]\d{8}$", @"pwd&密码&^[\u0391-\uFFE5]+$");
            //string userName = Query<string>("un");
            //string pwd = Query<string>("pwd");
            if (!string.IsNullOrEmpty(un) && !string.IsNullOrEmpty(pwd))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes("Taurus:" + un);
                string base64 = Convert.ToBase64String(data);
                Write(base64);
            }
            else
            {
                Write("UserName or Password Error");
            }
        }

        [Token, HttpGet]
        public void GetDataWithToken(List<AB> unList, string a, int? b, AB ab)
        {
            Write("GetDataWithToken A:" + unList[0].A + " B:" + unList[0].B, true);
        }
        public void GetDataWithNoToken(List<AB> unList, string a, int? b, AB ab)
        {
            Write("GetDataWithNoToken A:" + unList[0].A + " B:" + unList[0].B, true);
        }
        // [Token, HttpPost]
        [Require("ab.a"), Require("ab.b"), Require("unList.0.b")]
        public void GetDataOnlyPost(List<AB> unList, string a, int? b, AB ab)
        {
            Write("GetDataOnlyPost A:" + unList[0].A + " B:" + unList[0].B, true);
        }


        //public void GetData(AB un)
        //{
        //    Write("your data A:" + un.A, true);
        //    Write("your data B:" + un.B, true);
        //}
        //public void GetData(List<AB> unList,string a,int? b,AB ab)
        //{
        //    Write("your data A:" + unList[0].A+" your data B:" + unList[0].B, true);
        //}
        //itlinks.cn/user?uid=666 itlinks.cn/user/uid/666
        public void Get()
        {
            int uid = Query<int>("uid");
        }

        public void Get(int uid)
        {

        }

    }

    public partial class APIController
    {
        //其它示例代码 
        public class AB
        {
            public string A;
            public string B;
        }
        public override void Default()
        {
            List<AB> list = JsonHelper.ToList<AB>(GetJson());
            AB ab = GetEntity<AB>();
            Write(GetJson());
        }
    }
}
