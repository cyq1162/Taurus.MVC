﻿using CYQ.Data;
using CYQ.Data.Tool;
using System;
using System.Reflection;
using System.Threading;
using System.Web;
using Taurus.MicroService;
using Taurus.Mvc;

namespace Taurus.Core
{
    /// <summary>
    /// 权限检测模块（NetCore 下处理成单例模式）
    /// </summary>
    internal class UrlRewrite : IHttpModule
    {
        public void Dispose()
        {

        }
        public void Init(HttpApplication context)
        {
            context.BeginRequest += context_BeginRequest;
            context.PostMapRequestHandler += context_PostMapRequestHandler;
            context.AcquireRequestState += context_AcquireRequestState;
            context.Error += context_Error;
            context.Disposed += context_Disposed;
        }

        void context_Disposed(object sender, EventArgs e)
        {
//#if DEBUG
//            ThreadBreak.ClearGlobalThread();
//            System.Diagnostics.Debug.WriteLine("应用程序退出：HttpApplication Disposed。");
//            System.Console.WriteLine("应用程序退出：HttpApplication Disposed!");
//#endif

        }
        void context_BeginRequest(object sender, EventArgs e)
        {
            HttpApplication app = (HttpApplication)sender;
            HttpContext context = app.Context;
            Uri uri = context.Request.Url;
            #region 微服务检测与启动
            string urlAbs = uri.AbsoluteUri;
            string urlPath = uri.PathAndQuery;
            string host = urlAbs.Substring(0, urlAbs.Length - urlPath.Length);
            MicroService.MSRun.Start(host);//微服务检测、启动。
            if (!WebTool.IsCallMicroServiceReg(uri) && Rpc.Gateway.Proxy(context, true))
            {
                WebTool.SetRunProxySuccess(context);
                try
                {
                    context.Response.End();
                }
                catch (ThreadAbortException)
                {

                }

                return;
            }
            #endregion

            if (WebTool.IsCallMvc(uri))
            {
                if (context.Request.Url.LocalPath == "/")//设置默认首页
                {
                    string defaultUrl = MvcConfig.DefaultUrl;
                    if (!string.IsNullOrEmpty(defaultUrl))
                    {
                        context.RewritePath(defaultUrl);
                        return;
                    }
                }
                if (WebTool.IsTaurusSuffix(uri))
                {
                    MethodEntity routeMapInvoke = MethodCollector.GlobalRouteMapInvoke;
                    if (routeMapInvoke != null)
                    {
                        string url = Convert.ToString(routeMapInvoke.Method.Invoke(null, new object[] { context.Request }));
                        if (!string.IsNullOrEmpty(url))
                        {
                            context.RewritePath(url);
                        }
                    }
                }
            }
        }

        void context_PostMapRequestHandler(object sender, EventArgs e)
        {
            HttpContext cont = ((HttpApplication)sender).Context;
            if (cont != null && WebTool.IsCallMvc(cont.Request.Url) && !WebTool.IsProxyCall(cont) && WebTool.IsTaurusSuffix(cont.Request.Url))
            {
                cont.Handler = SessionHandler.Instance;//注册Session
            }
        }
        void context_AcquireRequestState(object sender, EventArgs e)
        {
            HttpContext cont = ((HttpApplication)sender).Context;
            if (cont != null && WebTool.IsCallMvc(cont.Request.Url) && !WebTool.IsProxyCall(cont) && WebTool.IsTaurusSuffix(cont.Request.Url))
            {
                CheckCORS(cont);
                ReplaceOutput(cont);
                InvokeClass(cont);
            }
        }

        void context_Error(object sender, EventArgs e)
        {
            HttpContext cont = ((HttpApplication)sender).Context;
            if (cont != null && WebTool.IsCallMvc(cont.Request.Url) && WebTool.IsTaurusSuffix(cont.Request.Url))
            {
                Log.WriteLogToTxt(cont.Error);
            }
        }

        #region 检测CORS跨域请求
        private void CheckCORS(HttpContext context)
        {
            if (MvcConfig.IsAllowCORS)
            {
                if (context.Request.HttpMethod == "OPTIONS")
                {
                    context.Response.StatusCode = 204;
                    context.Response.AppendHeader("Access-Control-Allow-Method", "GET,POST,PUT,DELETE");
                    context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                    if (context.Request.Headers["Access-Control-Allow-Headers"] != null)
                    {
                        context.Response.AppendHeader("Access-Control-Allow-Headers", context.Request.Headers["Access-Control-Allow-Headers"]);
                    }
                    else if (context.Request.Headers["Access-Control-Request-Headers"] != null)
                    {
                        context.Response.AppendHeader("Access-Control-Allow-Headers", context.Request.Headers["Access-Control-Request-Headers"]);
                    }
                    context.Response.End();
                }
                else if (context.Request.UrlReferrer == null || context.Request.Url.Authority != context.Request.UrlReferrer.Authority)
                {
                    //跨域访问
                    context.Response.AppendHeader("Access-Control-Allow-Origin", "*");
                    context.Response.AppendHeader("Access-Control-Allow-Credentials", "true");
                }
            }
        }

        #endregion


        #region 替换输出，仅对子目录部署时有效
        void ReplaceOutput(HttpContext context)
        {
            if (WebTool.IsSubAppSite(context.Request.Url))
            {
                //如果项目需要部署成子应用程序，则开启，否则不需要开启（可注释掉下面一行代码）
                context.Response.Filter = new HttpResponseFilter(context.Response.Filter);
            }
        }
        #endregion

        #region 逻辑反射调用Controlls的方法
        private void InvokeClass(HttpContext context)
        {
            Type t = null;
            //ViewController是由页面的前两个路径决定了。
            string[] items = WebTool.GetLocalPath(context.Request.Url).Trim('/').Split('/');
            string className = ReflectConst.Default;
            if (MvcConfig.RouteMode == 1)
            {
                className = items.Length > 2 ? items[0] + "." + items[1] : items[0];
            }
            else if (MvcConfig.RouteMode == 2)
            {
                className = items.Length > 1 ? items[0] + "." + items[1] : items[0];
            }
            t = ControllerCollector.GetController(className);
            if (t == null || t.Name == ReflectConst.DefaultController)
            {
                if (Rpc.Gateway.Proxy(context, false))//客户端做为网关。
                {
                    return;
                }
            }
            if (t == null)
            {
                WriteError("You need a " + className + " controller for coding!", context);
            }
            else
            {
                try
                {
                    Controller o = (Controller)Activator.CreateInstance(t);//实例化
                    o.ProcessRequest(context);
                }

                catch (ThreadAbortException e)
                {
                    //内部提前Response.End()时引发的异常
                    //ASP.NET 的机制就是通过异常退出线程（不要觉的奇怪）
                }
                catch (Exception err)
                {
                    WriteError(err.Message, context);
                }
            }
            //context.Response.End();
        }
        private void WriteError(string tip, HttpContext context)
        {
            context.Response.Write(JsonHelper.OutResult(false, tip));
        }
        #endregion


    }

}