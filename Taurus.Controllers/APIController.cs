using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;

namespace Taurus.Controllers
{
    public class AB
    {
        public string A { get; set; }
        public string B { get; set; }
    }

    public class APIController : Controller
    {
        protected override void BeforeInvoke(string methodName)
        {
            CancelLoadHtml = true;
        }
        public override void Default()
        {
            List<AB> list = JsonHelper.ToList<AB>(GetJson());
            AB ab = GetEntity<AB>();
            Write(GetJson());
        }
        [Token]
        public void SayHello()
        {
            Write("<a href='/index'>Hello：</a>");
            Write("World!", true);
        }
        public void SayHello2()
        {
           
            List<string> list = new List<string>();
            for (int i = 0; i < 5; i++)
            {
                list.Add(i.ToString());
            }
            Write(list);
            Write("<a href='/index'>Hello：</a>");
            Write("World!", true);
        }
    }
}
