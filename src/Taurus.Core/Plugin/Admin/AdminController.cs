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
using Taurus.Plugin.Doc;
using System.Configuration;
using System.Net;

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

        private string GetMsType()
        {

            if (MsConfig.IsRegCenterOfMaster)
            {
                return "Register Center of Master";
            }
            else if (MsConfig.IsRegCenter)
            {
                return "Register Center of Slave" + (Server.IsLiveOfMasterRC ? "" : " ( Master connection refused )");
            }
            else if (MsConfig.IsGateway)
            {
                return "Gateway" + (Server.IsLiveOfMasterRC ? "" : " ( Register center connection refused )"); ;
            }
            else if (MsConfig.IsClient)
            {
                return "Client of MicroService" + (Client.IsLiveOfMasterRC ? "" : " ( Register center connection refused )");
            }
            else
            {
                return "None";
            }

        }
        /// <summary>
        /// 微服务UI首页
        /// </summary>
        public void Index()
        {
            if (View != null)
            {
                View.KeyValue.Set("Version", MvcConst.Version);
                View.KeyValue.Set("MsType", GetMsType());
                //基础信息：
                if (MsConfig.IsServer)
                {
                    View.KeyValue.Set("MsKey", MsConfig.Server.RcKey);
                    View.KeyValue.Set("Path", MsConfig.Server.RcPath);
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
            string uid = Query<string>("uid");
            string pwd = Query<string>("pwd");
            bool isOK = uid == AdminConfig.UserName && pwd == AdminConfig.Password;
            if (!isOK)
            {
                string[] items = IO.Read(AdminConst.AccountPath).Split(',');
                isOK = items.Length == 2 && items[0] == uid && items[1] == pwd;
            }
            if (isOK)
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
                    View.KeyValue.Set("detail", System.Web.HttpUtility.HtmlEncode(logDetail).Replace("\n", "<br/>"));
                }
            }
        }

        /// <summary>
        /// AppSetting 基础配置信息
        /// </summary>
        public void Config()
        {
            View.KeyValue.Set("Version", MvcConst.Version);
            string type = Query<string>("t", "mvc").ToLower();

            MDataTable dt = new MDataTable();
            dt.Columns.Add("ConfigKey,ConfigValue,Description");
            if (type == "mvc")
            {
                dt.NewRow(true).Sets(0, "Taurus.RunUrl", MvcConfig.RunUrl, "Application run url.");
                dt.NewRow(true).Sets(0, "Taurus.DefaultUrl", MvcConfig.DefaultUrl, "Application default url.");
                dt.NewRow(true).Sets(0, "Taurus.IsAllowCORS", MvcConfig.IsAllowCORS, "Application is allow cross-origin resource sharing.");
                dt.NewRow(true).Sets(0, "Taurus.RouteMode", MvcConfig.RouteMode, "Route mode for selected.");
                dt.NewRow(true).Sets(0, "Taurus.Controllers", MvcConfig.Controllers, "Load controller names.");
                dt.NewRow(true).Sets(0, "Taurus.Views", MvcConfig.Views, "Mvc view folder name.");
                dt.NewRow(true).Sets(0, "Taurus.SslPath", MvcConfig.SslPath, "Ssl path for https.");
                dt.NewRow(true).Sets(0, "----------SslCertificate - Count", MvcConfig.SslCertificate.Count, "Num of ssl for https (Show Only).");
                if (MvcConfig.SslCertificate.Count > 0)
                {
                    int i = 1;
                    foreach (string name in MvcConfig.SslCertificate.Keys)
                    {
                        dt.NewRow(true).Sets(0, "----------SslCertificate - " + i, name, "Domain ssl for https (Show Only).");
                        i++;
                    }
                }
                dt.NewRow(true).Sets(0, "Taurus.Suffix", MvcConfig.Suffix, "Deal with mvc suffix.");
                dt.NewRow(true).Sets(0, "Taurus.SubAppName", MvcConfig.SubAppName, "Name of deploy as sub application.");

            }
            else if (type == "plugin")
            {
                dt.NewRow(true).Sets(0, "Admin.IsEnable", AdminConfig.IsEnable, "Admin plugin is enable.");
                dt.NewRow(true).Sets(0, "Admin.Path", "/" + AdminConfig.Path, "Admin url path.");
                dt.NewRow(true).Sets(0, "Admin.HtmlFolderName", AdminConfig.HtmlFolderName, "Mvc view folder name for admin.");
                dt.NewRow(true);
                dt.NewRow(true).Sets(0, "Admin.UserName", AdminConfig.UserName, "Admin account.");
                dt.NewRow(true).Sets(0, "Admin.Password", string.IsNullOrEmpty(AdminConfig.Password) ? "" : "******", "Admin password.");

                string[] items = IO.Read(AdminConst.AccountPath).Split(',');
                if (items.Length == 2)
                {
                    dt.NewRow(true).Sets(0, "Admin.UserName - Setting ", items[0], "Admin account by setting.");
                    dt.NewRow(true).Sets(0, "Admin.Password - Setting", "******", "Admin password by setting.");
                }

                dt.NewRow(true);
                dt.NewRow(true).Sets(0, "Doc.IsEnable", DocConfig.IsEnable, "Doc plugin is enable.");
                dt.NewRow(true).Sets(0, "Doc.Path", "/" + DocConfig.Path, "Doc url path.");
                dt.NewRow(true).Sets(0, "Doc.HtmlFolderName", DocConfig.HtmlFolderName, "Mvc view folder name for doc.");
                dt.NewRow(true).Sets(0, "Doc.DefaultImg", DocConfig.DefaultImg, "Default images path for doc auto test,as :/App_Data/xxx.jpg");
                dt.NewRow(true).Sets(0, "Doc.DefaultParas", DocConfig.DefaultParas, "global para for doc auto test,as :ack,token");

                dt.NewRow(true);
                dt.NewRow(true).Sets(0, "Limit.IP.IsEnable", LimitConfig.IP.IsEnable, "IP limit plugin is enable.");
                dt.NewRow(true).Sets(0, "Limit.IP.IsSync", LimitConfig.IP.IsSync, "IP limit : is sync ip blackname list from register center.");

                dt.NewRow(true);
                dt.NewRow(true).Sets(0, "Limit.Ack.IsEnable", LimitConfig.Ack.IsEnable, "Ack limit plugin is enable.");
                dt.NewRow(true).Sets(0, "Limit.Ack.Key", LimitConfig.Ack.Key, "Ack limit : secret key.");
                dt.NewRow(true).Sets(0, "Limit.Ack.IsVerifyDecode", LimitConfig.Ack.IsVerifyDecode, "Ack limit : ack must be decode and valid.");
                dt.NewRow(true).Sets(0, "Limit.Ack.IsVerifyUsed", LimitConfig.Ack.IsVerifyUsed, "Ack limit : ack use once only.");


            }
            else if (type == "microservice")
            {
                dt.NewRow(true).Sets(0, "MicroServer Type", GetMsType(), "Type of current microservice (Show Only).");
                if (MsConfig.IsServer)
                {
                    dt.NewRow(true).Sets(0, "MicroServer.Server.Name", MsConfig.Server.Name, "Server name.");
                    dt.NewRow(true).Sets(0, "MicroServer.Server.RcKey", MsConfig.Server.RcKey, "Register center secret key.");
                    dt.NewRow(true).Sets(0, "MicroServer.Server.RcUrl", MsConfig.Server.RcUrl, "Register center url.");
                    dt.NewRow(true).Sets(0, "MicroServer.Server.RcPath", "/" + MsConfig.Server.RcPath, "Register center local path.");
                    dt.NewRow(true).Sets(0, "MicroServer.Server.GatewayTimeout", MsConfig.Server.GatewayTimeout + "s", "Gateway timeout for big file upload.");
                    dt.NewRow(true).Sets(0, "MicroServer Gateway Proxy LastTime", Rpc.Gateway.LastProxyTime.ToString("yyyy-MM-dd HH:mm:ss"), "The last time the proxy forwarded the request (Show Only).");
                }

                if (MsConfig.IsClient)
                {
                    if (MsConfig.IsServer)
                    {
                        dt.NewRow(true);
                    }
                    dt.NewRow(true).Sets(0, "MicroServer.Client.Name", MsConfig.Client.Name, "Client module name.");
                    dt.NewRow(true).Sets(0, "MicroServer.Client.Domain", MsConfig.Client.Domain, "Client bind domain.");
                    dt.NewRow(true).Sets(0, "MicroServer.Client.Version", MsConfig.Client.Version, "Client web version.");
                    dt.NewRow(true).Sets(0, "MicroServer.Client.RemoteExit", MsConfig.Client.RemoteExit, "Client is allow remote stop by register center.");
                    dt.NewRow(true).Sets(0, "MicroServer.Client.RcKey", MsConfig.Client.RcKey, "Register center secret key.");
                    dt.NewRow(true).Sets(0, "MicroServer.Client.RcUrl", MsConfig.Client.RcUrl, "Register center url.");
                    dt.NewRow(true).Sets(0, "MicroServer.Client.RcPath", "/" + MsConfig.Client.RcPath, "Register center local path.");
                    
                }
            }
            else if (type == "cyq.data")
            {
                dt.NewRow(true).Sets(0, "IsWriteLog", AppConfig.Log.IsWriteLog, "Write log to file or database on error,otherwise throw exception.");
                dt.NewRow(true).Sets(0, "LogPath", AppConfig.Log.LogPath, "Log folder name or path.");

                //dt.NewRow(true).Sets(0, "LogConn", string.IsNullOrEmpty(AppConfig.Log.LogConn) ? "" : "******", "Log database connection string.");
                //dt.NewRow(true).Sets(0, "LogTableName", AppConfig.Log.LogTableName, "Log tablename on log database.");
                dt.NewRow(true);

                dt.NewRow(true).Sets(0, "SchemaMapPath", AppConfig.DB.SchemaMapPath, "Database metadata cache path.");
                dt.NewRow(true);
                dt.NewRow(true).Sets(0, "OpenDebugInfo", AppConfig.Debug.OpenDebugInfo, "Record sql on dev debug.");
                dt.NewRow(true);
                dt.NewRow(true).Sets(0, "SqlFilter", AppConfig.Debug.SqlFilter + "ms", "Write sql to log file when sql exe time > value(value must>0).");

                dt.NewRow(true);
                dt.NewRow(true).Sets(0, "IsAutoCache", AppConfig.Cache.IsAutoCache, "Use auto cache.");
                dt.NewRow(true).Sets(0, "DefaultCacheTime", AppConfig.Cache.DefaultCacheTime + "m", "Default cache time (minute).");

                dt.NewRow(true);
                dt.NewRow(true).Sets(0, "RedisServers", string.IsNullOrEmpty(AppConfig.Cache.RedisServers) ? "" : "******", "Redis servers.");
                dt.NewRow(true).Sets(0, "RedisUseDBCount", AppConfig.Cache.RedisUseDBCount, "Redis use db count.");
                dt.NewRow(true).Sets(0, "RedisUseDBIndex", AppConfig.Cache.RedisUseDBIndex, "Redis use db index.");

                dt.NewRow(true);
                foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                {
                    dt.NewRow(true).Sets(0, item.Name, string.IsNullOrEmpty(item.ConnectionString) ? "" : "******", "Database connection string.");
                }
            }
            dt.Bind(View);
        }

        /// <summary>
        /// 操作系统环境信息
        /// </summary>
        public void OSInfo()
        {
            MDataTable dtTaurus = new MDataTable();
            dtTaurus.Columns.Add("ConfigKey,ConfigValue,Description");
            dtTaurus.NewRow(true).Sets(0, "Client-IP", Request.UserHostAddress, "Client ip.");
            dtTaurus.NewRow(true).Sets(0, "Server-IP", MvcConst.HostIP, "Server ip.");
            dtTaurus.NewRow(true).Sets(0, "Taurus-Version", "V" + MvcConst.Version, "Version of the Taurus.Core.dll.");
            dtTaurus.NewRow(true).Sets(0, "Orm-Version", "V" + AppConfig.Version, "Version of the CYQ.Data.dll.");
            dtTaurus.NewRow(true).Sets(0, "AppPath", AppConfig.RunPath, "Web application path of the working directory.");
            dtTaurus.NewRow(true);
            dtTaurus.NewRow(true).Sets(0, "Runtime-Version", (AppConfig.IsNetCore ? ".Net Core - " : ".Net Framework - ") + Environment.Version, "Version of the common language runtime.");
            dtTaurus.NewRow(true).Sets(0, "OS-Version", Environment.OSVersion, "Operating system.");
            dtTaurus.NewRow(true).Sets(0, "ProcessID", MvcConst.ProcessID, "Process id.");
            dtTaurus.NewRow(true).Sets(0, "ThreadID", Thread.CurrentThread.ManagedThreadId, "Identifier for the managed thread.");
            dtTaurus.NewRow(true).Sets(0, "ThreadCount", Process.GetCurrentProcess().Threads.Count, "Number of threads for the process.");
            dtTaurus.NewRow(true).Sets(0, "TickCount", (Environment.TickCount / 1000) + "s | " + (Environment.TickCount / 1000 / 60) + "m | " + (Environment.TickCount / 1000 / 3600) + "h | " + (Environment.TickCount / 1000 / 3600 / 24) + "d", "Time since the system started.");
            dtTaurus.NewRow(true).Sets(0, "ProcessorCount", Environment.ProcessorCount, "Number of processors on the machine.");
            dtTaurus.NewRow(true).Sets(0, "MachineName", Environment.MachineName, "Name of computer.");
            dtTaurus.NewRow(true).Sets(0, "UserName", Environment.UserName, "Name of the person who is logged on to Windows.");
            dtTaurus.NewRow(true).Sets(0, "WorkingSet", Environment.WorkingSet / 1024 + "KB | " + Environment.WorkingSet / 1024 / 1024 + "MB", "Physical memory mapped to the process context.");
            dtTaurus.NewRow(true).Sets(0, "CurrentDirectory", Environment.CurrentDirectory, "Fully qualified path of the working directory.");

            dtTaurus.Bind(View);
        }
    }

    /// <summary>
    /// 设置：账号、IP黑名单、手工添加微服务客户端
    /// </summary>
    internal partial class AdminController
    {
        public void Setting() { }


        public void SettingOfAccount()
        {
            if (!IsHttpPost)
            {
                View.KeyValue.Set("UserName", AdminConfig.UserName);
            }
        }

        public void BtnSaveAccount()
        {
            string uid = Query<string>("uid");
            string pwd = Query<string>("pwd");
            string pwdAgain = Query<string>("pwdAgain");
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(pwd))
            {
                View.Set("msg", "User or passowrd can't be empty.");
            }
            else if (pwd != pwdAgain)
            {
                View.Set("msg", "Password must be same.");
            }
            if (uid.Contains(",") || pwd.Contains(","))
            {
                View.Set("msg", "User or password can't contain ','.");
            }
            else
            {
                IO.Write(AdminConst.AccountPath, uid + "," + pwd);
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
            LimitConfig.IP.IsSync = false;//手工保存后，重启服务前不再与注册同心保持同步。
            View.KeyValue.Add("IPList", ipList);
            View.Set("msg", "Save success.");
        }
    }
}
