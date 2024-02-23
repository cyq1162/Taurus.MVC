using System;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 系统环境信息
    /// </summary>
    internal partial class AdminController
    {
        private void InitCount()
        {
            Assembly[] assList = AppDomain.CurrentDomain.GetAssemblies();
            int taurusCount = 0, microsoftCount = 0, systemCount = 0;
            foreach (Assembly assembly in assList)
            {
                AssemblyName assName = assembly.GetName();
                if (assName.Name.StartsWith("Microsoft"))
                {
                    microsoftCount++;
                }
                else if (assName.Name.StartsWith("System") || assName.Name.StartsWith("netstandard") || assName.Name.StartsWith("mscorlib") || assName.Name.StartsWith("Anonymously"))
                {
                    systemCount++;
                }
                else
                {
                    taurusCount++;
                }
            }
            View.KeyValue.Add("Count", assList.Length.ToString());
            View.KeyValue.Add("Count-Taurus", taurusCount.ToString());
            View.KeyValue.Add("Count-Microsoft", microsoftCount.ToString());
            View.KeyValue.Add("Count-System", systemCount.ToString());
        }

        /// <summary>
        /// 操作系统环境信息
        /// </summary>
        public void OSInfo()
        {
            InitCount();
            MDataTable dtTaurus = new MDataTable();
            dtTaurus.Columns.Add("ConfigKey,ConfigValue,Description");
            string type = Query<string>("t", "os").ToLower();

            if (type == "os")
            {
                dtTaurus.NewRow(true).Sets(0, "App-StartTime", MsRun.StartTime.ToString("yyyy-MM-dd HH:mm:ss"), "Applicatin start time.");
                dtTaurus.NewRow(true).Sets(0, "Client-IP-Public", Request.UserHostAddress, "Client public ip.");
                IPAddress[] ips = Dns.GetHostAddresses(Request.Url.Host);
                if(ips != null && ips.Length > 0)
                {
                    dtTaurus.NewRow(true).Sets(0, "Server-IP-Public", ips[0].ToString(), "Server public ip.");
                }
                dtTaurus.NewRow(true).Sets(0, "Server-IP-Internal", MvcConst.HostIP, "Server internal ip.");
                dtTaurus.NewRow(true).Sets(0, "Runtime-Version", (AppConfig.IsNetCore ? ".Net Core - " : ".Net Framework - ") + Environment.Version, "Version of the common language runtime.");
                dtTaurus.NewRow(true).Sets(0, "OS-Version", Environment.OSVersion, "Operating system.");
                dtTaurus.NewRow(true).Sets(0, "ProcessID", MvcConst.ProcessID, "Process id.");
                dtTaurus.NewRow(true).Sets(0, "ThreadID", Thread.CurrentThread.ManagedThreadId, "Identifier for the managed thread.");
                dtTaurus.NewRow(true).Sets(0, "ThreadCount", MvcConst.Proc.Threads.Count, "Number of threads for the process.");
                //long tc = Environment.TickCount > 0 ? (long)Environment.TickCount : ((long)int.MaxValue + (Environment.TickCount & int.MaxValue));
                //TimeSpan ts = TimeSpan.FromMilliseconds(tc);
                //dtTaurus.NewRow(true).Sets(0, "TickCount", (int)ts.TotalSeconds + "s | " + (int)ts.TotalMinutes + "m | " + (int)ts.TotalHours + "h | " + (int)ts.TotalDays + "d", "Time since the system started(max(days)<=49.8)).");
                dtTaurus.NewRow(true).Sets(0, "ProcessorCount", Environment.ProcessorCount, "Number of processors on the machine.");
                dtTaurus.NewRow(true).Sets(0, "MachineName", Environment.MachineName, "Name of computer.");
                dtTaurus.NewRow(true).Sets(0, "UserName", Environment.UserName, "Name of the person who is logged on to Windows.");
                dtTaurus.NewRow(true).Sets(0, "WorkingSet", Environment.WorkingSet / 1024 / 1024 + "MB", "Physical memory mapped to the process context.");
                dtTaurus.NewRow(true).Sets(0, "CurrentDirectory", AppConfig.RunPath, "Web application path of the working directory.");
            }
            else if (type.StartsWith("ass"))
            {
                int t = 0;
                switch (type)
                {
                    case "ass-taurus":
                        t = 1; break;
                    case "ass-microsoft":
                        t = 2; break;
                    case "ass-system":
                        t = 3; break;
                }
                MDataTable dtSystem = new MDataTable();
                dtSystem.Columns = dtTaurus.Columns;
                MDataTable dtMicrosoft = new MDataTable();
                dtMicrosoft.Columns = dtTaurus.Columns;
                Assembly[] assList = AppDomain.CurrentDomain.GetAssemblies();
                foreach (Assembly assembly in assList)
                {
                    string desc = GetDescription(assembly);
                    AssemblyName assName = assembly.GetName();
                    if (assName.Name.StartsWith("Microsoft"))
                    {
                        if (t == 0 || t == 2)
                        {
                            dtMicrosoft.NewRow(true).Sets(0, assName.Name, assName.Version.ToString(), desc);
                        }
                    }
                    else if (assName.Name.StartsWith("System") || assName.Name.StartsWith("netstandard") || assName.Name.StartsWith("mscorlib") || assName.Name.StartsWith("Anonymously"))
                    {
                        if (t == 0 || t == 3)
                        {
                            dtSystem.NewRow(true).Sets(0, assName.Name, assName.Version.ToString(), desc);
                        }
                    }
                    else
                    {
                        if (t == 0 || t == 1)
                        {
                            dtTaurus.NewRow(true).Sets(0, assName.Name, assName.Version.ToString(), desc);
                        }
                    }
                }
                dtTaurus.Rows.Sort("ConfigKey");
                if (dtMicrosoft.Rows.Count > 0)
                {
                    dtMicrosoft.Rows.Sort("ConfigKey");
                    dtTaurus.Merge(dtMicrosoft);
                }
                if (dtSystem.Rows.Count > 0)
                {
                    dtSystem.Rows.Sort("ConfigKey");
                    dtTaurus.Merge(dtSystem);
                }

            }
            dtTaurus.Bind(View);
        }


        private string GetDescription(Assembly assembly)
        {
            string desc = assembly.FullName;
            object[] attrs = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
            if (attrs != null && attrs.Length > 0)
            {
                desc = ((AssemblyDescriptionAttribute)attrs[0]).Description;
            }
            else
            {
                attrs = assembly.GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attrs != null && attrs.Length > 0)
                {
                    desc = ((AssemblyTitleAttribute)attrs[0]).Title;
                }
            }
            return desc;
        }
    }
}
