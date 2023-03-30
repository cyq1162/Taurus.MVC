using CYQ.Data;
using CYQ.Data.Tool;
using Microsoft.AspNetCore.Builder;
using System;
using System.Threading.Tasks;
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
                string tip;
                if (!LimitRun.CheckRequestIsSafe(context, out tip))
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync(JsonHelper.OutResult(false, tip));
                }
                else
                {
                    await next(context);
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
            return builder.UseMiddleware<TaurusLimitMiddleware>();
        }
    }
}
