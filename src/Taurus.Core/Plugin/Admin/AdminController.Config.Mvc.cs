using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using Taurus.Plugin.Doc;
using System.Configuration;
using System.IO;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 应用配置信息
    /// </summary>
    internal partial class AdminController
    {
        private void ConfigMvc()
        {
            string type = Query<string>("t", "mvc").ToLower();
            MDataTable dt = new MDataTable();
            dt.Columns.Add("ConfigKey,ConfigValue,Description");
            #region Mvc
            if (type == "mvc")
            {
                Sets(dt, "Mvc.IsEnable", MvcConfig.IsEnable, "Taurus mvc  is enable.");
                Sets(dt, "Mvc.IsAllowIPHost", MvcConfig.IsAllowIPHost, "Taurus mvc  is allow ip host request.");
                Sets(dt, "Mvc.IsPrintRequestLog", MvcConfig.IsPrintRequestLog, "Print mvc suffix request logs to 【Debug_RequestLog*.txt】 for debug.");
                Sets(dt, "Mvc.IsPrintRequestSql", MvcConfig.IsPrintRequestSql, "Print mvc suffix request sqls to 【Debug_RequestSql*.txt】 for debug.");
                Sets(dt, "Mvc.RunUrl", MvcConfig.RunUrl, "Application run url （http request url）.");
                Sets(dt, "Mvc.DefaultUrl", MvcConfig.DefaultUrl, "Application default url （url local path）.");
                Sets(dt, "Mvc.Suffix", MvcConfig.Suffix, "Deal with mvc suffix.");
                Sets(dt, "Mvc.SubAppName", MvcConfig.SubAppName, "Name of deploy as sub application.");
                Sets(dt, "Mvc.RouteMode", GetRouteModeText(), "Route mode 【0、1、2】 for selected.");
            }
            else if (type == "mvc-controller")
            {
                Sets(dt, "Mvc.Controllers", MvcConfig.Controllers, "Load controller dll names.");
                var controllerAssemblys = ControllerCollector.GetAssemblys();
                Sets(dt, "----------Mvc.Controllers - Count", controllerAssemblys.Count, "Num of controller dll for mvc (Show Only).");
                for (int i = 0; i < controllerAssemblys.Count; i++)
                {
                    var assembly = controllerAssemblys[i].GetName();
                    Sets(dt, "----------Mvc.Controllers - " + (i + 1), assembly.Name + ".dll", assembly.Version.ToString());
                }
            }
            else if (type == "mvc-view")
            {
                Sets(dt, "Mvc.Views", MvcConfig.Views, "Mvc view folder name.");
                var viewFolders = Directory.GetDirectories(ViewEngine.ViewsPath);
                Sets(dt, "----------Mvc.Views - Count", viewFolders.Length, "Num of view folder for mvc (Show Only).");
                for (int i = 0; i < viewFolders.Length; i++)
                {
                    var folder = Path.GetFileNameWithoutExtension(viewFolders[i]);
                    Sets(dt, "----------Mvc.Views - " + (i + 1), folder, "/" + MvcConfig.Views + "/" + folder);
                }
            }
            #endregion
            dt.Bind(View);
        }
    }
}
