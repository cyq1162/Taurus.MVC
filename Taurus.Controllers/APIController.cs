using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;

namespace Taurus.Controllers
{
   

    public partial class APIController : Controller
    {
        protected override void BeforeInvoke(string methodName)
        {
            CancelLoadHtml = true;
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
        [HttpGet]
        public void GetData()
        {
            Write("your data :" + Query<string>("un"), true);
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
