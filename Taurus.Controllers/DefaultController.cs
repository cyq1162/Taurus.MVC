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
            Context.Response.Write("Hell Taurus.MVC! <a href=\"/home\">Go To Home</a>");
        }
    }
}
