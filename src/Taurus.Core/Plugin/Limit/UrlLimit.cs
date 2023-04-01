using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Taurus.Plugin.Limit
{
    internal class UrlLimit : IHttpModule
    {
        public void Dispose()
        {

        }

        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
        }

        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            HttpContext context = app.Context;
            string tip;
            if (!LimitRun.CheckRequestIsSafe(context.Request.Url.LocalPath, out tip))
            {
                context.Response.StatusCode = 401;
                context.Response.Write(JsonHelper.OutResult(false, tip));
                context.Response.End();
            }
        }
    }
}
