using Taurus.Mvc;

var builder = WebApplication.CreateBuilder(args);
builder.WebHost.UseUrls(MvcConfig.Kestrel.Urls);
builder.Services.AddTaurusMvc();

var app = builder.Build();
app.UseTaurusMvc();
app.UseStaticFiles();
app.Run();
