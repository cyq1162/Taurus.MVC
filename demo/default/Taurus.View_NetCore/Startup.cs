﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Taurus.MicroService;

namespace Taurus.View
{
    public class Startup
    {
        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            // Taurus
            services.AddDistributedMemoryCache();//支持Session的必要组件
            services.AddSession();
            services.AddHttpContext();
            services.Configure<KestrelServerOptions>((x) =>
            {
                if (MsConfig.IsGateway)
                {
                   // x.Listen(IPAddress.Any, 80);
                    x.Listen(IPAddress.Any, 443, op =>
                    {
                        op.UseHttps(opx =>
                        {
                            var certificates = MsConfig.SslCertificate;
                            opx.ServerCertificateSelector = (connectionContext, name) =>
                               name != null && certificates.TryGetValue(name, out var cert) ? cert : certificates["localhost"];

                        });


                    });
                }
                x.AllowSynchronousIO = true;
                //为整个应用设置并发打开的最大 TCP 连接数,默认情况下，最大连接数不受限制 (NULL)
                // x.Limits.MaxConcurrentConnections = 100000;
                //对于已从 HTTP 或 HTTPS 升级到另一个协议（例如，Websocket 请求）的连接，有一个单独的限制。 连接升级后，不会计入 MaxConcurrentConnections 限制
                // x.Limits.MaxConcurrentUpgradedConnections = 100000;
                //获取或设置保持活动状态超时。 默认值为 2 分钟。
                // x.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(100);
                //  serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
            });//.Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
            services.Configure<IISServerOptions>((x) =>
            {
                x.AllowSynchronousIO = true;
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)//把IHostingEnvironment IWebHostEnvironment
        {
            //MicroService.MsConfig.MsTableName = "MsHostList" + DateTime.Now.ToString("yyyymmdd");
            app.UseWebSockets();
            app.UseStaticFiles();
            app.UseSession();
            app.UseHttpContext();
            app.UseTaurusMvc(env);
            //app.Run(async (context) =>
            //{
            //    await context.Response.WriteAsync("Hello World!");
            //});
        }
    }
}
