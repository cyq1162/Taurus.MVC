using CYQ.Data;
namespace Taurus.Mvc
{
    internal class ReflectConst
    {
        internal const string Default = "Default";
        internal const string Controller = "Controller";
        internal const string DefaultController = "DefaultController";
        internal const string DocController = "DocController";
        internal const string TaurusMvcController = "Taurus.Mvc.Controller";
        internal const string MicroServiceController = "MicroServiceController";

        internal const string Doc = "Doc";
        internal const string MicroService = "MicroService";
        internal const string CoreDoc = "Core.Doc";
        internal const string CoreMicroService = "Core.MicroService";

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


        //internal static bool IsStartAuth
        //{
        //    get
        //    {
        //        return !string.IsNullOrEmpty(AppConfig.GetApp(MvcConfigConst.Auth));
        //    }
        //}
    }
}
