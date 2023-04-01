using CYQ.Data;
using CYQ.Data.Tool;
using Microsoft.AspNetCore.Builder;
using System;
using System.Threading.Tasks;
using Taurus.Core;
using Taurus.Mvc;
using Taurus.Plugin.Limit;

namespace Microsoft.AspNetCore.Http
{
    /// <summary>
    /// 为支持Asp.net core 存在的文件
    /// </summary>
    internal class TaurusLimitMiddleware
    {
        private readonly RequestDelegate next;

        public TaurusLimitMiddleware(RequestDelegate next)
        {
            this.next = next;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                if (context.Request.Path.Value.IndexOf("/App_Data/", StringComparison.OrdinalIgnoreCase) > -1)//兼容受保护的目录
                {
                    context.Response.StatusCode = 403;
                    await context.Response.WriteAsync("403 Forbidden");
                }
                else
                {
                    System.Web.HttpApplication.Instance.ExecuteEventHandler();
                    if (System.Web.HttpContext.Current.Response.HasStarted)
                    {
                        context.Response.StatusCode = 401;
                        await context.Response.WriteAsync("");
                    }
                    //处理信息
                    else
                    {
                        await next(context);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.WriteLogToTxt(ex);
            }
        }
    }
    public static class TaurusLimitExtensions
    {
        /// <summary>
        /// 使用Ack限制中间件：服务端进行安全和防重复提交较验（客户端请求头需要带ack标识），本功能一般针对网关使用；
        /// 本功能在使用：app.UseTaurusMvc(env);之前调用。
        /// </summary>
        /// <returns></returns>
        public static IApplicationBuilder UseTaurusAckLimit(this IApplicationBuilder builder)
        {
            UrlLimit limit = new UrlLimit();
            limit.Init(System.Web.HttpApplication.Instance);
            return builder.UseMiddleware<TaurusLimitMiddleware>();
        }
    }
}
