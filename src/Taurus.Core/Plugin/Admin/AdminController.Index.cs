using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using System.Collections.Generic;
using Taurus.Mvc;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Admin
{

    /// <summary>
    /// 首页：微服务
    /// </summary>
    internal partial class AdminController
    {
        private MDictionary<string, List<HostInfo>> _HostList;
        private MDictionary<string, List<HostInfo>> HostList
        {
            get
            {
                if (_HostList == null)
                {
                    if (MsConfig.IsServer)
                    {
                        _HostList = Server.Gateway.HostList;
                    }
                    else if (MsConfig.IsClient)
                    {
                        _HostList = Client.Gateway.HostList;
                    }
                }
                return _HostList;
            }
        }

        private List<HostInfo> GetHostList(string name, bool withStar)
        {
            if (MsConfig.IsServer)
            {
                return Server.Gateway.GetHostList(name, withStar);
            }
            return Client.Gateway.GetHostList(name, withStar);
        }



        /// <summary>
        /// 微服务UI首页
        /// </summary>
        public void Index()
        {
            if (View != null)
            {
                View.KeyValue.Set("Version", MvcConst.Version);
                View.KeyValue.Set("MsType", GetMsTypeText());
                //基础信息：
                if (MsConfig.IsServer)
                {
                    View.KeyValue.Set("RcKey", MsConfig.Server.RcKey);
                    View.KeyValue.Set("RcPath", MsConfig.Server.RcPath);
                }
                if (HostList != null && HostList.Count > 0)
                {
                    BindNamesView();
                    BindDefaultView();
                }
            }
        }
        private void BindNamesView()
        {
            MDataTable dtServer = new MDataTable();
            dtServer.Columns.Add("Name,Count");
            MDataTable dtDomain = dtServer.Clone();
            MDataTable dtClient = dtServer.Clone();

            var hostList = HostList;//先获取引用【避免执行过程，因线程更换了引用的对象】
            foreach (var item in hostList)
            {
                if (item.Key == "RegCenter" || item.Key == "RegCenterOfSlave" || item.Key == "Gateway")
                {
                    dtServer.NewRow(true).Sets(0, item.Key, item.Value.Count);
                }
                else if (item.Key.Contains("."))
                {
                    dtDomain.NewRow(true).Sets(0, item.Key, item.Value.Count);
                }
                else
                {
                    dtClient.NewRow(true).Sets(0, item.Key, item.Value.Count);
                }

            }
            dtServer.Bind(View, "serverNamesView");
            dtDomain.Bind(View, "domainNamesView");
            dtClient.Bind(View, "clientNamesView");

        }
        private void BindDefaultView()
        {
            string host = Query<string>("h");
            string name = Query<string>("n", "RegCenter");
            if (!string.IsNullOrEmpty(host) || (name != "RegCenter" && name != "RegCenterOfSlave" && name != "Gateway"))
            {
                var hostList = HostList;
                if (hostList.ContainsKey("Gateway"))
                {
                    View.KeyValue.Set("GatewayUrl", hostList["Gateway"][0].Host);
                }
                else if (hostList.ContainsKey("RegCenter"))
                {
                    View.KeyValue.Set("GatewayUrl", hostList["RegCenter"][0].Host);
                }
            }
            if (!string.IsNullOrEmpty(host))
            {
                BindViewByHost(host);
            }
            else
            {
                BindViewByName(name);
            }

        }
        private void BindViewByName(string name)
        {
            List<HostInfo> list = GetHostList(name, false);
            if (list.Count > 0)
            {
                MDataTable dt = MDataTable.CreateFrom(list);
                if (dt != null)
                {
                    dt.Columns.Add("Name");
                    dt.Columns["Name"].Set(name);
                    if (MsConfig.IsServer && IsAdmin)
                    {
                        dt.Columns.Add("RemoteExit");
                        dt.Columns["RemoteExit"].Set("Stop");
                    }
                    View.OnForeach += View_OnForeach;
                    dt.Bind(View);
                }
            }
        }
        private void BindViewByHost(string host)
        {
            MDataTable dt = null;
            var hostList = HostList;
            foreach (var item in hostList)
            {
                foreach (var info in item.Value)
                {
                    if (info.Host == host)
                    {
                        if (dt == null)
                        {
                            dt = MDataTable.CreateFrom(info);
                            dt.Columns.Add("Name");
                            dt.Rows[0].Set("Name", item.Key);
                        }
                        else
                        {
                            dt.NewRow(true).Set("Name", item.Key).LoadFrom(info);
                        }
                        break;
                    }
                }
            }
            if (dt != null)
            {
                if (MsConfig.IsServer && IsAdmin)
                {
                    dt.Columns.Add("RemoteExit");
                    dt.Columns["RemoteExit"].Set("Stop");
                }
                View.OnForeach += View_OnForeach;
                dt.Bind(View);
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
            values["IsVirtual"] = values["IsVirtual"] == "True" ? "√" : "false";
            return text;
        }
    }

}
