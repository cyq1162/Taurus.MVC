using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using Taurus.Plugin.Doc;
using System.Configuration;
using Taurus.Plugin.CORS;
using Taurus.Plugin.Metric;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 应用配置信息
    /// </summary>
    internal partial class AdminController
    {
        private void ConfigMicroService()
        {
            string type = Query<string>("t", "mvc").ToLower();
            MDataTable dt = new MDataTable();
            dt.Columns.Add("ConfigKey,ConfigValue,Description");

            if (type.StartsWith("plugin-microservice"))
            {
                #region MicroService

                if (type == "plugin-microservice-server")
                {
                    Sets(dt, "MicroService.Server.Type", MsConfig.Server.Type, "Server type 【Gateway、RegistryCenter】【*】.");
                    Sets(dt, "MicroService.Server.RcUrl", MsConfig.Server.RcUrl, "Registry center url【?】.");
                    Sets(dt, "MicroService.Server.IsEnable", MsConfig.Server.IsEnable, "Microservice server (registry center, gateway) plugin.");
                    if (!MsConfig.IsRegistryCenterOfMaster)
                    {
                        Sets(dt, "MicroService.Server.IsAllowSyncIP", MsConfig.Server.IsAllowSyncIP, "IP limit : Synchronize ip blackname list from registry center.");
                    }
                    Sets(dt, "MicroService.Server.RcKey", MsConfig.Server.RcKey, "Registry center secret key.");
                    Sets(dt, "MicroService.Server.RcPath", MsConfig.Server.RcPath, "Registry center local path.");
                    if (MsConfig.IsGateway)
                    {
                        Sets(dt, "MicroService.Server.GatewayTimeout", MsConfig.Server.GatewayTimeout + " (s)", "Gateway timeout (second) for request forward.");
                        Sets(dt, "MicroService Gateway Proxy LastTime", Gateway.LastProxyTime.ToString("yyyy-MM-dd HH:mm:ss"), "The last time the proxy forwarded the request【Read Only】.");
                    }
                    Sets(dt, "MicroService.Server.RcUrl - 2", Server.Host2, "Registry center backup url【Read Only】.");
                }

                if (type == "plugin-microservice-client")
                {
                    Sets(dt, "MicroService.Client.Name", MsConfig.Client.Name, "Client registry module name【*】.");
                    Sets(dt, "MicroService.Client.RcUrl", MsConfig.Client.RcUrl, "Registry center url【*】.");
                    Sets(dt, "MicroService.Client.IsEnable", MsConfig.Client.IsEnable, "Microservice client plugin.");
                    Sets(dt, "MicroService.Client.IsAllowSyncConfig", MsConfig.Client.IsAllowSyncConfig, "Client is allow synchronize config from registry center.");
                    Sets(dt, "MicroService.Client.IsAllowRemoteExit", MsConfig.Client.IsAllowRemoteExit, "Client is allow stop by registry center.");
                    Sets(dt, "MicroService.Client.Domain", MsConfig.Client.Domain, "Client bind domain.");
                    Sets(dt, "MicroService.Client.Version", MsConfig.Client.Version, "Client web version.");
                    Sets(dt, "MicroService.Client.RcKey", MsConfig.Client.RcKey, "Registry center secret key.");
                    Sets(dt, "MicroService.Client.RcUrl - 2", Client.Host2, "Registry center backup url.");
                    Sets(dt, "MicroService.Client.RcPath", MsConfig.Client.RcPath, "Registry center local path.");

                }
                #endregion
            }
            dt.Bind(View);
        }
    }
}
