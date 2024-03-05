using CYQ.Data;
using System;
using System.Web;
using System.Diagnostics;
using System.Threading.Tasks;
using Taurus.Mvc;
using Taurus.Plugin.MicroService;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// 为支持Asp.net core 存在的文件
    /// </summary>
    internal class TaurusMiddleware
    {
        private static HttpApplication app;
        private readonly RequestDelegate next;

        public TaurusMiddleware(RequestDelegate next)
        {
            this.next = next;
            app = HttpApplication.GetInstance("Taurus");
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                // 遍历 HttpContext.Features
                //foreach (var feature in context.Features)
                //{
                //    // 输出特性类型和实例信息
                //    Console.WriteLine($"Feature type: {feature.Key}, instance: {feature.Value}");
                //}
                //await context.Response.Body.FlushAsync();
                //return;

                var request = context.Request;
                var response = context.Response;
                if (!request.Host.HasValue)
                {
                    response.StatusCode = 400;
                    await response.WriteAsync("400 Invalid hostname.");
                    return;
                }
                if (request.Path.Value.IndexOf("/App_Data/", StringComparison.OrdinalIgnoreCase) > -1)//兼容受保护的目录
                {
                    response.StatusCode = 403;
                    await response.WriteAsync("403 Forbidden");
                    return;
                }

                //if (request.HasFormContentType && request.ContentLength.HasValue && request.ContentLength.Value > 0)
                //{
                //    // 使用处：对应Rpc.Gateway.cs 代码：Proxy 方法 149行上下。
                //    //Controller.cs GetJson 方法 1098行上下
                //    request.EnableBuffering();
                //}
                
                //Stopwatch sw = Stopwatch.StartNew();
                app.ExecuteEventHandler();
                //sw.Stop();
                //Console.WriteLine("ElapsedTicks Total: "+sw.ElapsedTicks); 
                if (System.Web.HttpContext.Current.Response.HasStarted)  // || Body是只写流  (context.Response.Body != null && context.Response.Body.CanRead
                {
                    await response.Body.FlushAsync();
                    //if (response.StatusCode == 204 || response.StatusCode.ToString().StartsWith("30"))
                    //{
                    //    await response.Body.FlushAsync();
                    //}
                    //else
                    //{
                    //    return;
                    //}
                }
                //处理信息
                else
                {
                    await next(context);
                }

            }
            catch (Exception ex)
            {
                CYQ.Data.Log.WriteLogToTxt(ex, LogType.Taurus);
            }
        }
    }

}
