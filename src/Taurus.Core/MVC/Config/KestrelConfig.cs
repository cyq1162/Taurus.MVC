using CYQ.Data;
using System.Net.Sockets;
using System.Net;
using Taurus.Plugin.Admin;
using CYQ.Data.Tool;
using System.IO;
using System.Security.Cryptography.X509Certificates;
using System;

namespace Taurus.Mvc
{
    /// <summary>
    /// Taurus.Mvc Config
    /// </summary>
    public static partial class MvcConfig
    {
        /// <summary>
        /// Kestrel 配置
        /// </summary>
        public static class Kestrel
        {
            static Kestrel()
            {
                //需要还原数据
                AdminConfig.Init();
            }
            /// <summary>
            /// 应用配置：当前 Web 允许访问主机头【Kestrel端限制】
            /// </summary>
            public static string AllowedHosts
            {
                get
                {
                    string host = AppConfig.GetApp("AllowedHosts", "*");
                    return host.TrimEnd('/');
                }
            }
            /// <summary>
            /// 应用配置：当前Web监听主机【Kestrel启动运行时绑定】
            /// </summary>
            public static string Urls
            {
                get
                {
                    string host = AppConfig.GetApp("Kestrel.Urls,Urls", "");
                    if (host.Contains(":0"))//常规部署随机端口
                    {
                        TcpListener tl = new TcpListener(IPAddress.Any, 0);
                        tl.Start();
                        int port = ((IPEndPoint)tl.LocalEndpoint).Port;//获取随机可用端口
                        tl.Stop();
                        host = host.Replace(":0", ":" + port);
                        AppConfig.SetApp("Kestrel.Urls", host);
                    }
                    return host.TrimEnd('/');
                }
                set
                {
                    AppConfig.SetApp("Kestrel.Urls", value);
                }
            }

            /// <summary>
            /// 应用配置：是否允许同步IO读取请求的流数据。
            /// </summary>
            public static bool AllowSynchronousIO
            {
                get
                {
                    return AppConfig.GetAppBool("Kestrel.AllowSynchronousIO", true);
                }
                set
                {
                    AppConfig.SetApp("Kestrel.AllowSynchronousIO", value.ToString());
                }
            }
            /// <summary>
            /// 应用配置：Https 证书 存放路径【客户端默认开启、服务端默认关闭】
            /// </summary>
            public static string SslPath
            {
                get
                {
                    return AppConfig.GetApp("Taurus.SslPath", "/App_Data/ssl");
                }
                set
                {
                    AppConfig.SetApp("Taurus.SslPath", value);
                }
            }
            public static MDictionary<string, X509Certificate2> _SslCertificate = new MDictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);
            /// <summary>
            /// 获取应用证书【证书路径由SslPath配置】（只读）
            /// </summary>
            public static MDictionary<string, X509Certificate2> SslCertificate
            {
                get
                {
                    string sslFolder = AppConfig.WebRootPath + SslPath.TrimStart('/', '\\');
                    if (Directory.Exists(sslFolder))
                    {
                        string[] files = Directory.GetFiles(sslFolder, "*.pfx", SearchOption.TopDirectoryOnly);
                        foreach (string file in files)
                        {
                            string pwdPath = file.Replace(".pfx", ".txt");
                            if (File.Exists(pwdPath))
                            {
                                string pwd = IOHelper.ReadAllText(pwdPath);
                                string domain = Path.GetFileName(pwdPath).Replace(".txt", "");
                                if (!_SslCertificate.ContainsKey(domain))
                                {
                                    _SslCertificate.Add(domain, new X509Certificate2(file, pwd));//实例化比较耗时，避开重复实例化，兼顾缓存更新。
                                }
                            }
                        }
                    }

                    return _SslCertificate;
                }
            }

            public static class Limits
            {
                /// <summary>
                ///    Gets or sets the maximum size of the response buffer before write calls begin
                ///    to block or return tasks that don't complete until the buffer size drops below
                ///    the configured limit. Defaults to 65,536 bytes (64 KB).
                ///    When set to null, the size of the response buffer is unlimited. When set to zero,
                ///    all write calls will block or return tasks that don't complete until the entire
                ///    response buffer is flushed.
                /// </summary>
                public static long MaxResponseBufferSize
                {
                    get
                    {
                        return AppConfig.GetApp<long>("Kestrel.Limits.MaxResponseBufferSize", long.MaxValue);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.MaxResponseBufferSize", value.ToString());
                    }
                }
                /// <summary>
                /// Gets or sets the maximum size of the request buffer. Defaults to 1,048,576 bytes (1 MB).
                /// When set to null, the size of the request buffer is unlimited.
                /// </summary>
                public static long MaxRequestBufferSize
                {
                    get
                    {
                        return AppConfig.GetApp<long>("Kestrel.Limits.MaxRequestBufferSize", long.MaxValue);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.MaxRequestBufferSize", value.ToString());
                    }
                }

