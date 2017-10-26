using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;

namespace Taurus.Controllers
{
   

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
        /// <returns></returns>
        public override bool CheckToken()
        {
            string token = Query<string>("token");
            bool result = false;
            if (!string.IsNullOrEmpty(token))
            {
                byte[] data = Convert.FromBase64String(token);
                string text = System.Text.Encoding.UTF8.GetString(data);
                result = text.StartsWith("Taurus:");
            }
            if (!result)
            {
                Write("can't find token!", false);
            }
            return result;
        }
       
        public void GetToken()
        {
            CheckFormat("{0}不能为空&{0}格式错误", @"un&用户名&^1[3|4|5|8][0-9]\d{8}$", @"pwd&密码&^[\u0391-\uFFE5]+$");
            string userName = Query<string>("un");
            string pwd = Query<string>("pwd");
            if (!string.IsNullOrEmpty(userName) && !string.IsNullOrEmpty(pwd))
            {
                byte[] data = System.Text.Encoding.UTF8.GetBytes("Taurus:" + userName);
                string base64 = Convert.ToBase64String(data);
                Write(base64);
            }
            else
            {
                Write("UserName or Password Error");
            }
        }
        [Token]
        [HttpPost]
        //public void GetData(AB un)
        //{
        //    Write("your data A:" + un.A, true);
        //    Write("your data B:" + un.B, true);
        //}
        public void GetData(List<AB> unList,string a,int? b,AB ab)
        {
            Write("your data A:" + unList[0].A+" your data B:" + unList[0].B, true);
        }
    }

    public partial class APIController
    {
        //其它示例代码 
        public class AB
        {
            public string A { get; set; }
            public string B { get; set; }
        }
        public override void Default()
        {
            List<AB> list = JsonHelper.ToList<AB>(GetJson());
            AB ab = GetEntity<AB>();
            Write(GetJson());
        }
    }
}
