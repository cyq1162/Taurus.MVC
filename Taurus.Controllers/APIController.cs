using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;

namespace Taurus.Controllers
{
    //[Token()]
    public class APIController : Controller
    {
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
