using CYQ.Data;
using System;
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
        private readonly RequestDelegate next;

        public TaurusMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (!context.Request.Host.HasValue)
                {
                    context.Response.StatusCode = 400;
                    await context.Response.WriteAsync("400 Invalid hostname.");
                    return;
                }
                if (context.Request.Path.Value.IndexOf("/App_Data/", StringComparison.OrdinalIgnoreCase) > -1)//兼容受保护的目录
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("403 Forbidden");
                    return;
                }

                if (context.Request.Method != "GET" && context.Request.ContentLength.HasValue && context.Request.ContentLength.Value > 0)
                {
                    // 使用处：对应Rpc.Gateway.cs 代码：Proxy 方法 149行上下。
                    //Controller.cs GetJson 方法 1098行上下
                    context.Request.EnableBuffering();
                }
                System.Web.HttpApplication.Instance.ExecuteEventHandler();
                if (System.Web.HttpContext.Current.Response.HasStarted)  // || Body是只写流  (context.Response.Body != null && context.Response.Body.CanRead
                {
                    if (context.Response.StatusCode == 204)
                    {
                        await context.Response.Body.FlushAsync();
                    }
                    else
                    {
                        await context.Response.WriteAsync("");
                    }
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
