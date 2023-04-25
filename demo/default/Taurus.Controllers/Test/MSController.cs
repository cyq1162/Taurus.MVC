using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Mvc;
using Taurus.Mvc.Attr;

namespace Taurus.Controllers.Test
{
    /// <summary>
    /// 微服务测试
    /// </summary>
    public class MSController : Controller
    {
        public override void Default()
        {
            Write("UrlReferrer : " + Convert.ToString(Request.UrlReferrer) + "\r\n<br/>");
            Write("X-Real-IP : " + Request.Headers["X-Real-IP"] + "\r\n<br/>");
            Write("X-Real-Port : " + Request.Headers["X-Real-Port"] + "\r\n<br/>");
            Write("X-Forwarded-For : " + Request.Headers["X-Forwarded-For"] + "\r\n<br/>");
            Write("REMOTE_ADDR : " + Request.ServerVariables["REMOTE_ADDR"] + "\r\n<br/>");
            Write("REMOTE_PORT : " + Request.ServerVariables["REMOTE_PORT"] + "\r\n<br/>");
            Write("App.RunUrl : " + MvcConfig.RunUrl + "\r\n<br/>");
            Write("Url.LocalPath : " + Request.Url.LocalPath + "\r\n<br/>");
            Write("Request.Url.Port  : " + Request.Url.Port + "\r\n<br/>");
            Write("UserHostAddress : " + Request.UserHostAddress.ToString() + "\r\n<br/>");
        }
        /// <summary>
        /// Get or Post 测试
        /// </summary>
        /// <param name="custom" type="header">post 消息</param>
        /// <param name="msg" required="true">post 消息</param>
        /// <param name="file" type="file">文件</param>
        /// <returns>返回Json数据</returns>
        [HttpGet]
        [HttpPost]
        [MicroService]
        public void Hello(string msg, System.Web.HttpPostedFile file)
        {
            if (file != null)
            {
                file.SaveAs(AppConfig.RunPath + file.FileName);
            }
            if (Request.UrlReferrer != null)
            {
                Write("UrlReferrer : " + Request.UrlReferrer.ToString() + "\r\n<br/>");
            }
            Write("UserHostAddress : " + Request.UserHostAddress.ToString() + "\r\n<br/>");
            Write("MicroService AppRunUrl: " + MvcConfig.RunUrl + Request.Url.LocalPath + " : " + Request.HttpMethod + " : " + msg ?? "Hello" + MicroService.MsConfig.Server.Name);
        }
        /// <summary>
        /// Get or Post 测试
        /// </summary>
        /// <param name="msg">post 消息</param>
        public void Hello2(string msg)
        {
            if (Request.UrlReferrer != null)
            {
                Write("UrlReferrer : " + Request.UrlReferrer.ToString() + "\r\n<br/>");
            }
            Write("UserHostAddress : " + Request.UserHostAddress.ToString() + "\r\n<br/>");
            Write("MicroService AppRunUrl : " + MvcConfig.RunUrl + Request.Url.LocalPath + " : " + Request.HttpMethod + " : " + msg ?? "Hello" + MicroService.MsConfig.Server.Name);
        }

        public void Cookie(string msg)
        {
            string key = "Test" + msg;
            System.Net.IPAddress ip;
            //if (Request.Url.Host != "localhost" && !System.Net.IPAddress.TryParse(Request.Url.Host, out ip))
            //{
            var cookie = Request.Cookies[key];
            if (cookie == null)
            {
                cookie = new System.Web.HttpCookie(key);
                cookie.Value = msg;
                cookie.Expires = DateTime.Now.AddYears(1);
                cookie.Domain = Request.Url.Host;
                Response.Cookies.Add(cookie);
                Write("MicroService : " + Request.Url.AbsoluteUri + " :Set Cookie Value :" + key + " port: " + Request.Url.Port);
            }
            else
            {
                Write("MicroService : " + Request.Url.AbsoluteUri + " :Get Cookie Value :" + cookie.Value + " port: " + Request.Url.Port);

            }
            return;
            ///}
            //Write("MicroService : " + Request.Url.AbsoluteUri + " : " + msg + " port: " + Request.Url.Port);
        }
    }
}
