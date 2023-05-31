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
        private void ConfigMvc()
        {
            string type = Query<string>("t", "mvc").ToLower();
            MDataTable dt = new MDataTable();
            dt.Columns.Add("ConfigKey,ConfigValue,Description");
            #region Mvc
            Sets(dt, "Taurus.IsEnable", MvcConfig.IsEnable, "Taurus mvc  is enable.");
            Sets(dt, "Taurus.IsPrintRequestLog", MvcConfig.IsPrintRequestLog, "Print mvc suffix request logs to 【Debug_RequestLog*.txt】 for debug.");
            Sets(dt, "Taurus.IsPrintRequestSql", MvcConfig.IsPrintRequestSql, "Print mvc suffix request sqls to 【Debug_RequestSql*.txt】 for debug.");
            Sets(dt, "Taurus.RunUrl", MvcConfig.RunUrl, "Application run url （http request url）.");
            Sets(dt, "Taurus.DefaultUrl", MvcConfig.DefaultUrl, "Application default url （url local path）.");
            Sets(dt, "Taurus.IsAllowCORS", MvcConfig.IsAllowCORS, "Application is allow cross-origin resource sharing.");

            Sets(dt, "Taurus.RouteMode", GetRouteModeText(), "Route mode 【0、1、2】 for selected.");
            Sets(dt, "Taurus.Controllers", MvcConfig.Controllers, "Load controller dll names.");
            Sets(dt, "Taurus.Views", MvcConfig.Views, "Mvc view folder name.");
            Sets(dt, "Taurus.Suffix", MvcConfig.Suffix, "Deal with mvc suffix.");
            Sets(dt, "Taurus.SubAppName", MvcConfig.SubAppName, "Name of deploy as sub application.");
            #endregion
            dt.Bind(View);
        }
    }
}
