using CYQ.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using System.Net;
using Taurus.MicroService;
using Taurus.Mvc;

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
            // services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "DataProtection"));
            services.AddHttpContext();
            services.Configure<FormOptions>(options => options.MultipartBodyLengthLimit = long.MaxValue);
            services.Configure<KestrelServerOptions>((x) =>
            {
                if (MsConfig.IsServer && MvcConfig.SslCertificate.Count > 0)
                {
                    //if (MsConfig.IsRegCenter)
                    //{
                        string url = !string.IsNullOrEmpty(MvcConfig.RunUrl) ? MvcConfig.RunUrl : AppConfig.GetApp("Host");
                        if (!string.IsNullOrEmpty(url) && !url.StartsWith("https"))
                        {
                            string[] items = url.Split(":");
                            if (items.Length == 2)
                            {
                                x.Listen(IPAddress.Any, 80);
                            }
                            else if (items.Length == 3)
                            {
                                x.Listen(IPAddress.Any, int.Parse(items[2]));
                            }
                        }
                    //}
                    //x.Listen(IPAddress.Any, 9999);
                    x.Listen(IPAddress.Any, 443, op =>
                    {

                        op.UseHttps(opx =>
                        {
                            var certificates = MvcConfig.SslCertificate;
                            opx.ServerCertificateSelector = (connectionContext, name) =>
                               name != null && certificates.TryGetValue(name, out var cert) ? cert : certificates["localhost"];

                        });


                    });
                }
                x.AllowSynchronousIO = true;
                x.Limits.MaxRequestBufferSize = null;
                x.Limits.MaxRequestBodySize = null;

                //为整个应用设置并发打开的最大 TCP 连接数,默认情况下，最大连接数不受限制 (NULL)
                // x.Limits.MaxConcurrentConnections = 100000;
                //对于已从 HTTP 或 HTTPS 升级到另一个协议（例如，Websocket 请求）的连接，有一个单独的限制。 连接升级后，不会计入 MaxConcurrentConnections 限制
                // x.Limits.MaxConcurrentUpgradedConnections = 100000;
                //获取或设置保持活动状态超时。 默认值为 2 分钟。
                // x.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(100);
                //  serverOptions.Limits.RequestHeadersTimeout = TimeSpan.FromMinutes(1);
            });//.Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
            //services.Configure<IISServerOptions>((x) =>
            //{
            //    x.AllowSynchronousIO = true;
            //});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)//把IHostingEnvironment IWebHostEnvironment
        {
            app.UseWebSockets();
            app.UseSession();
            app.UseHttpContext();
            app.UseTaurusMvc(env);
            app.UseStaticFiles();//做为注册中心服务时，静态文件功能应该放后面。

        }
    }
}
