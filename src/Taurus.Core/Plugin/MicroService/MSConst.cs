
using CYQ.Data;
using System;
using System.IO;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 常量
    /// </summary>
    internal class MsConst
    {
        /// <summary>
        /// 请求头带上的Header的Key名称
        /// </summary>
        internal const string HeaderKey = "mskey";
        /// <summary>
        /// 网关
        /// </summary>
        internal const string Gateway = "gateway";
        /// <summary>
        /// 注册中心（主）
        /// </summary>
        internal const string RegistryCenter = "registrycenter";

        /// <summary>
        /// 注册中心（从）
        /// </summary>
        internal const string RegistryCenterOfSlave = "registrycenterofslave";

        internal const string ServerRegistryCenterJsonPath = "/microservice/server_rc.json";
        internal const string ServerRegistryCenterOfSlaveJsonPath = "/microservice/server_rcofslave.json";
        internal const string ServerGatewayJsonPath = "/microservice/server_gateway.json";
        internal const string ClientGatewayJsonPath = "/microservice/client_gateway.json";
        internal const string ServerHost2Path = "/microservice/server_host2.json";
        internal const string ClientHost2Path = "/microservice/client_host2.json";
    }
}
