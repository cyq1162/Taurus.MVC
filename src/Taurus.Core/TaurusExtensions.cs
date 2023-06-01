using CYQ.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using System;
using Taurus.Plugin.MicroService;
using Taurus.Mvc;
using Taurus.Core;
using Microsoft.AspNetCore.Hosting.Server.Features;
using System.Configuration;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Http
{
    public static class TaurusExtensions
    {
        /// <summary>
        /// 默认配置：HttpContext、FormOptions、KestrelServerOptions。
        /// </summary>
        /// <param name="services"></param>
        public static void AddTaurusMvc(this IServiceCollection services)
        {
            services.AddHttpContext();
            //开放表单不限制长度。
            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = MvcConfig.Kestrel.Limits.MaxRequestBodySize);
            services.Configure<KestrelServerOptions>((x) =>
            {
                if (MvcConfig.Kestrel.SslCertificate.Count > 0)
                {
                    //重新绑定监听端口。
                    #region 处理 Https 端口监听
                    x.Listen(IPAddress.Any, MvcConfig.Kestrel.SslPort, op =>
                    {
                        op.UseHttps(opx =>
                        {
                            var certificates = MvcConfig.Kestrel.SslCertificate;
                            opx.ServerCertificateSelector = (connectionContext, name) =>
                               name != null && certificates.TryGetValue(name, out var cert) ? cert : certificates[0];
                        });
                    });
                    #endregion

                    #region 处理常规端口绑定配置
                    string host = MvcConfig.Kestrel.Urls;
                    string url = !string.IsNullOrEmpty(host) ? host : MvcConfig.RunUrl;
                    if (!string.IsNullOrEmpty(url) && !url.StartsWith("https"))
                    {
                        string[] items = url.Split(":");
                        if (items.Length == 2)
                        {
                            x.Listen(IPAddress.Any, 80);
                        }
                        else if (items.Length == 3)
                        {
                            int port = int.Parse(items[2]);
                            if (port > 0 && port < 65535)
                            {
                                x.Listen(IPAddress.Any, port);
                            }
                        }
                    }
                    #endregion
                }
                x.AllowSynchronousIO = MvcConfig.Kestrel.AllowSynchronousIO;

                #region Limits 配置设置

                if (MvcConfig.Kestrel.Limits.MaxRequestBufferSize == long.MaxValue)
                {
                    x.Limits.MaxRequestBufferSize = null;
                }
                else
                {
                    x.Limits.MaxRequestBufferSize = MvcConfig.Kestrel.Limits.MaxRequestBufferSize;
                }
                if (MvcConfig.Kestrel.Limits.MaxRequestBodySize == long.MaxValue)
                {
                    x.Limits.MaxRequestBodySize = null;
                }
                else
                {
                    x.Limits.MaxRequestBodySize = MvcConfig.Kestrel.Limits.MaxRequestBodySize;
                }

                x.Limits.MaxRequestLineSize = MvcConfig.Kestrel.Limits.MaxRequestLineSize;
                x.Limits.MaxRequestHeaderCount = MvcConfig.Kestrel.Limits.MaxRequestHeaderCount;
                x.Limits.MaxRequestHeadersTotalSize = MvcConfig.Kestrel.Limits.MaxRequestHeadersTotalSize;

                x.Limits.MaxConcurrentConnections = 10;
                //为整个应用设置并发打开的最大 TCP 连接数,默认情况下，最大连接数不受限制 (NULL)
                //if (MvcConfig.Kestrel.Limits.MaxConcurrentConnections != long.MaxValue)
                //{
                //    x.Limits.MaxConcurrentConnections = MvcConfig.Kestrel.Limits.MaxConcurrentConnections;
                //}
                //对于已从 HTTP 或 HTTPS 升级到另一个协议（例如，Websocket 请求）的连接，有一个单独的限制。 连接升级后，不会计入 MaxConcurrentConnections 限制
                if (MvcConfig.Kestrel.Limits.MaxConcurrentUpgradedConnections != long.MaxValue)
                {
                    x.Limits.MaxConcurrentUpgradedConnections = MvcConfig.Kestrel.Limits.MaxConcurrentUpgradedConnections;
                }
                if (MvcConfig.Kestrel.Limits.MaxResponseBufferSize == long.MaxValue)
                {
                    x.Limits.MaxResponseBufferSize = null;
                }
                else
                {
                    x.Limits.MaxResponseBufferSize = MvcConfig.Kestrel.Limits.MaxResponseBufferSize;
                }
                //获取或设置保持活动状态超时。 默认值为 2 分钟。
                x.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(MvcConfig.Kestrel.Limits.KeepAliveTimeout);
                x.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(MvcConfig.Kestrel.Limits.RequestHeadersTimeout);
                #endregion
            });
        }
        /*
        /// <summary>
        /// 使用Taurus.MVC中件间功能：Net Core 3.1 把IHostingEnvironment 拆分成了：IWebHostEnvironment和IHostEnvironment 
        /// 所以增加重载方法适应。
        /// </summary>
        /// <param name="builder"></param>
        /// <param name="env">传递参数，Debug模式下，设置项目目录为根目录</param>
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
            char c = env.WebRootPath.StartsWith("/") ? '/' : '\\';
            string path = (env.WebRootPath ?? env.ContentRootPath.TrimEnd('/', '\\') + c + "wwwroot") + c;
            SetWebRootPath(path);
            //Net6新建的项目，WebRootPath竟然是空。
            return UseTaurusMvc(builder);
        }

        [Conditional("DEBUG")]
        private static void SetWebRootPath(string path)
        {
            AppConfig.WebRootPath = path;
        }
        */
        public static IApplicationBuilder UseTaurusMvc(this IApplicationBuilder builder)
        {
            //var aaa = builder.ApplicationServices.GetService<IOptions<KestrelServerOptions>>();
            //aaa.Value.Limits.MaxConcurrentConnections = 1;
            builder.UseHttpContext();

            Thread thread = new Thread(new ParameterizedThreadStart(StartMicroService));
            thread.Start(builder);
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
            if (host.Contains("*:") || host.Contains("[::]") || host.Contains("0.0.0.0"))//IP6 保留地址。
            {
                MvcConfig.RunUrl = host.Replace("*", MvcConst.HostIP).Replace("[::]", MvcConst.HostIP).Replace("0.0.0.0", MvcConst.HostIP);
            }
            else
            {
                MvcConfig.RunUrl = host;
            }
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
            else
            {
                Console.WriteLine("MvcConfig.RunUrl = " + MvcConfig.RunUrl);
            }
        }
    }
}
