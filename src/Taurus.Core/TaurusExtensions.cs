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
using Microsoft.AspNetCore.HostFiltering;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Http
{
    public static partial class TaurusExtensions
    {
        static KestrelServerOptions kestrelServerOptions;
        static HostFilteringOptions hostFilteringOptions;
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
                kestrelServerOptions = x;
                LoadKestrelServerOptions(false);
                /*
                研究说明：后期可以通过以下方式获取配置信息： 
                1、builder.ApplicationServices.GetService<IOptions<KestrelServerOptions>>();
                2、基本信息可以通过配置信息即时生效
                3、MaxConcurrentConnections 并发属性无法通过修改配置即时生效，因为Kestrel内部搞了个ConnectionLimitMiddleware，并以值的方式传递了该属性。
                 */
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


            });
            services.Configure<HostFilteringOptions>((y) =>
            {
                hostFilteringOptions = y;
                LoadHostFilteringOptions(false);
            });
        }

        public static IApplicationBuilder UseTaurusMvc(this IApplicationBuilder builder)
        {
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

    public static partial class TaurusExtensions
    {
        private static List<string> GetAllowedHostsList(string allowedHosts)
        {
            string[] hosts = allowedHosts.Split(',');
            List<string> newHosts = new List<string>();
            newHosts.AddRange(hosts);
            return newHosts;
        }
        /// <summary>
        /// 动态修改 Kestrel 配置相关属性。
        /// </summary>
        internal static void RefleshOptions()
        {
            LoadKestrelServerOptions(true);
            LoadHostFilteringOptions(true);
        }

        private static void LoadKestrelServerOptions(bool isReflesh)
        {
            if (kestrelServerOptions == null) { return; }
            var x = kestrelServerOptions;
            x.AddServerHeader = MvcConfig.Kestrel.AddServerHeader;
            x.AllowSynchronousIO = MvcConfig.Kestrel.AllowSynchronousIO;

            #region Limits 配置设置
            x.Limits.MaxRequestLineSize = MvcConfig.Kestrel.Limits.MaxRequestLineSize;
            x.Limits.MaxRequestHeaderCount = MvcConfig.Kestrel.Limits.MaxRequestHeaderCount;
            x.Limits.MaxRequestHeadersTotalSize = MvcConfig.Kestrel.Limits.MaxRequestHeadersTotalSize;

            if (isReflesh) { return; }

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

            //为整个应用设置并发打开的最大 TCP 连接数,默认情况下，最大连接数不受限制 (NULL)
            if (MvcConfig.Kestrel.Limits.MaxConcurrentConnections != long.MaxValue)
            {
                x.Limits.MaxConcurrentConnections = MvcConfig.Kestrel.Limits.MaxConcurrentConnections;
            }
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
        }

        private static void LoadHostFilteringOptions(bool isReflesh)
        {
            if (hostFilteringOptions == null) { return; }
            var y = hostFilteringOptions;
            y.AllowEmptyHosts = MvcConfig.Kestrel.AllowEmptyHosts;
            y.IncludeFailureMessage = MvcConfig.Kestrel.IncludeFailureMessage;
            //HostFilteringOptions 无法动态修改生效。
            if (isReflesh)
            {
                return;
            }
            y.AllowedHosts = GetAllowedHostsList(MvcConfig.Kestrel.AllowedHosts);
        }

    }
}
