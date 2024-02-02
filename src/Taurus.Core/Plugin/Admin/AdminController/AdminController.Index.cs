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
                        _HostList = Gateway.Server.HostList;
                    }
                    else if (MsConfig.IsClient)
                    {
                        _HostList = Gateway.Client.HostList;
                    }
                }
                return _HostList;
            }
        }

        private List<HostInfo> GetHostList(string name, bool withStar)
        {
            if (MsConfig.IsServer)
            {
                return Gateway.Server.GetHostList(name, withStar);
            }
            return Gateway.Client.GetHostList(name, withStar);
        }



        /// <summary>
        /// 微服务UI首页
        /// </summary>
        public void Index()
        {
            if (IsGotoAdmin())
            {
                return;
            }
            if (View != null)
            {
                View.KeyValue.Set("Version", MvcConst.Version);
                View.KeyValue.Set("MsType", GetMsTypeText());
                View.KeyValue.Set("Target", (Query<int>("t") == 3 && MsConfig.IsServer) ? "_blank" : "_self");

                if (HostList != null && HostList.Count > 0)
                {
                    BindNamesView();
                    BindDefaultView();
                }
            }
        }

        private bool IsGotoAdmin()
        {
            if (!MsConfig.IsServer || !IsAdmin)
            {
                return false;
            }
            int to = Query<int>("to");
            if (to == 1 || to == 2)
            {
                //绑定域名 进行跳转
                string hostIP = Query<string>("hostIP");
                string host = Query<string>("host");
                if (!string.IsNullOrEmpty(hostIP) && !string.IsNullOrEmpty(host))
                {
                    string url = string.Empty;
                    string[] items = host.Split(':');
                    if (items.Length == 2)
                    {
                        url = host;
                    }
                    else if (items.Length == 3)
                    {
                        url = items[0] + "://" + hostIP + ":" + items[2];
                    }
                    if (!string.IsNullOrEmpty(url))
                    {
                        switch (to)
                        {
                            case 1://to doc
                                string gatewayUrl = string.Empty;
                                var hostList = HostList;
                                if (hostList.ContainsKey(MsConst.Gateway))
                                {
                                    gatewayUrl = hostList[MsConst.Gateway][0].Host;
                                }
                                else if (hostList.ContainsKey(MsConst.RegistryCenter))
                                {
                                    gatewayUrl = hostList[MsConst.RegistryCenter][0].Host;
                                }
                                url = url + "/doc?g=" + gatewayUrl;
                                break;
                            case 2://to admin
                                url = url + AdminConfig.Path + "/login";
                                break;
                        }
                        Response.Redirect(url);
                        return true;
                    }
                }
            }
            return false;
        }

        private void BindNamesView()
        {
            MDataTable dtServer = new MDataTable();
            dtServer.Columns.Add("Name,Count");
            MDataTable dtClientDomain = dtServer.Clone();
            MDataTable dtClientModule = dtServer.Clone();
            MDataTable dtClientHost = dtServer.Clone();
            var hostList = HostList;//先获取引用【避免执行过程，因线程更换了引用的对象】
            var hostDic = new Dictionary<string, int>();
            foreach (var item in hostList)
            {
                string lowerKey = item.Key.ToLower();
                if (lowerKey == MsConst.RegistryCenter || lowerKey == MsConst.RegistryCenterOfSlave || lowerKey == MsConst.Gateway)
                {
                    dtServer.NewRow(true).Sets(0, item.Key, item.Value.Count);
                }
                else if (item.Key.Contains("."))
                {
                    dtClientDomain.NewRow(true).Sets(0, item.Key, item.Value.Count);
                }
                else
                {
                    foreach (var info in item.Value)
                    {
                        if (!hostDic.ContainsKey(info.Host))
                        {
                            hostDic.Add(info.Host, info.State);
                        }
                        else
                        {
                            if (info.State != 0)
                            {
                                hostDic[info.Host] = info.State;
                            }
                        }
                    }
                    dtClientModule.NewRow(true).Sets(0, item.Key, item.Value.Count);
                }
            }
            #region 处理主机绑定
            if (hostDic.Count > 0)
            {
                int ok = 0, fail = 0;
                foreach (var item in hostDic)
                {
                    switch (item.Value)
                    {
                        case -1:
                            fail++;
                            break;
                        case 1:
                            ok++;
                            break;
                    }
                }
                dtClientHost.NewRow(true).Sets(0, "Connected", ok);
                dtClientHost.NewRow(true).Sets(0, "Connection-Failed", fail);
                dtClientHost.Bind(View, "clientHostView");
            }

            #endregion
            dtServer.Bind(View, "serverNamesView");
            dtClientDomain.Bind(View, "clientDomainView");
            dtClientModule.Bind(View, "clientModuleView");

        }
        private void BindDefaultView()
        {
            int type = Query<int>("t", 1);
            string name = Query<string>("n", "RegistryCenter");
            switch (type)
            {
                case 2:
                    BindViewByConnection(name); break;
                case 5:
                    BindViewByHost(name); break;
                default:
                    BindViewByName(name); break;
            }


        }
        private void BindViewByName(string name)
        {
            List<HostInfo> list = GetHostList(name, false);
            if (list.Count > 0)
            {
                MDataTable dt = MDataTable.CreateFrom(list, BreakOp.None);
                if (dt != null)
                {
                    dt.Rows.Sort("Host");
                    dt.Columns.Add("Name");
                    dt.Columns["Name"].Set(name);
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
                View.OnForeach += View_OnForeach;
                dt.Bind(View);
            }

        }

        private void BindViewByConnection(string name)
        {
            int state = 0;
            switch (name)
            {
                case "Connection-Failed":
                    state = -1; break;
                default:
                    state = 1; break;
            }
            var hostList = HostList;//先获取引用【避免执行过程，因线程更换了引用的对象】
            var hostDic = new Dictionary<string, int>();
            List<HostInfo> list = new List<HostInfo>();
            foreach (var item in hostList)
            {
                string lowerKey = item.Key.ToLower();
                if (lowerKey == MsConst.RegistryCenter || lowerKey == MsConst.RegistryCenterOfSlave || lowerKey == MsConst.Gateway || lowerKey.Contains("."))
                {
                    continue;
                }
                foreach (var info in item.Value)
                {
                    if (info.State == state)
                    {
                        if (!hostDic.ContainsKey(info.Host))
                        {
                            hostDic.Add(info.Host, info.State);
                            list.Add(info);
                        }
                    }
                }
            }
            MDataTable dt = MDataTable.CreateFrom(list, BreakOp.None);
            if (dt != null)
            {
                dt.Rows.Sort("Host");
                dt.Columns.Add("Name");
                dt.Columns["Name"].Set(name);
                if (state == 1 && MsConfig.IsRegistryCenterOfMaster && IsAdmin)
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
            string state = values["State"];
            values["State"] = state == "1" ? "√" : (state == "0" ? "- - -" : "detection failed.");
            return text;
        }
    }

}
