using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;

namespace Taurus.MicroService
{
    /// <summary>
    /// 微服务 - 注册中心 界面。
    /// </summary>
    internal partial class MicroServiceController
    {
        //public void Json()
        //{
        //    Write(Server.HostListJson);
        //}
        public void Index()
        {
            if (View != null)
            {
                MDataTable dt = Server.GetHostTable();
                if (dt != null)
                {
                    View.OnForeach += View_OnForeach;
                    dt.Bind(View);
                }
            }
        }

        private string View_OnForeach(string text, MDictionary<string, string> values, int rowIndex)
        {
            DateTime dt;
            if (DateTime.TryParse(values["LastActiveTime"], out dt))
            {
                string time = dt.ToString("yyyy-MM-dd HH:mm:ss");
                values["LastActiveTime"] = time;
            }
            return text;
        }
    }
}
