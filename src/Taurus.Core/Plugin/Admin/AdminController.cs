using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using System.Collections.Generic;
using Taurus.Mvc;
using CYQ.Data;
using System.IO;
using Taurus.MicroService;
using Taurus.Plugin.Limit;
using System.Threading;
using System.Diagnostics;

namespace Taurus.Plugin.Admin
{

    /// <summary>
    /// Taurus Admin Management Center。
    /// </summary>
    internal partial class AdminController : Controller
    {
        protected override string HtmlFolderName
        {
            get
            {
                return AdminConfig.HtmlFolderName;
            }
        }
        /// <summary>
        /// 账号检测是否登陆状态
        /// </summary>
        /// <returns></returns>
        public override bool BeforeInvoke()
        {
            switch (MethodName)
            {
                case "login":
                    return true;
                default:
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
    }

    /// <summary>
    /// 首页：
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
                if (MsConfig.IsRegCenterOfMaster)
                {
                    View.KeyValue.Set("MsType", "RegCenter of Master");
                }
                else if (MsConfig.IsRegCenter)
                {
                    View.KeyValue.Set("MsType", "RegCenter of Slave" + (Server.IsLiveOfMasterRC ? "" : " ( To Be Master Temporarily )"));
                }
                else if (MsConfig.IsGateway)
                {
                    View.KeyValue.Set("MsType", "Gateway");
                }
                else if (MsConfig.IsClient)
                {
                    View.KeyValue.Set("MsType", "Client of 【" + MsConfig.Client.Name + "】");
                }
                else
                {
                    View.KeyValue.Set("MsType", "None");
                }
                //基础信息：
                if (MsConfig.IsServer)
                {
                    View.KeyValue.Set("ClientKey", MsConfig.Client.Key);
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
    /// 登陆退出
    /// </summary>
    internal partial class AdminController
    {
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
            if (Query<string>("uid") == AdminConfig.UserName && Query<string>("pwd") == AdminConfig.Password)
            {
                Context.Session["login"] = "1";
                Response.Redirect("index");
                return;
            }
            View.Set("msg", "user or password is error.");
        }
    }
    /// <summary>
    /// 日志、配置信息
    /// </summary>
    internal partial class AdminController
    {
        /// <summary>
        /// 错误日志
        /// </summary>
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

        /// <summary>
        /// AppSetting 基础配置信息
        /// </summary>
        public void Config()
        {
            View.KeyValue.Set("Version", MvcConst.Version);

            MDataTable dtTaurus = new MDataTable();
            dtTaurus.Columns.Add("ConfigKey,ConfigValue,Description");
            dtTaurus.NewRow(true).Sets(0, "RunUrl", MvcConfig.RunUrl, "Web Run Url.");
            dtTaurus.NewRow(true).Sets(0, "DefaultUrl", MvcConfig.DefaultUrl, "Web Default Url.");
            dtTaurus.NewRow(true).Sets(0, "IsAllowCORS", MvcConfig.IsAllowCORS, "Is Allow CORS.");
            dtTaurus.NewRow(true).Sets(0, "RouteMode", MvcConfig.RouteMode, "Taurus Route Mode.");
            dtTaurus.NewRow(true).Sets(0, "Controllers", MvcConfig.Controllers, "Load Controller Names.");
            dtTaurus.NewRow(true).Sets(0, "Views", MvcConfig.Views, "Taurus Mvc View Folder Name.");
            dtTaurus.NewRow(true).Sets(0, "SslPath", MvcConfig.SslPath, "Ssl Folder Path For Https.");
            dtTaurus.NewRow(true).Sets(0, "SslCertificate - Count", MvcConfig.SslCertificate.Count, "Num Of Ssl(Https) File (ReadOnly).");
            if (MvcConfig.SslCertificate.Count > 0)
            {
                int i = 1;
                foreach (string name in MvcConfig.SslCertificate.Keys)
                {
                    dtTaurus.NewRow(true).Sets(0, "SslCertificate - " + i, name, "Ssl(Https) (ReadOnly).");
                    i++;
                }
            }
            dtTaurus.NewRow(true).Sets(0, "Suffix", MvcConfig.Suffix, "Deal With Suffix.");
            dtTaurus.NewRow(true).Sets(0, "SubAppName", MvcConfig.SubAppName, "Deploy As Sub App Name.");
            dtTaurus.Bind(View, "configTaurusView");

            MDataTable dtPlugin = new MDataTable();
            dtPlugin.Columns.Add("ConfigKey,ConfigValue,Description");

            //dtPlugin.NewRow(true).Sets(0, "Doc.IsEnable", DocConfig.IsEnable, "Doc Plugin Is Enable.");
            //dtPlugin.NewRow(true).Sets(0, "Limit.IsEnable", LimitConfig.IsEnable, "Limit Plugin Is Enable.");
            //dtPlugin.NewRow(true).Sets(0, "Admin.IsEnable", AdminConfig.IsEnable, "Admin Plugin Is Enable.");
            dtPlugin.NewRow(true).Sets(0, "Admin.Path", "/" + AdminConfig.Path, "Admin Url Path.");
            //dtPlugin.NewRow(true).Sets(0, "Admin.HtmlFolderName", AdminConfig.HtmlFolderName, "Admin Html Folder Name.");
            dtPlugin.NewRow(true).Sets(0, "Admin.UserName", AdminConfig.UserName, "Admin Account.");
            dtPlugin.NewRow(true).Sets(0, "Admin.Password", string.IsNullOrEmpty(AdminConfig.Password) ? "" : "******", "Admin Password.");


            dtPlugin.Bind(View, "configPluginView");

            if (MsConfig.IsServer)
            {
                MDataTable dtServer = new MDataTable();
                dtServer.Columns.Add("ConfigKey,ConfigValue,Description");
                dtServer.NewRow(true).Sets(0, "Name", MsConfig.Server.Name, "Service Name.");
                dtServer.NewRow(true).Sets(0, "Key", MsConfig.Server.Key, "Server Secret Key.");
                dtServer.NewRow(true).Sets(0, "RcUrl", MsConfig.Server.RcUrl, "Register Center Url.");
                dtServer.NewRow(true).Sets(0, "GatewayTimeout", MsConfig.Server.GatewayTimeout + "s", "Timeout For BigFile Upload.");
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
                dtClient.NewRow(true).Sets(0, "RemoteExit", MsConfig.Client.RemoteExit, "Allow Remote Stop Web Application By Register Center.");
                dtClient.Bind(View, "configClientView");
            }
            else
            {
                View.Remove("configClientNode");
            }
        }

        /// <summary>
        /// 操作系统环境信息
        /// </summary>
        public void OSInfo()
        {
            MDataTable dtTaurus = new MDataTable();
            dtTaurus.Columns.Add("ConfigKey,ConfigValue,Description");
            dtTaurus.NewRow(true).Sets(0, "Taurus-Version", MvcConst.Version, "Version of the Taurus.");
            dtTaurus.NewRow(true).Sets(0, "Net-Version", Environment.Version, "Version of the common language runtime.");
            dtTaurus.NewRow(true).Sets(0, "OS-Version", Environment.OSVersion, "Operating system.");
            dtTaurus.NewRow(true).Sets(0, "ProcessID", MvcConst.ProcessID, "Process id.");
            dtTaurus.NewRow(true).Sets(0, "ThreadID", Thread.CurrentThread.ManagedThreadId, "Identifier for the current managed thread.");
            dtTaurus.NewRow(true).Sets(0, "ThreadCount", Process.GetCurrentProcess().Threads.Count, "Number of threads for the current process.");
            dtTaurus.NewRow(true).Sets(0, "TickCount", (Environment.TickCount / 1000) + "s | " + (Environment.TickCount / 1000 / 60) + "m | " + (Environment.TickCount / 1000 / 3600) + "h | " + (Environment.TickCount / 1000 / 3600 / 24) + "d", "Time since the system started.");
            dtTaurus.NewRow(true).Sets(0, "ProcessorCount", Environment.ProcessorCount, "Number of processors on the current machine.");
            dtTaurus.NewRow(true).Sets(0, "MachineName", Environment.MachineName, "Name of this computer.");
            dtTaurus.NewRow(true).Sets(0, "UserName", Environment.UserName, "Name of the person who is logged on to Windows.");
            dtTaurus.NewRow(true).Sets(0, "WorkingSet", Environment.WorkingSet / 1024 + "KB | " + Environment.WorkingSet / 1024 / 1024 + "MB", "Physical memory mapped to the process context.");
            dtTaurus.NewRow(true).Sets(0, "CurrentDirectory", Environment.CurrentDirectory, "Fully qualified path of the current working directory.");
            dtTaurus.Bind(View);
        }
    }

    /// <summary>
    /// 设置
    /// </summary>
    internal partial class AdminController
    {
        public void Setting() { }


        public void SettingOfAccount()
        {
            View.KeyValue.Set("UserName", AdminConfig.UserName);
        }

        public void BtnSaveAccount()
        {
            string uid = Query<string>("uid");
            string pwd = Query<string>("pwd");
            string pwdAgain = Query<string>("pwdAgain");
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(pwd))
            {
                View.Set("msg", "UserName or passowrd can't be empty.");
            }
            else if (pwd != pwdAgain)
            {
                View.Set("msg", "Password must be same.");
            }
            else
            {
                View.Set("msg", "Save success.");
            }
        }

        public void SettingOfHostAdd()
        {
            if (!IsHttpPost)
            {
                if (MsConfig.IsRegCenterOfMaster)
                {
                    string hostList = IO.Read(AdminConst.HostAddPath);
                    View.KeyValue.Add("HostList", hostList);
                }
                else
                {
                    View.Set("btnAddHost", CYQ.Data.Xml.SetType.Disabled, "true");
                    View.Set("hostList", CYQ.Data.Xml.SetType.Disabled, "true");
                }
            }
        }
        /// <summary>
        /// 添加注册主机
        /// </summary>
        public void BtnAddHost()
        {
            if (!MsConfig.IsRegCenterOfMaster)
            {
                View.Set("msg", "Setting only for register center of master.");
                return;
            }

            string hostList = Query<string>("hostList");
            Server.RegCenter.AddHostByAdmin(hostList);
            IO.Write(AdminConst.HostAddPath, hostList);
            View.KeyValue.Add("HostList", hostList);
            View.Set("msg", "Save success.");
        }

        public void SettingOfIpBlackname()
        {
            if (!IsHttpPost)
            {
                string ipList = IO.Read(AdminConst.IPBlacknamePath);
                View.KeyValue.Add("IPList", ipList);
            }
        }
        /// <summary>
        /// 添加黑名单
        /// </summary>
        public void BtnAddIPBlackname()
        {
            string ipList = Query<string>("ipList");
            IPLimit.ResetIPList(ipList);
            LimitConfig.IsSyncIP = false;//手工保存后，重启服务前不再与注册同心保持同步。
            View.KeyValue.Add("IPList", ipList);
            View.Set("msg", "Save success.");
        }
    }
}
