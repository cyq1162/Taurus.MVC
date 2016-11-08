using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;

namespace Taurus.Controllers
{
    public class APIController : Controller
    {
        public void SayHello()
        {
            Write("<a href='/index'>Hello：</a>");
            Write("World!", true);
        }
    }
}
