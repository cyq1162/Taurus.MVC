using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using Taurus.Plugin.Doc;
using System.Configuration;


namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 应用配置信息
    /// </summary>
    internal partial class AdminController
    {
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
                #region Mvc
                Sets(dt, "Taurus.IsEnable", MvcConfig.IsEnable, "Taurus mvc  is enable.");
                Sets(dt, "Taurus.RunUrl", MvcConfig.RunUrl, "Application run url.");
                Sets(dt, "Taurus.DefaultUrl", MvcConfig.DefaultUrl, "Application default url.");
                Sets(dt, "Taurus.IsAllowCORS", MvcConfig.IsAllowCORS, "Application is allow cross-origin resource sharing.");

                Sets(dt, "Taurus.RouteMode", GetRouteModeText(), "Route mode for selected.");
                Sets(dt, "Taurus.Controllers", MvcConfig.Controllers, "Load controller names.");
                Sets(dt, "Taurus.Views", MvcConfig.Views, "Mvc view folder name.");
                Sets(dt, "Taurus.SslPath", MvcConfig.SslPath, "Ssl path for https (*.pfx for ssl , *.txt for pwd).");
                Sets(dt, "----------SslCertificate - Count", MvcConfig.SslCertificate.Count, "Num of ssl for https (Show Only).");
                if (MvcConfig.SslCertificate.Count > 0)
                {
                    int i = 1;
                    foreach (string name in MvcConfig.SslCertificate.Keys)
                    {
                        Sets(dt, "----------SslCertificate - " + i, name, "Domain ssl for https (Show Only).");
                        i++;
                    }
                }
                Sets(dt, "Taurus.Suffix", MvcConfig.Suffix, "Deal with mvc suffix.");
                Sets(dt, "Taurus.SubAppName", MvcConfig.SubAppName, "Name of deploy as sub application.");
                #endregion
            }
            else if (type.StartsWith("plugin"))
            {
                if (type.StartsWith("plugin-limit"))
                {
                    if (type == "plugin-limit")
                    {
                        Sets(dt, "Limit.IsIgnoreLAN", LimitConfig.IsIgnoreLAN, "limit : is ignore LAN (Local Area Network) IP address.");
                        Sets(dt, "Limit.IsIgnoreAdmin", LimitConfig.IsIgnoreAdmin, "limit : is ignore /admin path.");
                        Sets(dt, "Limit.IsIgnoreDoc", LimitConfig.IsIgnoreDoc, "limit : is ignore /doc path.");
                        Sets(dt, "Limit.IsIgnoreMicroService", LimitConfig.IsIgnoreMicroService, "limit : is ignore /microservice path.");
                        Sets(dt, "Limit.IsUseXRealIP", LimitConfig.IsUseXRealIP, "limit : is use X-Real-IP to obtain the client IP address.");
                        dt.NewRow(true);
                        Sets(dt, "Limit.IP.IsEnable", LimitConfig.IP.IsEnable, "IP limit : IP blackname plugin.");
                        Sets(dt, "Limit.Rate.IsEnable", LimitConfig.Rate.IsEnable, "Rate limit : API request rate limiting plugin.");
                        Sets(dt, "Limit.Ack.IsEnable", LimitConfig.Ack.IsEnable, "Ack limit : ACK security code verification plugin.");
                    }
                    else if (type == "plugin-limit-ip")
                    {
                        Sets(dt, "Limit.IP.IsEnable", LimitConfig.IP.IsEnable, "IP limit : IP blackname plugin.");
                        Sets(dt, "Limit.IP.IsSync", LimitConfig.IP.IsSync, "IP limit : is sync ip blackname list from register center.");
                    }
                    else if (type == "plugin-limit-rate")
                    {
                        Sets(dt, "Limit.Rate.IsEnable", LimitConfig.Rate.IsEnable, "Rate limit : API request rate limiting plugin.");
                        Sets(dt, "Limit.Rate.Period", LimitConfig.Rate.Period + " (s)", "Rate limit : interval period (second).");
                        Sets(dt, "Limit.Rate.Limit", LimitConfig.Rate.Limit, "Rate limit : maximum number of requests within an interval time.");
                        Sets(dt, "Limit.Rate.Key", LimitConfig.Rate.Key, "Rate limit : can customize a key to replace IP.");

                    }
                    else if (type == "plugin-limit-ack")
                    {
                        Sets(dt, "Limit.Ack.IsEnable", LimitConfig.Ack.IsEnable, "Ack limit : ACK security code verification plugin.");
                        Sets(dt, "Limit.Ack.Key", LimitConfig.Ack.Key, "Ack limit : secret key.");
                        Sets(dt, "Limit.Ack.IsVerifyDecode", LimitConfig.Ack.IsVerifyDecode, "Ack limit : ack must be decode and valid.");
                        Sets(dt, "Limit.Ack.IsVerifyUsed", LimitConfig.Ack.IsVerifyUsed, "Ack limit : ack use once only.");
                    }
                }
                else if (type == "plugin-admin")
                {
                    Sets(dt, "Admin.IsEnable", AdminConfig.IsEnable, "Admin plugin : backend visual management plugin.");
                    Sets(dt, "Admin.Path", AdminConfig.Path, "Admin url path.");
                    Sets(dt, "Admin.HtmlFolderName", AdminConfig.HtmlFolderName, "Mvc view folder name for admin.");
                    dt.NewRow(true);
                    Sets(dt, "Admin.UserName", AdminConfig.UserName + GetOnlineText(true), "Admin account.");
                    Sets(dt, "Admin.Password", string.IsNullOrEmpty(AdminConfig.Password) ? "" : "******", "Admin password.");

                    string[] items = IO.Read(AdminConst.AccountPath).Split(',');
                    if (items.Length == 2)
                    {
                        dt.NewRow(true);
                        Sets(dt, "Admin.UserName - Setting ", items[0] + GetOnlineText(false), "Readonly account by setting.");
                        Sets(dt, "Admin.Password - Setting", items[1], "Readonly password by setting.");
                    }
                }
                else if (type == "plugin-doc")
                {
                    Sets(dt, "Doc.IsEnable", DocConfig.IsEnable, "Doc plugin : API automation testing plugin.");
                    Sets(dt, "Doc.Path", DocConfig.Path, "Doc url path.");
                    Sets(dt, "Doc.HtmlFolderName", DocConfig.HtmlFolderName, "Mvc view folder name for doc.");
                    Sets(dt, "Doc.DefaultImg", DocConfig.DefaultImg, "Default images path for doc auto test,as :/App_Data/xxx.jpg");
                    Sets(dt, "Doc.DefaultParas", DocConfig.DefaultParas, "global para for doc auto test,as :ack,token");
                }
                else if (type == "plugin-microservice")
                {
                    #region MicroService

                    Sets(dt, "MicroService Type", GetMsTypeText(), "Type of current microservice (Show Only).");
                    if (MsConfig.IsServer)
                    {
                        Sets(dt, "MicroService.Server.IsEnable", MsConfig.Server.IsEnable, "Microservice server (register center, gateway) plugin.");
                        Sets(dt, "MicroService.Server.Name", MsConfig.Server.Name, "Server name.");
                        Sets(dt, "MicroService.Server.RcKey", MsConfig.Server.RcKey, "Register center secret key.");
                        Sets(dt, "MicroService.Server.RcUrl", MsConfig.Server.RcUrl, "Register center url.");
                        Sets(dt, "MicroService.Server.RcUrl - 2", Server.Host2, "Register center backup url.");
                        Sets(dt, "MicroService.Server.RcPath", MsConfig.Server.RcPath, "Register center local path.");
                        Sets(dt, "MicroService.Server.GatewayTimeout", MsConfig.Server.GatewayTimeout + " (s)", "Gateway timeout (second) for request forward.");
                        Sets(dt, "MicroService Gateway Proxy LastTime", Rpc.Gateway.LastProxyTime.ToString("yyyy-MM-dd HH:mm:ss"), "The last time the proxy forwarded the request (Show Only).");
                    }

                    if (MsConfig.IsClient)
                    {
                        if (MsConfig.IsServer)
                        {
                            dt.NewRow(true);
                        }
                        Sets(dt, "MicroService.Client.IsEnable", MsConfig.Client.IsEnable, "Microservice client plugin.");
                        Sets(dt, "MicroService.Client.Name", MsConfig.Client.Name, "Client module name.");
                        Sets(dt, "MicroService.Client.Domain", MsConfig.Client.Domain, "Client bind domain.");
                        Sets(dt, "MicroService.Client.Version", MsConfig.Client.Version, "Client web version.");
                        Sets(dt, "MicroService.Client.RemoteExit", MsConfig.Client.RemoteExit, "Client is allow remote stop by register center.");
                        Sets(dt, "MicroService.Client.RcKey", MsConfig.Client.RcKey, "Register center secret key.");
                        Sets(dt, "MicroService.Client.RcUrl", MsConfig.Client.RcUrl, "Register center url.");
                        Sets(dt, "MicroService.Client.RcUrl - 2", Client.Host2, "Register center backup url.");
                        Sets(dt, "MicroService.Client.RcPath", MsConfig.Client.RcPath, "Register center local path.");

                    }
                    #endregion
                }
            }
            else if (type.StartsWith("cyq.data"))
            {
                if (type == "cyq.data")
                {
                    Sets(dt, "AutoCache.IsEnable", AppConfig.AutoCache.IsEnable, "Use auto cache.");
                    Sets(dt, "Debug.IsEnable", AppConfig.Debug.IsEnable, "Record sql when dev debug.");
                    dt.NewRow(true);
                    Sets(dt, "Log.IsEnable", AppConfig.Log.IsEnable, "Write log to file or database on error,otherwise throw exception.");
                    Sets(dt, "Log.Path", AppConfig.Log.Path, "Log folder name or path.");
                    dt.NewRow(true);
                    Sets(dt, "DB.SchemaMapPath", AppConfig.DB.SchemaMapPath, "Database metadata cache path.");
                    Sets(dt, "DB.CommandTimeout", AppConfig.DB.CommandTimeout + " (s)", "Timeout for database command.");
                    Sets(dt, "DB.SqlFilter", AppConfig.DB.SqlFilter + " (ms)", "Write sql to log file when sql exe time > value(value must>0).");
                    dt.NewRow(true);
                    Sets(dt, "Aop", AppConfig.Aop, "Aop config :【Aop-Class-FullName,DllName】");
                    Sets(dt, "EncryptKey", AppConfig.EncryptKey, "Encrypt key for EncryptHelper tool.");
                    Sets(dt, "DefaultCacheTime", AppConfig.DefaultCacheTime + " (m)", "Default cache time (minute).");
                }
                else if (type == "cyq.data-log")
                {
                    Sets(dt, "Log.IsEnable", AppConfig.Log.IsEnable, "Write log to file or database on error,otherwise throw exception.");
                    Sets(dt, "Log.Path", AppConfig.Log.Path, "Log folder name or path.");
                    Sets(dt, "Log.TableName", AppConfig.Log.TableName, "Log tablename on log database.");
                    Sets(dt, "LogConn", HideConnPassword(AppConfig.Log.Conn), "Log database connection string.");
                }
                else if (type == "cyq.data-conn")
                {
                    foreach (ConnectionStringSettings item in ConfigurationManager.ConnectionStrings)
                    {
                        Sets(dt, item.Name, HideConnPassword(item.ConnectionString), "DataBaseType : " + DBTool.GetDataBaseType(item.ConnectionString));
                    }
                }
                else if (type == "cyq.data-autocache")
                {
                    Sets(dt, "AutoCache.IsEnable", AppConfig.AutoCache.IsEnable, "AutoCache is enabled.");
                    Sets(dt, "AutoCache.Tables", AppConfig.AutoCache.Tables, "Set the tables that need to be cached by specifying their names separated by commas.");
                    Sets(dt, "AutoCache.IngoreTables", AppConfig.AutoCache.IngoreTables, "Set the tables that no need to be cached by specifying their names separated by commas.");
                    Sets(dt, "AutoCache.IngoreColumns", AppConfig.AutoCache.IngoreColumns, "Set column names that will not be affected by updates, using a JSON format as {tablename:'col1,col2'}.");
                    Sets(dt, "AutoCache.TaskTime", AppConfig.AutoCache.TaskTime + " (ms)", "When AutoCacheConn is enabled, the task time (in milliseconds) for regularly scanning the database.");
                    Sets(dt, "AutoCacheConn", HideConnPassword(AppConfig.AutoCache.Conn), "For auto remove cache when database data change.");
                }

                else if (type == "cyq.data-redis")
                {
                    if (!string.IsNullOrEmpty(AppConfig.Redis.Servers))
                    {
                        dt.NewRow(true);
                        Sets(dt, "RedisUseDBCount", AppConfig.Redis.UseDBCount, "Redis use db count.");
                        Sets(dt, "RedisUseDBIndex", AppConfig.Redis.UseDBIndex, "Redis use db index.");
                        string[] items = AppConfig.Redis.Servers.Split(',');
                        Sets(dt, "----------RedisServers - Count", items.Length, "Num of server node for redis (Show Only).");

                        for (int i = 0; i < items.Length; i++)
                        {
                            Sets(dt, "----------RedisServers - " + (i + 1), items[i], "Server node for redis (Show Only).");
                        }

                        if (!string.IsNullOrEmpty(AppConfig.Redis.ServersBak))
                        {
                            items = AppConfig.Redis.ServersBak.Split(',');
                            Sets(dt, "----------RedisServersBak - Count", items.Length, "Num of server node for redis bak(Show Only).");
                            for (int i = 0; i < items.Length; i++)
                            {
                                Sets(dt, "----------RedisServersBak - " + (i + 1), items[i], "Server node for redis (Show Only).");
                            }
                        }
                    }
                }
                else if (type == "cyq.data-memcache")
                {
                    if (!string.IsNullOrEmpty(AppConfig.MemCache.Servers))
                    {
                        string[] items = AppConfig.MemCache.Servers.Split(',');
                        Sets(dt, "----------MemCacheServers - Count", items.Length, "Num of server node for memcache (Show Only).");

                        for (int i = 0; i < items.Length; i++)
                        {
                            Sets(dt, "----------MemCacheServers - " + (i + 1), items[i], "Server node for memcache (Show Only).");
                        }

                        if (!string.IsNullOrEmpty(AppConfig.MemCache.ServersBak))
                        {
                            items = AppConfig.MemCache.ServersBak.Split(',');
                            Sets(dt, "----------MemCacheServersBak - Count", items.Length, "Num of server node for memcache bak(Show Only).");
                            for (int i = 0; i < items.Length; i++)
                            {
                                Sets(dt, "----------MemCacheServersBak - " + (i + 1), items[i], "Server node for memcache (Show Only).");
                            }
                        }
                    }
                }

                else if (type == "cyq.data-debug")
                {
                    Sets(dt, "Debug.IsEnable", AppConfig.Debug.IsEnable, "Record sql when dev debug.");
                }
                else if (type == "cyq.data-database")
                {
                    Sets(dt, "DB.CommandTimeout", AppConfig.DB.CommandTimeout + " (s)", "Timeout for database command.");
                    Sets(dt, "DB.SchemaMapPath", AppConfig.DB.SchemaMapPath, "Database metadata cache path.");
                    Sets(dt, "DB.SqlFilter", AppConfig.DB.SqlFilter + " (ms)", "Write sql to log file when sql exe time > value(value must>0).");
                    dt.NewRow(true);
                    Sets(dt, "DB.HiddenFields", AppConfig.DB.HiddenFields, "Hide fields that are not returned when querying.");
                    Sets(dt, "DB.DeleteField", AppConfig.DB.DeleteField, "Soft-deletion field name (if a table has this specified field name, MAction's delete operation will be changed to an update operation).");
                    Sets(dt, "DB.EditTimeFields", AppConfig.DB.EditTimeFields, "Name of the update time field (if the specified field name exists in the table, the update time will be automatically updated).");
                    dt.NewRow(true);

                    Sets(dt, "DB.IsPostgreLower", AppConfig.DB.IsPostgreLower, "Postgres is in lowercase mode.");
                    Sets(dt, "DB.IsTxtReadOnly", AppConfig.DB.IsTxtReadOnly, "Txt database is read-only (used for demo purposes to prevent demo accounts or data from being deleted).");
                    dt.NewRow(true);
                    Sets(dt, "DB.AutoID", AppConfig.DB.AutoID, "The sequence id config for oracle.");
                    Sets(dt, "DB.EntitySuffix", AppConfig.DB.EntitySuffix, "Entity suffix which will be ignore when orm operate.");
                    Sets(dt, "DB.MasterSlaveTime", AppConfig.DB.MasterSlaveTime + " (s)", "The duration of user operations on the primary database when using read-write separation.");
                }
            }
            dt.Bind(View);
        }
        private string HideConnPassword(string conn)
        {
            if (!string.IsNullOrEmpty(conn))
            {
                int i = conn.IndexOf("pwd=");
                if (i > 0)
                {
                    int end = conn.IndexOf(";", i);
                    if (end > 0)
                    {
                        return conn.Substring(0, i + 4) + "******" + conn.Substring(end);
                    }
                    else
                    {
                        return conn.Substring(0, i + 4) + "******";
                    }
                }
            }

            return conn;
        }
        private void Sets(MDataTable dt, string key, object objValue, string description)
        {

            string value = Convert.ToString(objValue);
            if (objValue is Boolean)
            {
                value = value == "True" ? "√" : "false";
            }
            if (AdminConfig.IsContainsDurableKey(key))
            {
                value = value + " 【durable】";
            }
            else if (AdminConfig.IsContainsTempKey(key))
            {
                value = value + " 【temp modify】";
            }
            dt.NewRow(true).Sets(0, key, value, description);
        }

        /// <summary>
        /// 保存配置
        /// </summary>
        public void BtnSaveConfig()
        {
            string key = Query<string>("key");
            string value = Query<string>("value");
            bool isDurable = Query<bool>("durable");
            string oldValue = string.Empty;

            //需要特殊处理的值
            switch (key)
            {
                case "Admin.Path":
                    oldValue = AdminConfig.Path; break;
                case "Doc.Path":
                    oldValue = DocConfig.Path; break;
                case "MicroService.Server.RcPath":
                    oldValue = MsConfig.Server.RcPath; break;
                case "MicroService.Client.RcPath":
                    oldValue = MsConfig.Client.RcPath; break;
                case "Taurus.Views":
                    ViewEngine.ViewsPath = null;
                    break;
            }
            if (!string.IsNullOrEmpty(oldValue))
            {
                ControllerCollector.ChangePath(oldValue, value);
            }
            AppConfig.SetApp(key, value);
            if (isDurable)
            {
                AdminConfig.AddDurableConfig(key, value);
            }
            else
            {
                AdminConfig.RemoveDurableConfig(key, value);
            }
            Write("Save success.", true);
        }
    }
}
