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
            MDataTable dtClientDomain = dtServer.Clone();
            MDataTable dtClientModule = dtServer.Clone();
            MDataTable dtClientHost = dtServer.Clone();
            var hostList = HostList;//先获取引用【避免执行过程，因线程更换了引用的对象】
            var hostDic = new Dictionary<string, int>();
            foreach (var item in hostList)
            {
                if (item.Key == "RegCenter" || item.Key == "RegCenterOfSlave" || item.Key == "Gateway")
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
            string name = Query<string>("n", "RegCenter");
            if (type == 2 || (name != "RegCenter" && name != "RegCenterOfSlave" && name != "Gateway"))
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
            switch (type)
            {
                case 1:
                    BindViewByName(name); break;
                case 2:
                    BindViewByHost(name);
                    break;
                case 3:
                    BindViewByConnection(name); break;
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
                if (item.Key == "RegCenter" || item.Key == "RegCenterOfSlave" || item.Key == "Gateway" || item.Key.Contains("."))
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
                dt.Columns.Add("Name");
                dt.Columns["Name"].Set(name);
                if (state == 1 && MsConfig.IsServer && IsAdmin)
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
            values["State"] = state == "1" ? "√" : (state == "0" ? "not detected." : "detection failed.");
            return text;
        }
    }

}
