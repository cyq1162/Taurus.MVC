using CYQ.Data;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Web;
using System.Web.SessionState;

namespace Taurus.Core
{
    /// <summary>
    /// 权限检测模块
    /// </summary>
    public class UrlRewrite : IHttpModule, IRequiresSessionState
    {
        public void Dispose()
        {

        }
        public void Init(HttpApplication context)
        {
            context.BeginRequest += new EventHandler(context_BeginRequest);
            context.Error += context_Error;
        }

        void context_Error(object sender, EventArgs e)
        {
            if (QueryTool.IsTaurusSuffix())
            {
                Log.WriteLogToTxt(HttpContext.Current.Error);
            }
        }

        HttpContext context;
        void context_BeginRequest(object sender, EventArgs e)
        {
            if (QueryTool.IsTaurusSuffix())
            {
                HttpApplication app = (HttpApplication)sender;
                context = app.Context;
                CheckCORS();
                ReplaceOutput();
                InvokeClass();
            }
        }

        #region 检测CORS跨域请求
        private void CheckCORS()
        {
            if (QueryTool.IsAllowCORS())
            {
                if (context.Request.HttpMethod == "OPTIONS")
                {
                    context.Response.StatusCode = 204;
                    context.Response.AppendHeader("Access-Control-Allow-Method", "GET,POST,PUT,DELETE");
                    context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                    context.Response.AppendHeader("Access-Control-Allow-Headers", context.Request.Headers["Access-Control-Allow-Headers"]);
                    context.Response.End();
                }
                else if (context.Request.UrlReferrer != null && context.Request.Url.Host != context.Request.UrlReferrer.Host)
                {
                    //跨域访问
                    context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                    context.Response.AppendHeader("Access-Control-Allow-Credentials", "true");
                }
            }
        }

        #endregion


        #region 替换输出，仅对子目录部署时有效
        void ReplaceOutput()
        {
            if (QueryTool.IsUseUISite)
            {
                //如果项目需要部署成子应用程序，则开启，否则不需要开启（可注释掉下面一行代码）
                context.Response.Filter = new HttpResponseFilter(context.Response.Filter);
            }
        }
        #endregion

        #region 逻辑反射调用Controlls的方法
        private void InvokeClass()
        {
            Type t = null;
            //ViewController是由页面的前两个路径决定了。
            string[] items = QueryTool.GetLocalPath().Trim('/').Split('/');
            string className = InvokeLogic.Default;
            if (RouteConfig.RouteMode == 1)
            {
                className = items[0];
            }
            else if (RouteConfig.RouteMode == 2)
            {
                className = items.Length > 1 ? items[1] : "";
            }
            t = InvokeLogic.GetType(className);
            if (t == null)
            {
                WriteError("You need a controller for coding!");
            }
            try
            {
                object o = Activator.CreateInstance(t);//实例化
                t.GetMethod("ProcessRequest").Invoke(o, new object[] { context });
            }
            catch (ThreadAbortException e)
            {
                //ASP.NET 的机制就是通过异常退出线程（不要觉的奇怪）
            }
            catch (Exception err)
            {
                WriteError(err.Message);
            }
        }
        private void WriteError(string tip)
        {
            context.Response.Write(JsonHelper.OutResult(false, tip));
            context.Response.End();
        }
        #endregion


    }
}
