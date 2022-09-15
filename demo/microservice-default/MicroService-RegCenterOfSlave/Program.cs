using CYQ.Data;

try
{
    var builder = WebApplication.CreateBuilder(args);
    string host = AppConfig.GetApp("Host");

    builder.WebHost.UseUrls(host);
    builder.Services.AddHttpContext();

    var app = builder.Build();
    app.UseHttpContext();
    app.UseTaurusMvc(app.Environment);
    app.Run();
}
catch (Exception err)
{
    Console.WriteLine(err.Message);

}