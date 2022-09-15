using CYQ.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using System.Net;
using System.Net.Sockets;
using Taurus.MicroService;

var builder = WebApplication.CreateBuilder(args);
string host = AppConfig.GetApp("Host");
string runUrl = MSConfig.AppRunUrl;
if (host.Contains(":0"))//����˿�
{
    TcpListener tl = new TcpListener(IPAddress.Any, 0);
    tl.Start();
    int port = ((IPEndPoint)tl.LocalEndpoint).Port;//��ȡ������ö˿�
    tl.Stop();
    host = host.Replace(":0", ":" + port);
    if (runUrl.Contains(":0"))
    {
        MSConfig.AppRunUrl = runUrl.Replace(":0", ":" + port);//��������·��
    }
}

builder.WebHost.UseUrls(host);
builder.Services.AddHttpContext();
builder.Services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true);

var app = builder.Build();
app.UseHttpContext();
app.UseTaurusMvc(app.Environment);
app.Run();
