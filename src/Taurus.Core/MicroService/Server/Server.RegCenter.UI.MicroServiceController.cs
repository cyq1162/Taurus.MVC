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
        public void Login()
        {

        }
        public void BtnLogin()
        {
            if (Query<string>("uid") == "admin" && Query<string>("pwd") == MsConfig.Server.RcPassword)
            {
                Context.Session["login"] = "1";
                Response.Redirect("index");
                return;
            }
            View.Set("msg", "user or password is error.");
        }
        public void Index()
        {
            if (!string.IsNullOrEmpty(MsConfig.Server.RcPassword) && Context.Session["login"] == null)
            {
                //检测账号密码，跳转登陆页
                Response.Redirect("login");
                return;
            }
            if (View != null && Server.Gateway.HostList != null && Server.Gateway.HostList.Count > 0)
            {
                View.KeyValue.Set("ClientKey", MsConfig.Client.Key);
                BindNamesView();
                BindDefaultView();
            }
        }
        public void BindNamesView()
        {
            MDataTable dt = new MDataTable();
            dt.Columns.Add("Name,Count");
            var hostList = Server.Gateway.HostList;//先获取引用【避免执行过程，因线程更换了引用的对象】
            foreach (var item in hostList)
            {
                dt.NewRow(true).Sets(0, item.Key, item.Value.Count);
            }
            dt.Bind(View, "namesView");

        }
        public void BindDefaultView()
        {
            string name = Query<string>("name", "RegCenter");
            if (name != "RegCenter" && name != "Gateway")
            {
                var hostList = Server.Gateway.HostList;
                if (hostList.ContainsKey("Gateway"))
                {
                    View.KeyValue.Set("GatewayUrl", hostList["Gateway"][0].Host);
                }
                else if (hostList.ContainsKey("RegCenter"))
                {
                    View.KeyValue.Set("GatewayUrl", hostList["RegCenter"][0].Host);
                }
            }
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
