using System;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using System.Threading;
using System.Diagnostics;
using System.Reflection;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 系统环境信息
    /// </summary>
    internal partial class AdminController
    {
        /// <summary>
        /// 操作系统环境信息
        /// </summary>
        public void OSInfo()
        {
            MDataTable dtTaurus = new MDataTable();
            dtTaurus.Columns.Add("ConfigKey,ConfigValue,Description");
            string type = Query<string>("t", "os").ToLower();
            Assembly[] assList = AppDomain.CurrentDomain.GetAssemblies();
            View.KeyValue.Set("Count", assList.Length.ToString());
            if (type == "os")
            {
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
                long tc = Environment.TickCount > 0 ? (long)Environment.TickCount : ((long)int.MaxValue + (Environment.TickCount & int.MaxValue));
                TimeSpan ts = TimeSpan.FromMilliseconds(tc);
                dtTaurus.NewRow(true).Sets(0, "TickCount", (int)ts.TotalSeconds + "s | " + (int)ts.TotalMinutes + "m | " + (int)ts.TotalHours + "h | " + (int)ts.TotalDays + "d", "Time since the system started(max(days)<=49.8)).");
                dtTaurus.NewRow(true).Sets(0, "ProcessorCount", Environment.ProcessorCount, "Number of processors on the machine.");
                dtTaurus.NewRow(true).Sets(0, "MachineName", Environment.MachineName, "Name of computer.");
                dtTaurus.NewRow(true).Sets(0, "UserName", Environment.UserName, "Name of the person who is logged on to Windows.");
                dtTaurus.NewRow(true).Sets(0, "WorkingSet", Environment.WorkingSet / 1024 + "KB | " + Environment.WorkingSet / 1024 / 1024 + "MB", "Physical memory mapped to the process context.");
                dtTaurus.NewRow(true).Sets(0, "CurrentDirectory", Environment.CurrentDirectory, "Fully qualified path of the working directory.");
            }
            else if (type == "ass")
            {
                MDataTable dtSystem=new MDataTable();
                dtSystem.Columns = dtTaurus.Columns;
                foreach (Assembly assembly in assList)
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
                    AssemblyName assName = assembly.GetName();
                    if (assName.Name.StartsWith("Microsoft.") || assName.Name.StartsWith("System."))
                    {
                        dtSystem.NewRow(true).Sets(0, assName.Name, assName.Version.ToString(), desc);
                    }
                    else
                    {
                        dtTaurus.NewRow(true).Sets(0, assName.Name, assName.Version.ToString(), desc);
                    }
                }
                dtTaurus.Rows.Sort("ConfigKey");
                dtTaurus.NewRow(true);
                dtSystem.Rows.Sort("ConfigKey");
                dtTaurus.Merge(dtSystem);
            }
            dtTaurus.Bind(View);
        }

    }
}