                /// <summary>
                /// Gets or sets the maximum allowed size for the HTTP request line. Defaults to 8,192 bytes (8 KB).
                /// For HTTP/2 this measures the total size of the required pseudo headers :method,:scheme, :authority, and :path.
                /// </summary>
                public static int MaxRequestLineSize
                {
                    get
                    {
                        return AppConfig.GetAppInt("Kestrel.Limits.MaxRequestLineSize", 8192);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.MaxRequestLineSize", value.ToString());
                    }
                }

                /// <summary>
                /// Gets or sets the maximum allowed size for the HTTP request headers. Defaults to 32,768 bytes (32 KB).
                /// </summary>
                public static int MaxRequestHeadersTotalSize
                {
                    get
                    {
                        return AppConfig.GetAppInt("Kestrel.Limits.MaxRequestHeadersTotalSize", 32768);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.MaxRequestHeadersTotalSize", value.ToString());
                    }
                }

                /// <summary>
                /// Gets or sets the maximum allowed number of headers per HTTP request. Defaults to 100.
                /// </summary>
                public static int MaxRequestHeaderCount
                {
                    get
                    {
                        return AppConfig.GetAppInt("Kestrel.Limits.MaxRequestHeaderCount", 100);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.MaxRequestHeaderCount", value.ToString());
                    }
                }


                /// <summary>
                ///     Gets or sets the maximum allowed size of any request body in bytes. When set
                ///     to null, the maximum request body size is unlimited. This limit has no effect
                ///     on upgraded connections which are always unlimited. This can be overridden per-request
                ///     via Microsoft.AspNetCore.Http.Features.IHttpMaxRequestBodySizeFeature. Defaults
                ///     to 30,000,000 bytes, which is approximately 28.6MB.
                /// </summary>
                public static long MaxRequestBodySize
                {
                    get
                    {
                        return AppConfig.GetApp<long>("Kestrel.Limits.MaxRequestBodySize", long.MaxValue);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.MaxRequestBodySize", value.ToString());
                    }
                }

                /// <summary>
                /// Gets or sets the keep-alive timeout. Defaults to 2 minutes.
                /// </summary>
                public static int KeepAliveTimeout
                {
                    get
                    {
                        return AppConfig.GetAppInt("Kestrel.Limits.KeepAliveTimeout", 2);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.KeepAliveTimeout", value.ToString());
                    }
                }

                /// <summary>
                /// Gets or sets the maximum amount of time the server will spend receiving request headers. Defaults to 30 seconds.
                /// </summary>
                public static int RequestHeadersTimeout
                {
                    get
                    {
                        return AppConfig.GetAppInt("Kestrel.Limits.RequestHeadersTimeout", 30);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.RequestHeadersTimeout", value.ToString());
                    }
                }

                /// <summary>
                /// Gets or sets the maximum number of open connections. When set to null, the number  of connections is unlimited.
                /// Defaults to null.
                /// When a connection is upgraded to another protocol, such as WebSockets, its connection 
                /// is counted against the Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerLimits.MaxConcurrentUpgradedConnections
                /// limit instead of Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerLimits.MaxConcurrentConnections.
                /// </summary>
                public static long MaxConcurrentConnections
                {
                    get
                    {
                        return AppConfig.GetApp<long>("Kestrel.Limits.MaxConcurrentConnections", long.MaxValue);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.MaxConcurrentConnections", value.ToString());
                    }
                }

                /// <summary>
                ///  Gets or sets the maximum number of open, upgraded connections. When set to null,
                ///  the number of upgraded connections is unlimited. An upgraded connection is one
                ///  that has been switched from HTTP to another protocol, such as WebSockets.
                ///  Defaults to null.
                ///  When a connection is upgraded to another protocol, such as WebSockets, its connection
                ///   is counted against the Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerLimits.MaxConcurrentUpgradedConnections
                ///   limit instead of Microsoft.AspNetCore.Server.Kestrel.Core.KestrelServerLimits.MaxConcurrentConnections.
                ///   
                /// </summary>
                public static long MaxConcurrentUpgradedConnections
                {
                    get
                    {
                        return AppConfig.GetApp<long>("Kestrel.Limits.MaxConcurrentUpgradedConnections", long.MaxValue);
                    }
                    set
                    {
                        AppConfig.SetApp("Kestrel.Limits.MaxConcurrentUpgradedConnections", value.ToString());
                    }
                }
            }
        }
    }
}
