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
        private void ConfigKestrel()
        {
            string type = Query<string>("t", "mvc").ToLower();
            MDataTable dt = new MDataTable();
            dt.Columns.Add("ConfigKey,ConfigValue,Description");
            if (type.StartsWith("kestrel"))
            {
                if (type == "kestrel")
                {
                    Sets(dt, "Kestrel.Urls", MvcConfig.Kestrel.Urls, "Kestrel listen host. - 【Restart】");
                    Sets(dt, "Kestrel.AllowSynchronousIO", MvcConfig.Kestrel.AllowSynchronousIO, "Kestrel allow synchronous IO.");
                    Sets(dt, "Kestrel.AddServerHeader", MvcConfig.Kestrel.AddServerHeader, "Kestrel is add server header.");
                    Sets(dt, "Kestrel.SslPort", MvcConfig.Kestrel.SslPort, "Ssl port for https. - 【Restart】");
                    Sets(dt, "Kestrel.SslPath", MvcConfig.Kestrel.SslPath, "Ssl path for https (*.pfx for ssl , *.txt for pwd).");
                    var cers = MvcConfig.Kestrel.SslCertificate;
                    Sets(dt, "----------SslCertificate - Count", cers.Count, "Num of ssl for https (Show Only).");
                    if (cers.Count > 0)
                    {
                        int i = 1;
                        foreach (string name in cers.Keys)
                        {
                            Sets(dt, "----------SslCertificate - " + i, name, "Domain ssl for https (Show Only).");
                            i++;
                        }
                    }
                }
                else if (type == "kestrel-hostfilter")
                {
                    Sets(dt, "Kestrel.AllowedHosts", MvcConfig.Kestrel.AllowedHosts, "Allowed hosts. - 【Restart】");
                    Sets(dt, "Kestrel.AllowEmptyHosts", MvcConfig.Kestrel.AllowEmptyHosts, "Indicates if requests without hosts are allowed.");
                    Sets(dt, "Kestrel.IncludeFailureMessage", MvcConfig.Kestrel.IncludeFailureMessage, "Indicates if the 400 response should include a default message.");
                }
                else if (type == "kestrel-connection")
                {
                    Sets(dt, "Kestrel.Limits.MaxConcurrentConnections", MvcConfig.Kestrel.Limits.MaxConcurrentConnections, "Maximum number of open connections. - 【Restart】");
                    Sets(dt, "Kestrel.Limits.MaxConcurrentUpgradedConnections", MvcConfig.Kestrel.Limits.MaxConcurrentUpgradedConnections, "Maximum number of open, upgraded connections. - 【Restart】");
                }
                else if (type == "kestrel-request")
                {
                    Sets(dt, "Kestrel.Limits.MaxRequestBodySize", MvcConfig.Kestrel.Limits.MaxRequestBodySize, "Maximum allowed size of any request body in bytes. - 【Restart】");
                    Sets(dt, "Kestrel.Limits.MaxRequestBufferSize", MvcConfig.Kestrel.Limits.MaxRequestBufferSize, "Maximum size of the request buffer. - 【Restart】");
                    Sets(dt, "Kestrel.Limits.MaxRequestLineSize", MvcConfig.Kestrel.Limits.MaxRequestLineSize, "Maximum allowed size for the HTTP request line.");
                    dt.NewRow(true);
                    Sets(dt, "Kestrel.Limits.MaxRequestHeaderCount", MvcConfig.Kestrel.Limits.MaxRequestHeaderCount, "Maximum allowed number of headers per HTTP request.");
                    Sets(dt, "Kestrel.Limits.MaxRequestHeadersTotalSize", MvcConfig.Kestrel.Limits.MaxRequestHeadersTotalSize, "Maximum allowed size for the HTTP request headers.");
                }
                else if (type == "kestrel-response")
                {
                    Sets(dt, "Kestrel.Limits.MaxResponseBufferSize", MvcConfig.Kestrel.Limits.MaxResponseBufferSize, "Maximum size of the response buffer. - 【Restart】");
                }
                else if (type == "kestrel-timeout")
                {
                    Sets(dt, "Kestrel.Limits.KeepAliveTimeout", MvcConfig.Kestrel.Limits.KeepAliveTimeout + " (m)", "Keep-alive timeout (minute). - 【Restart】");
                    Sets(dt, "Kestrel.Limits.RequestHeadersTimeout", MvcConfig.Kestrel.Limits.RequestHeadersTimeout + " (s)", "Request header timeout (sencond). - 【Restart】");
                }
            }
            dt.Bind(View);
        }
    }
}
