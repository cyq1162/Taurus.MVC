using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTCConfig
    {
        /// <summary>
        /// DTC 默认监听队列名称，不同项目应该有不同的名称。
        /// 配置项：DTC.ProjectName ：TaurusProj
        /// </summary>
        public static string ProjectName
        {
            get
            {
                string projectName = AppConfig.GetApp("DTC.ProjectName");
                if (string.IsNullOrEmpty(projectName))
                {
                    Assembly ass = Assembly.GetEntryAssembly();
                    if (ass == null)
                    {
                        ass = GetEntryAssembly();
                    }
                    if (ass == null)
                    {
                        projectName = Environment.UserName;
                    }
                    else
                    {
                        projectName = ass.GetName().Name;
                    }
                    projectName = projectName.Replace(".", "").Replace("_", "");
                    AppConfig.SetApp("DTC.ProjectName", projectName);
                }
                return projectName;
            }
            set
            {
                AppConfig.SetApp("DTC.ProjectName", value);
            }
        }

        private static Assembly GetEntryAssembly()
        {
            string current = Assembly.GetExecutingAssembly().GetName().Name;
            Assembly[] assList = AppDomain.CurrentDomain.GetAssemblies();
            List<Assembly> projs = new List<Assembly>();
            Assembly ctl = null;
            foreach (Assembly assembly in assList)
            {
                AssemblyName assName = assembly.GetName();
                if (assName.Name.StartsWith("Microsoft") || assName.Name.StartsWith("System") || assName.Name.StartsWith("netstandard") 
                    || assName.Name.StartsWith("mscorlib") || assName.Name.StartsWith("Anonymously"))
                {
                    continue;
                }
                else
                {
                    projs.Add(assembly);
                    if (ctl == null)
                    {
                        foreach (AssemblyName item in assembly.GetReferencedAssemblies())
                        {
                            if (current == item.Name)
                            {
                                ctl = assembly;
                                //引用了自身，要么是控制器，要么控制器本身就是主运行程序。
                                break;
                            }
                        }
                    }
                }
            }
            if (ctl != null)
            {
                current = ctl.GetName().Name;
                foreach (Assembly assembly in projs)
                {
                    foreach (AssemblyName item in assembly.GetReferencedAssemblies())
                    {
                        if (current == item.Name)
                        {
                            ctl = assembly;
                            //引用了自身，要么是控制器，要么控制器本身就是主运行程序。
                            break;
                        }
                    }
                }
            }
            return ctl;
        }
    }
}
