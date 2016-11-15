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
            Write("<a href='/index'>Hello：</a>");
            Write("World!", true);
        }
    }
}
