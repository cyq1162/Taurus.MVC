using System;
using System.Text;
using CYQ.Data.Tool;
using Taurus.Mvc;
using System.Collections.Generic;
using Taurus.Mvc.Attr;
using CYQ.Data.Table;

namespace Taurus.MicroService
{
    /// <summary>
    /// 微服务 - 注册中心 界面。
    /// </summary>
    internal partial class MicroServiceController
    {
        public void Json()
        {
            Write(Server.HostListJson);
        }
        public void Index()
        {
            if (View != null)
            {
                MDataTable dt = Server.GetHostTable();
                if(dt != null)
                {
                    dt.Bind(View);
                }
            }
        }
    }
}
