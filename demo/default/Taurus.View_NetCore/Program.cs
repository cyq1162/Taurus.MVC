using System;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using CYQ.Data;
using System.Net.Sockets;
using System.Net;
using Microsoft.AspNetCore.Builder;
using Taurus.MicroService;
using Taurus.Mvc;

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
            string runUrl = MsConfig.AppRunUrl;
            if (host.Contains(":0"))//常规部署随机端口
            {
                TcpListener tl = new TcpListener(IPAddress.Any, 0);
                tl.Start();
                int port = ((IPEndPoint)tl.LocalEndpoint).Port;//获取随机可用端口
                tl.Stop();
                host = host.Replace(":0", ":" + port);
                string ip = MvcConst.HostIP;
                if (!string.IsNullOrEmpty(runUrl))
                {
                    if (runUrl.Contains(":0"))
                    {
                        runUrl = runUrl.Replace(":0", ":" + port);//设置启动路径
                    }
                    if (runUrl.Contains("localhost") || runUrl.Contains("*"))
                    {

                        runUrl = runUrl.Replace("localhost", ip).Replace("*", ip);//设置启动路径
                    }
                    MsConfig.AppRunUrl = runUrl;
                }
                else
                {
                    MsConfig.AppRunUrl = "http://" + ip + ":" + port;
                }
            }
            else
            {
                // Docker部署：设置映射后的地址
                //判断是否Docker部署，通过环境变量传递当前运行地址，或端口：
                string dockerUrl = Environment.GetEnvironmentVariable("DockerUrl");//跨服务器配置完整路径：http://host:port
                if (!string.IsNullOrEmpty(dockerUrl))
                {
                    MsConfig.AppRunUrl = dockerUrl;
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
                        MsConfig.AppRunUrl = http + "://" + dockerHost + ":" + dockerPort;
                    }
                }
            }
            //string url = AppConfig.GetApp("Host", host);//"[http|https]://*:8888"
            //Console.WriteLine(host);
            return host;
        }
    }
}
