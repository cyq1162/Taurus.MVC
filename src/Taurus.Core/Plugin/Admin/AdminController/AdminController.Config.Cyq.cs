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
        private void ConfigCyq()
        {
            string type = Query<string>("t", "mvc").ToLower();
            MDataTable dt = new MDataTable();
            dt.Columns.Add("ConfigKey,ConfigValue,Description");
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
                Sets(dt, "DB.PrintSql", AppConfig.DB.PrintSql + " (ms)", "Write sql to log file when sql exe time > value (value must>=0).");
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
                Sets(dt, "Redis.Timeout", AppConfig.Redis.Timeout + " (ms)", "Socket connection establishment timeout time (milliseconds).");
                Sets(dt, "Redis.MaxSocket", AppConfig.Redis.MaxSocket, "Maximum socket pool size.");
                Sets(dt, "Redis.MaxWait", AppConfig.Redis.MaxWait + " (ms)", "Timeout (in milliseconds) waiting for a request from the socket pool.");
                Sets(dt, "Redis.UseDBCount", AppConfig.Redis.UseDBCount, "Redis use db count.");
                Sets(dt, "Redis.UseDBIndex", AppConfig.Redis.UseDBIndex, "Redis use db index.");
                Sets(dt, "Redis.Servers", AppConfig.Redis.Servers, "Redis servers.");
                if (!string.IsNullOrEmpty(AppConfig.Redis.Servers))
                {
                    string[] items = AppConfig.Redis.Servers.Split(',');
                    Sets(dt, "----------Redis.Servers - Count", items.Length, "Num of server node for redis (Show Only).");

                    for (int i = 0; i < items.Length; i++)
                    {
                        Sets(dt, "----------Redis.Servers - " + (i + 1), items[i], "Server node for redis (Show Only).");
                    }
                }
            }
            else if (type == "cyq.data-memcache")
            {
                Sets(dt, "MemCache.Timeout", AppConfig.MemCache.Timeout + " (ms)", "Socket connection establishment timeout time (milliseconds).");
                Sets(dt, "MemCache.MaxSocket", AppConfig.MemCache.MaxSocket, "Maximum socket pool size.");
                Sets(dt, "MemCache.MaxWait", AppConfig.MemCache.MaxWait + " (ms)", "Timeout (in milliseconds) waiting for a request from the socket pool.");
                Sets(dt, "MemCache.Servers", AppConfig.MemCache.Servers, "MemCache servers.");
                if (!string.IsNullOrEmpty(AppConfig.MemCache.Servers))
                {
                    string[] items = AppConfig.MemCache.Servers.Split(',');
                    Sets(dt, "----------MemCache.Servers - Count", items.Length, "Num of server node for memcache (Show Only).");

                    for (int i = 0; i < items.Length; i++)
                    {
                        Sets(dt, "----------MemCache.Servers - " + (i + 1), items[i], "Server node for memcache (Show Only).");
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
                Sets(dt, "DB.PrintSql", AppConfig.DB.PrintSql + " (ms)", "Write sql to  to 【Debug_PrintSql*.txt】 when sql exe time > value (value must>=0).");
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
            dt.Bind(View);
        }
    }
}
