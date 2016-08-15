using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.Web;
using System.Web.SessionState;
using CYQ.Data.Xml;
using CYQ.Data;
using System.IO;
using System.Xml;
namespace Taurus.Core
{
    /// <summary>
    /// 视图控制器基类
    /// </summary>
    public abstract partial class Controller : IHttpHandler, IRequiresSessionState
    {
        /// <summary>
        /// 取消继续调用事件（可以在重载BeforeInvoke方法内使用）
        /// </summary>
        protected bool CancelInvoke = false;
        /// <summary>
        /// 是否取消加载Html文件
        /// </summary>
        protected bool CancelLoadHtml = false;
        HttpContext context;
        public bool IsReusable
        {
            get { return true; }
        }

        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            try
            {
                Type t = _ControllerType = this.GetType();
                string[] items = context.Request.Url.LocalPath.Trim('/').Split('/');
                string methodName = string.Empty;
                switch (RouteConfig.RouteMode)
                {
                    case 0:
                        methodName = items[0];
                        break;
                    case 1:
                        if (items.Length > 1)
                        {
                            methodName = items[1];
                        }
                        break;
                    case 2:
                        if (items.Length > 2)
                        {
                            methodName = items[2];
                        }
                        break;
                }
                MethodInfo method = InvokeLogic.GetMethod(t, methodName);
                if (method != null)
                {
                    _Action = method.Name;
                    BeforeInvoke(method.Name);
                    if (!CancelLoadHtml)
                    {
                        _View = ViewEngine.Create(t.Name, method.Name);
                    }
                    //#if DEBUG
                    //                    string text = "Invoke " + t.FullName + "." + Action + "(" + Para + ")<hr />";
                    //                    if (_View != null)
                    //                    {
                    //                        _View.AppendNode(_View.GetList("body")[0], _View.CreateNode("div", text), 0);
                    //                    }
                    //                    else
                    //                    {
                    //                        System.Web.HttpContext.Current.Response.Write(text);
                    //                    }
                    //#endif
                    if (!CancelInvoke)
                    {
                        method.Invoke(this, null);
                        if (IsHttpPost)
                        {
                            string name = GetBtnName();
                            if (!string.IsNullOrEmpty(name))
                            {
                                MethodInfo postBtnMethod = InvokeLogic.GetMethod(t, name);
                                if (postBtnMethod != null && postBtnMethod.Name != InvokeLogic.Default)
                                {
                                    postBtnMethod.Invoke(this, null);
                                }
                            }
                        }
                        if (!CancelInvoke)
                        {
                            EndInvoke(method.Name);
                        }
                    }
                    if (View != null)
                    {
                        context.Response.Write(View.OutXml);
                    }
                    else if (!string.IsNullOrEmpty(ajaxResult))
                    {
                        context.Response.Write(ajaxResult);
                    }
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                return;
            }
            catch (Exception err)
            {
                WriteError(err.Message);
                context.Response.Write(err.Message);
            }

            context.Response.End();
        }
        protected void WriteError(string msg)
        {
            Log.WriteLogToTxt(msg);
        }
        protected virtual void BeforeInvoke(string methodName)
        {

        }
        protected virtual void EndInvoke(string methodName)
        {

        }
        public virtual void Default()
        {

        }

        /// <summary>
        /// 是否点击了某事件
        /// </summary>
        /// <param name="btnName">按钮名称</param>
        protected bool IsClick(string btnName)
        {
            return Query<string>(btnName) != null;
        }
        private string GetBtnName()
        {
            foreach (string name in Context.Request.QueryString)
            {
                if (name.ToLower().StartsWith("btn"))
                {
                    return name;
                }
            }
            foreach (string name in Context.Request.Form)
            {
                if (name.ToLower().StartsWith("btn"))
                {
                    return name;
                }
            }
            return null;
        }
    }
    public abstract partial class Controller : IController
    {
        private XHtmlAction _View;
        /// <summary>
        /// 视图模板引擎
        /// </summary>
        public XHtmlAction View
        {
            get
            {
                return _View;
            }
            set
            {
                _View = value;//开放Set是考虑用户可以获取OutXml后，将此设为Null，再自定义输出。
            }
        }
        private Type _ControllerType;
        public Type ControllerType
        {
            get
            {
                return _ControllerType;
            }
        }
        private string _Action;
        public string Action
        {
            get
            {
                return _Action;
            }
        }
        private string _Para;
        public string Para
        {
            get
            {
                if (_Para == null)
                {
                    _Para = "";
                    string[] items = Context.Request.Url.LocalPath.Trim('/').Split('/');
                    switch (RouteConfig.RouteMode)
                    {
                        case 1:
                            if (items.Length > 2)
                            {
                                _Para = items[2];
                            }
                            break;
                        case 2:
                            if (items.Length > 3)
                            {
                                _Para = items[3];
                            }
                            break;
                    }

                }
                return _Para;
            }
        }
        /// <summary>
        /// datagrid分页的页码数
        /// </summary>
        public int PageIndex
        {
            get
            {
                return Query<int>("page", 1);
            }
        }
        /// <summary>
        /// datagrid分页的页容量
        /// </summary>
        public int PageSize
        {
            get
            {
                return Query<int>("rows", 10);
            }
        }

        /// <summary>
        /// 排序字段名
        /// </summary>
        public string Sort
        {
            get
            {
                return Query<string>("sort", "");
            }

        }
        /// <summary>
        /// 排序类型（升或降）
        /// </summary>
        public string Order
        {
            get
            {
                return Query<string>("order", "desc");
            }
        }
        public HttpContext Context
        {
            get { return HttpContext.Current; }
        }

        public bool IsHttpGet
        {
            get { return Context.Request.RequestType == "GET"; }
        }

        public bool IsHttpPost
        {
            get { return Context.Request.RequestType == "POST"; }
        }

        public T Query<T>(Enum key)
        {
            return Query<T>(key.ToString(), default(T));
        }
        public T Query<T>(string key)
        {
            return Query<T>(key, default(T));
        }
        public T Query<T>(string key, T defaultValue)
        {
            return QueryTool.Query<T>(key, defaultValue, false);
        }

        protected string ajaxResult = string.Empty;
        /// <summary>
        /// Ajax发起的请求，需要返回值时，对此赋值即可。
        /// </summary>
        public string AjaxResult
        {
            get
            {
                return ajaxResult;
            }
            set
            {
                ajaxResult = value;
            }
        }
    }
}
