using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using System.Collections.Generic;

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
            if (View != null && Server.Gateway.HostList != null && Server.Gateway.HostList.Count > 0)
            {
                View.KeyValue.Add("ClientKey", MsConfig.ClientKey);
                BindNamesView();
                BindDefaultView();
            }
        }
        public void BindNamesView()
        {
            MDataTable dt = new MDataTable();
            dt.Columns.Add("Name,Count");
            foreach (var item in Server.Gateway.HostList)
            {
                dt.NewRow(true).Sets(0, item.Key, item.Value.Count);
            }
            dt.Bind(View, "namesView");

        }
        public void BindDefaultView()
        {
            string name = Query<string>("name", "RegCenter");
            List<HostInfo> list = Server.Gateway.GetHostList(name);
            if (list.Count > 0)
            {
                MDataTable dt = MDataTable.CreateFrom(list);
                if (dt != null)
                {
                    dt.Columns.Add("Name,RemoteExitUrl");
                    dt.Columns["Name"].Set(name);
                    View.OnForeach += View_OnForeach;
                    dt.Bind(View);
                }
            }


        }
        private string View_OnForeach(string text, MDictionary<string, string> values, int rowIndex)
        {
            DateTime dt;
            if (DateTime.TryParse(values["RegTime"], out dt))
            {
                string time = dt.ToString("yyyy-MM-dd HH:mm:ss");
                values["RegTime"] = time;
            }
            return text;
        }
    }
}
