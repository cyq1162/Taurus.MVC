using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;

namespace Taurus.Controllers
{
    public class HomeController : Controller
    {
        public override void Default()
        {
            Context.Response.Redirect("/");
        }
        public void Index()
        {
        }
        public void About() { }
        public void Contact() { }
        public void WebAPI() { }
    }
}
