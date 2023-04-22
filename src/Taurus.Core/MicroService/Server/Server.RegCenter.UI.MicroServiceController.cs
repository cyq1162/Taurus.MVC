using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using System.Collections.Generic;
using Taurus.Mvc;
using CYQ.Data;
using System.IO;

namespace Taurus.MicroService
{
    /// <summary>
    /// 微服务 - 注册中心 界面。
    /// </summary>
    internal partial class MicroServiceController
    {
        /// <summary>
        /// 账号检测是否登陆状态
        /// </summary>
        /// <returns></returns>
        private bool UIAccountCheck(string methodName)
        {
            switch (methodName)
            {
                case "index":
                case "log":
                case "logdetail":
                case "config":
                    if (Context.Session["login"] == null)
                    {
                        //检测账号密码，跳转登陆页
                        Response.Redirect("login");
                        return false;
                    }
                    break;
            }
            return true;
        }

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
        public void Logout()
        {
            Context.Session["login"] = null;
            Response.Redirect("login");
        }
        public void Login()
        {
            if (Context.Session["login"] != null)
            {
                Response.Redirect("index");
            }
        }
        public void BtnLogin()
        {
            if (Query<string>("uid") == MsConfig.App.UserName && Query<string>("pwd") == MsConfig.App.Password)
            {
                Context.Session["login"] = "1";
                Response.Redirect("index");
                return;
            }
            View.Set("msg", "user or password is error.");
        }
        public void Index()
        {
            if (View != null && HostList != null && HostList.Count > 0)
            {
                if (MsConfig.IsServer)
                {
                    View.KeyValue.Set("ClientKey", MsConfig.Client.Key);
                }
                View.KeyValue.Set("Version", MvcConst.Version);
                View.KeyValue.Set("ProcessID", MvcConst.ProcessID.ToString());
                BindNamesView();
                BindDefaultView();
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
                    if (MsConfig.IsServer)
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
                if (MsConfig.IsServer)
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
            return text;
        }
    }

    /// <summary>
    /// 账号设置
    /// </summary>
    internal partial class MicroServiceController
    {
        public void Log()
        {
            string logPath = AppConfig.WebRootPath + AppConfig.Log.LogPath;
            if (Directory.Exists(logPath))
            {
                string[] files = Directory.GetFiles(logPath, "*.txt", SearchOption.TopDirectoryOnly);
                MDataTable dt = new MDataTable();
                dt.Columns.Add("FileName");
                foreach (string file in files)
                {
                    dt.NewRow(true).Set(0, Path.GetFileName(file));
                }
                dt.Rows.Sort("FileName desc");
                dt.Bind(View, "fileList");

            }
        }
        public void LogDetail()
        {
            string fileName = Query<string>("filename");
            if (!string.IsNullOrEmpty(fileName))
            {
                string logPath = AppConfig.WebRootPath + AppConfig.Log.LogPath;
                string[] files = Directory.GetFiles(logPath, fileName, SearchOption.TopDirectoryOnly);
                if (files != null && files.Length > 0)
                {
                    string logDetail = IOHelper.ReadAllText(files[0]);
                    View.KeyValue.Set("detail", logDetail.Replace("\n", "<br/>"));
                }
            }
        }


        public void Config()
        {
            View.KeyValue.Set("Version", MvcConst.Version);
            View.KeyValue.Set("ProcessID", MvcConst.ProcessID.ToString());
            MDataTable dtApp = new MDataTable();
            dtApp.Columns.Add("ConfigKey,ConfigValue,Description");
            dtApp.NewRow(true).Sets(0, "UserName", MsConfig.App.UserName, "Account.");
            dtApp.NewRow(true).Sets(0, "Password", MsConfig.App.Password, "Password.");
            dtApp.NewRow(true).Sets(0, "RunUrl", MsConfig.App.RunUrl, "Web Run Url.");
            dtApp.NewRow(true).Sets(0, "SslPath", MsConfig.App.SslPath, "Ssl File For Https.");
            dtApp.NewRow(true).Sets(0, "SslCertificate Count", MsConfig.App.SslCertificate.Count, "Num Of Ssl File (ReadOnly).");
            dtApp.NewRow(true).Sets(0, "RemoteExit", MsConfig.App.RemoteExit, "Allow Remote Stop Web Application.");
            dtApp.Bind(View, "configAppView");

            if (MsConfig.IsServer)
            {
                MDataTable dtServer = new MDataTable();
                dtServer.Columns.Add("ConfigKey,ConfigValue,Description");
                dtServer.NewRow(true).Sets(0, "Name", MsConfig.Server.Name, "Service Name.");
                dtServer.NewRow(true).Sets(0, "Key", MsConfig.Server.Key, "Server Secret Key.");
                dtServer.NewRow(true).Sets(0, "GatewayTimeout", MsConfig.Server.GatewayTimeout, "Timeout For BigFile Upload (s).");
                dtServer.Bind(View, "configServerView");
            }
            else
            {
                View.Remove("configServerNode");
            }

            if (MsConfig.IsClient)
            {
                MDataTable dtClient = new MDataTable();
                dtClient.Columns.Add("ConfigKey,ConfigValue,Description");
                dtClient.NewRow(true).Sets(0, "Key", MsConfig.Client.Key, "Client Secret Key.");
                dtClient.NewRow(true).Sets(0, "Name", MsConfig.Client.Name, "Client Name.");
                dtClient.NewRow(true).Sets(0, "Version", MsConfig.Client.Version, "Client Version.");
                dtClient.NewRow(true).Sets(0, "RcUrl", MsConfig.Client.RcUrl, "Register Center Url.");
                dtClient.Bind(View, "configClientView");
            }
            else
            {
                View.Remove("configClientNode");
            }
        }
    }
}
