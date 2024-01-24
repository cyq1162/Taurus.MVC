using CYQ.Data;
using CYQ.Data.Tool;
using System;
using System.Threading;
using System.Web;
using Taurus.Plugin.MicroService;
using Taurus.Mvc;
using Taurus.Plugin.Limit;
using Taurus.Plugin.CORS;
using Taurus.Plugin.Metric;
using Taurus.Mvc.Reflect;
using CYQ.Data.Json;

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
            MvcRun.Start();
            context.BeginRequest += context_BeginRequest;
            context.AcquireRequestState += context_AcquireRequestState;
            context.Error += context_Error;
            if (!AppConfig.IsNetCore)
            {
                context.PostMapRequestHandler += context_PostMapRequestHandler;
#if DEBUG

                context.Disposed += context_Disposed;
#else
                context.PreSendRequestContent += context_PreSendRequestContent;
#endif
            }
        }

        void context_PreSendRequestContent(object sender, EventArgs e)
        {
            HttpContext cont = ((HttpApplication)sender).Context;
            if (cont != null && cont.Request.Url.Host != "localhost")
            {
                cont.Response.Headers.Remove("Server");
                cont.Response.Headers.Remove("X-AspNet-Version");
                cont.Response.Headers.Remove("X-Powered-By");
            }
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

            #region 0、请求头设置
            if (MvcConfig.IsAddTaurusHeader)
            {
                int pid = MvcConst.ProcessID;
                string[] items = MvcConst.HostIP.Split('.');
                string ipNum = items[items.Length - 1];
                context.Response.AppendHeader("Taurus" + "-" + ipNum + "-" + pid, MvcConst.Version);
            }
            #endregion

            #region 1、微服务启动检测
            MsRun.Start(uri);
            #endregion

            #region 主机 IP 地址访问限制

            if (!MvcConfig.IsAllowIPHost)
            {
                string[] items = uri.Authority.Split('.');
                int ip;
                if (items.Length == 4 && int.TryParse(items[3], out ip))
                {
                    context.Response.StatusCode = 400;
                    context.Response.Write("400 - Invalid hostname.");
                    context.Response.End();
                    return;
                }

            }
            #endregion

            #region 2、网关安全限制策略检测 - Admin管理后台和微服务不处理。
            if (!LimitRun.IsIgnoreUrl(uri, context.Request.UrlReferrer))
            {
                if (!LimitRun.CheckIP())
                {
                    //网关请求限制，直接返回
                    context.Response.StatusCode = 403;
                    context.Response.Write("403.6 - Forbidden: IP address rejected.");
                    context.Response.End();
                    return;
                }
                if (WebTool.IsMvcSuffix(uri.LocalPath))
                {
                    if (!LimitRun.CheckRate())
                    {
                        //网关请求限制，直接返回
                        context.Response.StatusCode = 403;
                        context.Response.Write("403.502 - Forbidden: Too many requests, dynamic restriction limit reached.");
                        context.Response.End();
                        return;
                    }
                    if (!LimitRun.CheckAck())
                    {
                        //网关请求限制，直接返回
                        context.Response.StatusCode = 412;
                        context.Response.Write("412 - Precondition failed: Ack is invalid.");
                        context.Response.End();
                        return;
                    }
                }
            }
            #endregion

            #region 3、跨域检测【在网关转发之前】
            if (!CORSRun.Check(context))
            {
                context.Response.End();
                return;
            }
            #endregion



            #region 4、网关代理请求检测与转发【接口调用次数统计】 - 5、纯网关检测 - 6、Mvc模块禁用检测
            bool isPluginUrl = WebTool.IsPluginUrl(uri, context.Request.UrlReferrer);

            if (!isPluginUrl || !MetricConfig.IsIgnorePluginUrl)
            {
                #region 接口调用次数统计，包含统计其它后缀。
                if (MetricConfig.IsEnable)
                {
                    MetricRun.Start(uri);
                }
                #endregion
            }

            if (!isPluginUrl)
            {
                if (WebTool.IsMvcSuffix(uri))
                {
                    #region 打印请求日志
                    if (MvcConfig.IsPrintRequestLog)
                    {
                        WebTool.PrintRequestLog(context, null);
                    }
                    #endregion
                }

                if (MsConfig.Server.IsEnable)
                {
                    #region 4、网关代理请求检测与转发
                    if (Rpc.Gateway.Proxy(context))
                    {
                        context.Response.End();
                        return;
                    }
                    #endregion

                    #region 5、纯网关检测。

                    //单纯网关，直接返回
                    if (MsConfig.IsGateway && !MsConfig.IsClient)
                    {
                        context.Response.StatusCode = 503;
                        context.Response.Write("503 - Service unavailable.");
                        context.Response.End();
                        return;
                    }
                    #endregion
                }
                #region 6、Mvc模块禁用检测
                if (!MvcConfig.IsEnable)
                {
                    context.Response.StatusCode = 503;
                    context.Response.Write("503 - Service unavailable: The mvc module has been disabled");
                    context.Response.End();
                    return;
                }
                #endregion
            }
            #endregion

            #region 7、Mvc模块运行
            if (context.Request.Url.LocalPath == "/")//设置默认首页
            {
                string defaultUrl = MvcConfig.DefaultUrl;
                if (!string.IsNullOrEmpty(defaultUrl))
                {
                    context.RewritePath(defaultUrl);
                    return;
                }
            }
            else
            {
                string mapUrl = RouteEngine.Get(uri.LocalPath);
                if (!string.IsNullOrEmpty(mapUrl))
                {
                    context.Items.Add("Uri", uri);
                    context.RewritePath(mapUrl);
                    return;
                }
            }
            if (WebTool.IsMvcSuffix(uri))
            {
                MethodEntity routeMapInvoke = MethodCollector.GlobalRouteMapInvoke;
                if (routeMapInvoke != null)
                {
                    string url = Convert.ToString(routeMapInvoke.Method.Invoke(null, new object[] { context.Request }));
                    if (!string.IsNullOrEmpty(url))
                    {
                        context.Items.Add("Uri", uri);
                        context.RewritePath(url);
                    }
                }
            }
            #endregion
        }

        void context_PostMapRequestHandler(object sender, EventArgs e)
        {
            HttpContext cont = ((HttpApplication)sender).Context;
            if (cont != null && !WebTool.IsRunToEnd(cont) && WebTool.IsMvcSuffix(cont.Request.Url))// && WebTool.IsCallMvc(cont.Request.Url)
            {
                if (cont.Handler == null || cont.Handler.ToString() == "System.Web.DefaultHttpHandler")
                {
                    //如果要兼容ASP.NET MVC，不能修改Handler，否则ASP.NET MVC不执行。
                    cont.Handler = SessionHandler.Instance;//注册Session
                }
            }
        }
        void context_AcquireRequestState(object sender, EventArgs e)
        {
            HttpContext cont = ((HttpApplication)sender).Context;
            if (cont != null && !WebTool.IsRunToEnd(cont) && WebTool.IsMvcSuffix(cont.Request.Url))// && WebTool.IsCallMvc(cont.Request.Url)
            {
                ReplaceOutput(cont);
                InvokeClass(cont);
            }
        }

        void context_Error(object sender, EventArgs e)
        {
            HttpContext cont = ((HttpApplication)sender).Context;
            if (cont != null && cont.Request.Url.LocalPath != "/favicon.ico")
            {
                WebTool.PrintRequestLog(cont, cont.Error);
            }
        }


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

            //ViewController是由页面的前两个路径决定了。
            string[] items = WebTool.GetLocalPath(context.Request.Url).Trim('/').Split('/');
            string className = ReflectConst.Global;
            if (MvcConfig.RouteMode == 1)
            {
                className = items.Length > 2 ? items[0] + "." + items[1] : items[0];
            }
            else if (MvcConfig.RouteMode == 2)
            {
                className = items.Length > 1 ? items[0] + "." + items[1] : items[0];
            }
            Type t = ControllerCollector.GetController(className);
            //if (t == null || t.Name == ReflectConst.DefaultController)
            //{
            //    if (Rpc.Gateway.Proxy(context, false))//客户端禁用做为网关，避免死循环。
            //    {
            //        return;
            //    }
            //}
            if (t == null)
            {
                //do nothing , next module to deal.
                //context.Response.StatusCode = 404;
                //context.Response.Write("404 - Not Found.");
                // WriteError("You need a " + className + " controller for coding!", context);
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

                context.Handler = null;//禁止其它模块执行。
            }
            
        }
        private void WriteError(string tip, HttpContext context)
        {
            context.Response.Write(JsonHelper.OutResult(false, tip));
        }
        #endregion


    }

}
