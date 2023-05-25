using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Taurus.Plugin.MicroService
{
    internal class RpcClient : MyWebClient
    {
        static RpcClient()
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls | SecurityProtocolType.Ssl3 | (SecurityProtocolType)12288;
        }
    }
}
