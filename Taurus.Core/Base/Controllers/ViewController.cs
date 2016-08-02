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
    public abstract class ViewController : ControllerProperty, IHttpHandler, IRequiresSessionState
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
                Type t = _Controller = this.GetType();
                MethodInfo method = null;
                string localPath = context.Request.Url.LocalPath.Trim('/');
                int len = 0;
                if (localPath != "")
                {
                    string[] items = localPath.ToLower().Split('/');
                    if (t.Name == InvokeLogic.DefaultViewController && items[0] != "default")
                    {
                        method = InvokeLogic.GetMethod(t, items[0]);
                        if (method != null)
                        {
                            len = items[0].Length;
                        }
                    }
                    else if (items.Length > 1)
                    {
                        method = InvokeLogic.GetMethod(t, items[1]);
                        len = items[0].Length;
                        if (method != null)
                        {
                            len += items[1].Length + 1;
                        }
                    }

                    _Para = localPath.Substring(len).Trim('/');
                }
                if (method == null)
                {
                    method = t.GetMethod(InvokeLogic.Default);
                }
                else
                {
                    //设置参数

                }
                if (method != null)
                {
                    _Action = method.Name;


                    BeforeInvoke(method.Name);
                    if (!CancelLoadHtml)
                    {
                        _View = ViewEngine.Create(t.Name, method.Name);
                    }
#if DEBUG
                    string text = "Invoke " + t.FullName + "." + Action + "(" + Para + ")<hr />";
                    if (_View != null)
                    {
                        _View.AppendNode(_View.GetList("body")[0], _View.CreateNode("div", text), 0);
                    }
                    else
                    {
                        System.Web.HttpContext.Current.Response.Write(text);
                    }
#endif
                    if (!CancelInvoke)
                    {
                        method.Invoke(this, null);
                        if (!CancelInvoke)
                        {
                            EndInvoke(method.Name);
                        }
                    }
                    if (View != null)
                    {
                        context.Response.Write(View.OutXml);
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
        private XHtmlAction _View;
        /// <summary>
        /// 视图模板引擎
        /// </summary>
        public override XHtmlAction View
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
        private Type _Controller;
        public override Type Controller
        {
            get
            {
                return _Controller;
            }
        }
        private string _Action;
        public override string Action
        {
            get
            {
                return _Action;
            }
        }
        private string _Para = "";
        public override string Para
        {
            get
            {
                return _Para;
            }
        }
        /// <summary>
        /// 是否点击了某事件
        /// </summary>
        /// <param name="btnName">按钮名称</param>
        protected bool IsClick(string btnName)
        {
            return Query<string>(btnName) != null;
        }
    }
}
