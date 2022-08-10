using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;
using CYQ.Data;
using System.Web;
namespace Taurus.Controllers
{
    /// <summary>
    /// 全局控制器（适全全局事件处理）
    /// </summary>
    public partial class DefaultController : Taurus.Core.Controller
    {
        /// <summary>
        /// 所有寻址不到的请求都集中执行到此方法。
        /// </summary>
        public override void Default()
        {
            Write("DefaultController : Hello world");
        }
        /// <summary>
        /// 用于所有的请求合法性验证，配合[Ack]属性
        /// 启用时：局部的先执行（若存在)，无局部才执行全局。
        /// </summary>
        public static bool CheckAck(IController controller, string methodName)
        {
            //需要自己实现Ack验证
            return !string.IsNullOrEmpty(controller.Query<string>("ack"));

        }

        /// <summary>
        /// 用于需要登陆后的身份验证，配合[Token]属性
        /// 启用时：局部的先执行（若存在)，无局部才执行全局。
        /// </summary>
        public static bool CheckToken(IController controller, string methodName)
        {
            //需要自己实现，或者通过配置Taurus.Auth启动自带的验证（自带的注释掉此方法即可）。
            return !string.IsNullOrEmpty(controller.Query<string>("token"));
        }

        /// <summary>
        /// 用于校验微服务的内部身份验证，配合[MicroService]属性
        /// 启用时：全局仅此一个生效，局部的失效。
        /// </summary>
        public static bool CheckMicroService(IController controller, string methodName)
        {
            return MicroService.Config.ServerKey == controller.Query<string>(MicroService.Const.HeaderKey);
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
        public static bool BeforeInvoke(IController controller, string methodName)
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
        public static void EndInvoke(IController controller, string methodName)
        {

        }
    }
}
