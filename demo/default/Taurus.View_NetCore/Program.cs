using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using CYQ.Data;
using System.Net.Sockets;
using System.Net;
using Taurus.Mvc;
using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using CYQ.Data.Tool;
using System.Collections.Generic;

namespace Taurus.View
{
    public class Program
    {
        public static void Main(string[] args)
        {
            try
            {
                BuildWebHost(args).Run();
            }
            catch (Exception err)
            {
                Console.WriteLine(err.Message);
                Console.Read();
            }
        }

        public static IWebHost BuildWebHost(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseStartup<Startup>()
                .UseUrls(GetUrl())
                .Build();
        public static string GetUrl()
        {
            string host = AppConfig.GetApp("Host");
            string runUrl = MicroService.MsConfig.AppRunUrl;
            if (host.Contains(":0"))//常规部署随机端口
            {
                TcpListener tl = new TcpListener(IPAddress.Any, 0);
                tl.Start();
                int port = ((IPEndPoint)tl.LocalEndpoint).Port;//获取随机可用端口
                tl.Stop();
                host = host.Replace(":0", ":" + port);
                if (runUrl.Contains(":0"))
                {
                    runUrl = runUrl.Replace(":0", ":" + port);//设置启动路径
                }
                if (runUrl.Contains("localhost") || runUrl.Contains("*"))
                {
                    System.Net.IPAddress[] addressList = Dns.GetHostEntry(Dns.GetHostName()).AddressList;
                    foreach (var address in addressList)
                    {
                        string ip = address.ToString();
                        if (ip.EndsWith(".1") || ip.Contains(":")) // 忽略路由和网卡地址。
                        {
                            continue;
                        }
                        runUrl = runUrl.Replace("localhost", ip).Replace("*", ip);//设置启动路径
                        break;
                    }

                }
                MicroService.MsConfig.AppRunUrl = runUrl;

            }
            else
            {
                // Docker部署：设置映射后的地址
                //判断是否Docker部署，通过环境变量传递当前运行地址，或端口：
                string dockerUrl = Environment.GetEnvironmentVariable("DockerUrl");//跨服务器配置完整路径：http://host:port
                if (!string.IsNullOrEmpty(dockerUrl))
                {
                    MicroService.MsConfig.AppRunUrl = dockerUrl;
                }
                else
                {
                    string dockerHost = Environment.GetEnvironmentVariable("DockerHost");//本机服务器IP，仅配置端口即可。
                    string dockerPort = Environment.GetEnvironmentVariable("DockerPort");//本机服务器，仅配置端口即可。

                    if (!string.IsNullOrEmpty(dockerHost) || !string.IsNullOrEmpty(dockerPort))
                    {
                        string http = "http";
                        if (!string.IsNullOrEmpty(runUrl))
                        {
                            Uri uri = new Uri(runUrl);
                            http = uri.Scheme;
                            if (string.IsNullOrEmpty(dockerHost))
                            {
                                dockerHost = uri.Host;
                            }
                            if (string.IsNullOrEmpty(dockerPort))
                            {
                                dockerPort = uri.Port.ToString();
                            }
                        }
                        if (string.IsNullOrEmpty(dockerHost))
                        {
                            dockerHost = "localhost";
                        }
                        if (string.IsNullOrEmpty(dockerHost))
                        {
                            dockerHost = "80";
                        }
                        MicroService.MsConfig.AppRunUrl = http + "://" + dockerHost + ":" + dockerPort;
                    }
                }
            }
            //string url = AppConfig.GetApp("Host", host);//"[http|https]://*:8888"
            //Console.WriteLine(host);
            return host;
        }
    }
}
