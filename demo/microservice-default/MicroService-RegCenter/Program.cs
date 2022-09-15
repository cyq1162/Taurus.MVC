using CYQ.Data;
using Microsoft.AspNetCore.Server.Kestrel.Core;

try
{
    var builder = WebApplication.CreateBuilder(args);
    string host = AppConfig.GetApp("Host");

    builder.WebHost.UseUrls(host);
    builder.Services.AddHttpContext();
    builder.Services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true);

    var app = builder.Build();
    app.UseHttpContext();
    app.UseTaurusMvc(app.Environment);
    app.Run();
}
catch (Exception err)
{
    Console.WriteLine(err.Message);
    
}