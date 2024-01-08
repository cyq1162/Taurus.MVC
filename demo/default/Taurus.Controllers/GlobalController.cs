using Taurus.Mvc;
using System.Web;
using Taurus.Plugin.Limit;
using Taurus.Plugin.MicroService;

namespace Taurus.Controllers
{
    /// <summary>
    /// 全局控制器（适全全局事件处理）
    /// </summary>
    public partial class GlobalController : Taurus.Mvc.Controller
    {
        /// <summary>
        /// 所有寻址不到的请求都集中执行到此方法。
        /// </summary>
        public override void Default()
        {
            Response.StatusCode = 404;
            Write("404 - by GlobalController.Default()");
        }
        /// <summary>
        /// 用于所有的请求合法性验证，配合[Ack]属性
        /// 启用时：局部的先执行（若存在)，无局部才执行全局。
        /// </summary>
        public static bool CheckAck(Controller controller, string ack)
        {
            //需要自己实现Ack验证
            return AckLimit.IsValid(ack);

        }

        /// <summary>
        /// 用于需要登陆后的身份验证，配合[Token]属性
        /// 启用时：局部的先执行（若存在)，无局部才执行全局。
        /// </summary>
        public static bool CheckToken(Controller controller, string token)
        {
            //需要自己实现，或者通过配置Taurus.Auth启动自带的验证（自带的注释掉此方法即可）。
            return !string.IsNullOrEmpty(token);
        }

        /// <summary>
        /// 用于校验微服务的内部身份验证，配合[MicroService]属性
        /// 启用时：全局仅此一个生效，局部的失效。
        /// </summary>
        public static bool CheckMicroService(Controller controller, string serverKey)
        {
            return MsConfig.Server.RcKey == serverKey;
        }

        /// <summary>
        /// 全局【路由映射】
        /// 启用时：所有请求都进入此地做映射。
        /// </summary>
        public static string RouteMapInvoke(HttpRequest request)
        {
            //if (request.Url.LocalPath.StartsWith("/api/") && RouteConfig.RouteMode == 2)
            //{
            //    return "/test" + request.RawUrl;
            //}
            return string.Empty;
        }
        /// <summary>
        /// 全局【方法执行前拦截】
        /// 启用时：先全局，再执行局部（若存在）。
        /// </summary>
        public static bool BeforeInvoke(Controller controller)
        {
            //if (controller.ControllerName == "doc")
            //{
            //    controller.SetQuery("msg", "初始msg参数值。");
            //}

            return true;
        }
        /// <summary>
        /// 全局【方法执行后业务】
        /// 启用时：先执行局部（若存在），再执行全局。
        /// </summary>
        public static void EndInvoke(Controller controller)
        {
            //Console.WriteLine(Environment.NewLine + "---------Call..【"+ methodName + "】 RemoteIP : " + controller.Request.UserHostAddress + "-----------");
        }
    }
}
