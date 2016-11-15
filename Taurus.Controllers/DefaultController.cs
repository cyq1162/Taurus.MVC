using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;

namespace Taurus.Controllers
{
    /// <summary>
    /// 默认控制器（此类不要动，partial类，可以在AriesController文件夹外建立自己的业务类）
    /// </summary>
    public partial class DefaultController:Controller
    {
        public override void Default()
        {
            
        }
        /*
        本方法可以在其它文件里实现（partial类）
        public static bool CheckToken(IController controller)
        {
            //实现Token验证
            controller.Context.Response.Write("NoToken");
            return false;
        }
         */
    }
}
