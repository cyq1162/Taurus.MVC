using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;
using CYQ.Data;
using System.Web;
namespace Taurus.Controllers
{
    /// <summary>
    /// 默认控制器（此类不要动，partial类，可以在AriesController文件夹外建立自己的业务类）
    /// </summary>
    public partial class DefaultController : Controller
    {
        [HttpGet]
        public override void Default()
        {
            if (RouteConfig.RouteMode == 2 && Module == "api")
            {
                Context.RewritePath("/text" + Context.Request.RawUrl);
            }
            // Write("Hello world");
        }
        /// <summary>
        /// 用于登陆前的请求合法性验证，配合[Ack]属性
        /// </summary>
        public static bool CheckAck(IController controller, string methodName)
        {
            //需要自己实现Ack验证
            return controller.CheckFormat("ack Can't be Empty", "ack");

        }

        /// <summary>
        /// 用于需要登陆后的身份验证，配合[Token]属性
        /// </summary>
        public static bool CheckToken(IController controller, string methodName)
        {
            //需要自己实现，或者通过配置Taurus.Auth启动自带的验证（自带的注释掉此方法即可）。
            return controller.CheckFormat("token Can't be Empty", "token");
        }
        /// <summary>
        /// 全局【路由映射】
        /// </summary>
        public static string RouteMapInvoke(HttpRequest request)
        {
            if (request.Url.LocalPath.StartsWith("/api/") && RouteConfig.RouteMode == 2)
            {
                return "/test" + request.RawUrl;
            }
            return string.Empty;
        }
        /// <summary>
        /// 全局【方法执行前拦截】
        /// </summary>
        public static bool BeforeInvoke(IController controller, string methodName)
        {
            //MAction action = new MAction("Test1", "server=.;database=demo;uid=sa;pwd=123456");

            //action.BeginTransation();
            //action.Set("name", "google");
            //if (action.Insert())
            //{
            //    throw new Exception("aa");
            //}

            //if (controller.IsHttpPost)
            //{
            //    //拦截全局处理
            //    controller.Write(methodName + " NoACK");
            //}

            return true;
        }
        /// <summary>
        /// 全局【方法执行后业务】
        /// </summary>
        public static void EndInvoke(IController controller, string methodName)
        {

        }
    }
}
