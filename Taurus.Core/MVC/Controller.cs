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
using CYQ.Data.Tool;
using CYQ.Data.Table;
namespace Taurus.Core
{
    /// <summary>
    /// the base of Controller
    /// <para>控制器基类</para>
    /// </summary>
    public abstract partial class Controller : IHttpHandler
    {
        private StringBuilder apiResult = new StringBuilder();
        /// <summary>
        /// to stop invoke method
        /// <para>取消继续调用事件（可以在重载BeforeInvoke方法内使用）</para>
        /// </summary>
        protected bool CancelInvoke = false;
        /// <summary>
        /// to stop load view html
        /// <para>是否取消加载Html文件</para>
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
                string[] items = QueryTool.GetLocalPath().Trim('/').Split('/');
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
                bool isGoOn = true;
                if (InvokeLogic.BeforeInvokeMethod != null)
                {
                    isGoOn = Convert.ToBoolean(InvokeLogic.BeforeInvokeMethod.Invoke(null, new object[] { this, methodName }));
                }
                if (isGoOn)
                {
                    bool hasTokenAttr = false;
                    MethodInfo method = InvokeLogic.GetMethod(t, methodName, out hasTokenAttr);
                    if (method != null)
                    {
                        if (hasTokenAttr)
                        {
                            //CheckToken
                            MethodInfo checkToken = InvokeLogic.GetMethod(t, InvokeLogic.CheckToken);
                            if (checkToken.Name == InvokeLogic.CheckToken)
                            {
                                isGoOn = Convert.ToBoolean(checkToken.Invoke(this, null));
                            }
                            else if (InvokeLogic.CheckTokenMethod != null)
                            {
                                isGoOn = Convert.ToBoolean(InvokeLogic.CheckTokenMethod.Invoke(null, new object[] { this, methodName }));
                            }
                        }
                        if (isGoOn)
                        {
                            #region MyRegion
                            _Action = method.Name;
                            BeforeInvoke(method.Name);
                            if (!CancelLoadHtml)
                            {
                                _View = ViewEngine.Create(t.Name, method.Name);
                            }
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
                            #endregion
                        }
                    }
                    
                }

                if (View != null)
                {
                    context.Response.Write(View.OutXml);
                }
                else if (apiResult.Length > 0)
                {
                    context.Response.Write(apiResult.ToString());
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
            if (string.IsNullOrEmpty(context.Response.Charset))
            {
                context.Response.Charset = "utf-8";
            }
        }
        /// <summary>
        /// Write log to txt
        /// </summary>
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
        /// if the result is false will stop invoke method
        /// <para>检测授权是否通过</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckToken()
        {
            return true;
        }
        /// <summary>
        /// is button event
        /// <para>是否点击了某事件</para>
        /// </summary>
        /// <param name="btnName">button name<para>按钮名称</para></param>
        protected bool IsClick(string btnName)
        {
            return Query<string>(btnName) != null;
        }
        private string GetBtnName()
        {
            foreach (string name in Context.Request.QueryString)
            {
                if (name != null && name.ToLower().StartsWith("btn"))
                {
                    return name;
                }
            }
            foreach (string name in Context.Request.Form)
            {
                if (name != null && name.ToLower().StartsWith("btn"))
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
        /// the View Engine
        /// <para>视图模板引擎</para>
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
        /// <summary>
        /// Controller Type
        /// </summary>
        public Type ControllerType
        {
            get
            {
                return _ControllerType;
            }
        }
        private string _Action;
        /// <summary>
        /// Action value
        /// </summary>
        public string Action
        {
            get
            {
                return _Action;
            }
        }
        private string _Para;
        /// <summary>
        /// Para value
        /// </summary>
        public string Para
        {
            get
            {
                if (_Para == null)
                {
                    _Para = "";
                    string[] items = QueryTool.GetLocalPath().Trim('/').Split('/');
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
        /// request["page"]
        /// <para>datagrid分页的第N页</para>
        /// </summary>
        public int PageIndex
        {
            get
            {
                return Query<int>("page", 1);
            }
        }
        /// <summary>
        /// request["rows"]
        /// <para>datagrid分页的每页N条</para>
        /// </summary>
        public int PageSize
        {
            get
            {
                return Query<int>("rows", 10);
            }
        }

        /// <summary>
        /// request["sort"]
        /// <para>排序字段名</para>
        /// </summary>
        public string Sort
        {
            get
            {
                return Query<string>("sort", "");
            }

        }
        /// <summary>
        /// request["order"]
        /// <para>排序类型（升或降） default desc</para>
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
        /// <summary>
        /// Get Request value
        /// </summary>
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
        /// <summary>
        /// Write String result
        /// <para> 输出原始msg的数据</para>
        /// </summary>
        /// <param name="msg">message<para>消息内容</para></param>
        public void Write(string msg)
        {
            apiResult.Append(msg);
        }
        /// <summary>
        /// Write Json result
        /// <para>输出Json格式的数据</para>
        /// </summary>
        /// <param name="isSuccess">success or not</param>
        public void Write(string msg, bool isSuccess)
        {
            apiResult.Append(JsonHelper.OutResult(isSuccess, msg));
        }
        /// <summary>
        /// Write Json result
        /// <para>传进对象时，会自动将对象转Json</para>
        /// </summary>
        /// <param name="obj">any obj is ok<para>对象或支持IEnumerable接口的对象列表</para></param>
        public void Write(object obj)
        {
            Write(JsonHelper.ToJson(obj));
        }

        public void Write(object obj, bool isSuccess)
        {
            Write(JsonHelper.ToJson(obj), isSuccess);
        }

        /// <summary>
        /// Get entity from post form
        /// <para>从Post过来的数据中获得实体类型的转换</para>
        /// </summary>
        /// <returns></returns>
        public T GetEntity<T>() where T : class
        {
            return JsonHelper.ToEntity<T>(GetJson());
            //object obj = Activator.CreateInstance(typeof(T));
            //MDataRow row = MDataRow.CreateFrom(obj);
            //row.LoadFrom();
            //return row.ToEntity<T>();
        }
        /// <summary>
        /// 获取Get或Post的数据并转换为Json格式。
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            if (IsHttpPost)
            {
                if (context.Request.Form.Count > 0)
                {
                    if (context.Request.Form.Count == 1 && context.Request.Form.Keys[0] == null)
                    {
                        return JsonHelper.ToJson(context.Request.Form[0]);
                    }
                    return JsonHelper.ToJson(context.Request.Form);
                }
                else
                {
                    Stream stream = context.Request.InputStream;
                    if (stream != null && stream.Length > 0)
                    {
                        Byte[] bytes = new Byte[stream.Length];
                        stream.Read(bytes, 0, bytes.Length);
                        string data = System.Text.Encoding.UTF8.GetString(bytes);
                        return JsonHelper.ToJson(data);
                    }
                }
            }
            else if (IsHttpGet)
            {
                return JsonHelper.ToJson(context.Request.Url.Query);
            }
            return "{}";
        }
    }
}
