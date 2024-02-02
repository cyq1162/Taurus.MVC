using System;
using System.Web;
using System.Collections.Generic;
using Taurus.Mvc;
using CYQ.Data;
using System.Threading;
using CYQ.Data.Tool;
using System.Net;
using Taurus.Plugin.Rpc;
using Yarp.ReverseProxy.Forwarder;
using System.Net.Http;
using System.Threading.Tasks;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 测试YARP，并发2000左右，比想象的差一些，自己写的还有3000多并发，怀疑还有优化的空间。
    /// </summary>

    public static partial class Gateway
    {
        internal static IHttpForwarder httpForwarder;
        private static HttpMessageInvoker HttpClient { get; set; }

        static Gateway()
        {
            var shh = new SocketsHttpHandler()
            {
                MaxConnectionsPerServer = 4096,
                Proxy = null,
                UseProxy = false,
                UseCookies = false,
                AllowAutoRedirect = false,
                AutomaticDecompression = DecompressionMethods.None
            };

            HttpClient = new HttpMessageInvoker(shh);
        }
        public static bool YarpProxy(HttpContext context, string url)
        {
            return YarpProxy2(context, url).Result;
        }
        private static async Task<bool> YarpProxy2(HttpContext context, string url)
        {
            try
            {
                await httpForwarder.SendAsync(context.NetCoreContext, url, HttpClient, ForwarderRequestConfig.Empty, HttpTransformer.Empty);
                return true;
            }
            catch (Exception err)
            {

                return false;
            }


        }
    }
}
