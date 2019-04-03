using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;
using CYQ.Data;
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
            Write("Hello world");
        }
        /* */
        //本方法可以在其它文件里实现（partial类）
        public static bool CheckToken(IController controller, string methodName)
        {
            controller.CheckFormat("token Can't be Empty", "token");
            //实现Token验证
            //controller.Write(methodName + " NoToken");
            return true;
        }
        //[Regex("mn",true,"addfd"];
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
        public static void EndInvoke(IController controller, string methodName)
        {

        }
    }
}
