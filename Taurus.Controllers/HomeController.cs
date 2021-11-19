using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Taurus.Core;

namespace Taurus.Controllers
{
    public class HomeController : Controller
    {
        public override void Default()
        {
            Context.Response.Redirect("/");
        }
        //public override bool BeforeInvoke(string methodName)
        //{
        //    ViewEngine.
        //    return true;
        //} 
        public void Index()
        {
            HttpCookie cookie = new HttpCookie("taurus");
            cookie.Value = "taurus.Test";
            Response.Cookies.Add(cookie);
            //string path = @"C:\Users\Administrator\Pictures\temp\timg.jpg";
            //HttpPostedFile hpf = Taurus.Core.HttpPostedFileExtend.Create(path);
            //hpf.SaveAs(System.IO.Path.GetDirectoryName(path) + "\\ddd.jpg");
        }
        public void About() { }
        public void Contact() { }
        public void WebAPI() { }
    }
}
