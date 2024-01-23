using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Taurus.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(MvcConfig.Kestrel.Urls);
builder.Services.AddTaurusMvc();

var app = builder.Build();
app.UseStaticFiles();
app.UseTaurusMvc();

app.Run();
