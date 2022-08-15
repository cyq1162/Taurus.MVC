using CYQ.Data;
using CYQ.Data.Tool;
using System;
using System.Reflection;
using System.Threading;
using System.Web;

namespace Taurus.Core
{
    /// <summary>
    /// 权限检测模块（NetCore 下处理成单例模式）
    /// </summary>
    public class UrlRewrite : IHttpModule
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
            MicroService.Run.Start(host);//微服务检测、启动。
            if (!QueryTool.IsCallMicroServiceReg(uri) && MicroService.Run.Proxy(context, true))
            {
                QueryTool.IsRunProxySuccess = true;
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

            if (QueryTool.IsCallMvc(uri))
            {
                if (context.Request.Url.LocalPath == "/")//设置默认首页
                {
                    string defaultUrl = QueryTool.GetDefaultUrl();
                    if (!string.IsNullOrEmpty(defaultUrl))
                    {
                        context.RewritePath(defaultUrl);
                        return;
                    }
                }
                if (QueryTool.IsTaurusSuffix(uri))
                {
                    MethodInfo routeMapInvoke = InvokeLogic.RouteMapInvokeMethod;
                    if (routeMapInvoke != null)
                    {
                        string url = Convert.ToString(routeMapInvoke.Invoke(null, new object[] { context.Request }));
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
            if (cont != null && QueryTool.IsCallMvc(cont.Request.Url) && !QueryTool.IsRunProxySuccess && QueryTool.IsTaurusSuffix(cont.Request.Url))
            {
                cont.Handler = SessionHandler.Instance;//注册Session
            }
        }
        void context_AcquireRequestState(object sender, EventArgs e)
        {
            HttpContext cont = ((HttpApplication)sender).Context;
            if (cont != null && QueryTool.IsCallMvc(cont.Request.Url) && !QueryTool.IsRunProxySuccess && QueryTool.IsTaurusSuffix(cont.Request.Url))
            {
                CheckCORS(cont);
                ReplaceOutput(cont);
                InvokeClass(cont);
            }
        }

        void context_Error(object sender, EventArgs e)
        {
            HttpContext cont = ((HttpApplication)sender).Context;
            if (cont != null && QueryTool.IsCallMvc(cont.Request.Url) && QueryTool.IsTaurusSuffix(cont.Request.Url))
            {
                Log.WriteLogToTxt(cont.Error);
            }
        }

        #region 检测CORS跨域请求
        private void CheckCORS(HttpContext context)
        {
            if (QueryTool.IsAllowCORS())
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
            if (QueryTool.IsSubAppSite(context.Request.Url))
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
            string[] items = QueryTool.GetLocalPath(context.Request.Url).Trim('/').Split('/');
            string className = InvokeLogic.Const.Default;
            if (RouteConfig.RouteMode == 1)
            {
                className = items.Length > 2 ? items[0] + "." + items[1] : items[0];
            }
            else if (RouteConfig.RouteMode == 2)
            {
                className = items.Length > 1 ? items[0] + "." + items[1] : items[0];
            }
            t = InvokeLogic.GetController(className);
            if (t == null || t.Name == InvokeLogic.Const.DefaultController)
            {
                if (MicroService.Run.Proxy(context, false))//客户端做为网关。
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
                    object o = Activator.CreateInstance(t);//实例化
                    t.GetMethod("ProcessRequest").Invoke(o, new object[] { context });
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
