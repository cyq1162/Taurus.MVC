using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
namespace Taurus.Core
{
    internal class Const
    {
        internal const string Default = "Default";
        internal const string Controller = "Controller";
        internal const string DefaultController = "DefaultController";
        internal const string DocController = "DocController";
        internal const string AuthController = "AuthController";
        internal const string TaurusCoreController = "Taurus.Core.Controller";
        internal const string MicroServiceController = "MicroServiceController";

        internal const string Doc = "Doc";
        internal const string Auth = "Auth";
        internal const string MicroService = "MicroService";
        internal const string CoreDoc = "Core.Doc";
        internal const string CoreAuth = "Core.Auth";
        internal const string CoreMicroService = "Core.MicroService";
        internal const string Proxy = "Proxy";//MicroService.Proxy

        internal const string CheckToken = "CheckToken";
        internal const string CheckAck = "CheckAck";
        internal const string CheckMicroService = "CheckMicroService";
        internal const string RouteMapInvoke = "RouteMapInvoke";
        internal const string BeforeInvoke = "BeforeInvoke";
        internal const string EndInvoke = "EndInvoke";
        internal const string Record = "Record";

        internal const string TokenAttribute = "TokenAttribute";
        internal const string AckAttribute = "AckAttribute";
        internal const string MicroServiceAttribute = "MicroServiceAttribute";
        internal const string HttpGetAttribute = "HttpGetAttribute";
        internal const string HttpPostAttribute = "HttpPostAttribute";
        internal const string HttpHeadAttribute = "HttpHeadAttribute";
        internal const string HttpPutAttribute = "HttpPutAttribute";
        internal const string HttpDeleteAttribute = "HttpDeleteAttribute";

        internal const string NeedConfigController = "Please make sure config appsettings : add key=\"Taurus.Controllers\" value=\"YourControllerProjectName\" is right!";

        internal static bool IsStartDoc
        {
            get
            {
                return AppConfig.GetAppBool(AppSettings.IsStartDoc, true);
            }
        }

        internal static bool IsStartAuth
        {
            get
            {
                return !string.IsNullOrEmpty(AppConfig.GetApp(AppSettings.Auth));
            }
        }
    }
}
