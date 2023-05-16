using CYQ.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using System.Threading.Tasks;
using Taurus.MicroService;
using Taurus.Mvc;
using Taurus.Core;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Configuration;
using System.Threading;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// 为支持Asp.net core 存在的文件
    /// </summary>
    internal class TaurusMiddleware
    {
        private readonly RequestDelegate next;

        public TaurusMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (!context.Response.HasStarted)
                {
                    context.Response.Headers.Add("Server", "Taurus/" + MvcConst.Version);
                }
                if (context.Request.Path.Value.IndexOf("/App_Data/", StringComparison.OrdinalIgnoreCase) > -1)//兼容受保护的目录
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("403 Forbidden");
                }
                else
                {
                    if (context.Request.Method != "GET" && context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > 0)
                    {
                        context.Request.EnableBuffering();// 使用处：对应Rpc.Gateway.cs 代码：Proxy 方法 149行上下。
                    }
                    System.Web.HttpApplication.Instance.ExecuteEventHandler();
                    if (System.Web.HttpContext.Current.Response.HasStarted)  // || Body是只写流  (context.Response.Body != null && context.Response.Body.CanRead
                    {
                        await context.Response.WriteAsync("");
                    }
                    //处理信息
                    else
                    {
                        await next(context);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLogToTxt(ex);
            }
        }
    }
    public static class TaurusExtensions
    {
        /// <summary>
        /// 使用Taurus.MVC中件间功能：Net Core 3.1 把IHostingEnvironment 拆分成了：IWebHostEnvironment和IHostEnvironment 
        /// 所以增加重载方法适应。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseTaurusMvc(this IApplicationBuilder builder, object env)
        {
            if (env is IHostingEnvironment)
            {
                return UseTaurusMvc(builder, env as IHostingEnvironment);
            }
            throw new Exception("env must be IWebHostEnvironment or IHostingEnvironment or String");
        }
        public static IApplicationBuilder UseTaurusMvc(this IApplicationBuilder builder, IHostingEnvironment env)
        {
            //Net6新建的项目，WebRootPath竟然是空。
            return UseTaurusMvc(builder, env.WebRootPath ?? env.ContentRootPath.TrimEnd('/', '\\') + "/wwwroot");
        }
        public static IApplicationBuilder UseTaurusMvc(this IApplicationBuilder builder, string webRootPath)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(StartMicroService));
            thread.Start(builder);


            AppConfig.WebRootPath = webRootPath;//设置根目录地址，ASPNETCore的根目录和其它应用不一样。
            //执行一次，用于注册事件
            UrlRewrite url = new UrlRewrite();
            url.Init(System.Web.HttpApplication.Instance);
            ControllerCollector.InitControllers();
            return builder.UseMiddleware<TaurusMiddleware>();
        }

        private static void StartMicroService(object builderObj)
        {
            Thread.Sleep(1000);//线程延时，待监听后，再获取监听端口号。
            SetAppRunUrl(builderObj as IApplicationBuilder);
            MsRun.Start(MvcConfig.RunUrl);
        }

        private static bool SetAppRunUrl(string host)
        {
            if (string.IsNullOrEmpty(host) || host.EndsWith(":0"))
            {
                return false;
            }
            string[] items = host.Split(":");
            string port = items[items.Length - 1];
            string url = items[0] + "://" + MvcConst.HostIP + ":" + port;
            MvcConfig.RunUrl = url;
            return true;
        }


        private static void SetAppRunUrl(IApplicationBuilder builder)
        {
            if (string.IsNullOrEmpty(MvcConfig.RunUrl))
            {
                //builder.ApplicationServices.
                var saf = builder.ServerFeatures.Get<IServerAddressesFeature>();
                if (saf != null)
                {
                    foreach (var host in saf.Addresses)
                    {
                        if (SetAppRunUrl(host))
                        {
                            Console.WriteLine("Set MvcConfig.RunUrl = " + MvcConfig.RunUrl);
                            return;
                        }
                    }
                }
                if (!SetAppRunUrl(AppConfig.GetApp("Host")))
                {
                    object urls = ConfigurationManager.GetSection("urls");
                    if (urls != null)
                    {
                        SetAppRunUrl(urls.ToString());
                    }
                }

            }
        }
    }
}
