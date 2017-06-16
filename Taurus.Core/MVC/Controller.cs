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
using System.Text.RegularExpressions;
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
                    char[] attrFlags;
                    MethodInfo method = InvokeLogic.GetMethod(t, methodName, out attrFlags);
                    if (method != null)
                    {
                        if (attrFlags[0] == '1')
                        {
                            #region CheckToken
                            MethodInfo checkToken = InvokeLogic.GetMethod(t, InvokeLogic.CheckToken);
                            if (checkToken != null && checkToken.Name == InvokeLogic.CheckToken)
                            {
                                isGoOn = Convert.ToBoolean(checkToken.Invoke(this, null));
                            }
                            else if (InvokeLogic.CheckTokenMethod != null)
                            {
                                isGoOn = Convert.ToBoolean(InvokeLogic.CheckTokenMethod.Invoke(null, new object[] { this, methodName }));
                            }
                            #endregion
                        }
                        if (isGoOn && attrFlags[1] != attrFlags[2])//配置了HttpGet或HttpPost
                        {
                            if (attrFlags[1] == '1' && !IsHttpGet)
                            {
                                isGoOn = false;
                                Write("Only support HttpGet!", false);
                            }
                            else if (attrFlags[2] == '1' && !IsHttpPost)
                            {
                                isGoOn = false;
                                Write("Only support HttpPost!", false);
                            }

                        }
                        if (isGoOn)
                        {
                            #region Method Invoke
                            _Action = method.Name;
                            BeforeInvoke(method.Name);
                            if (!CancelLoadHtml)
                            {
                                _View = ViewEngine.Create(t.Name, method.Name);
                            }
                            if (!CancelInvoke)
                            {
                                method.Invoke(this, GetInvokeParas(method));
                                if (IsHttpPost)
                                {
                                    string name = GetBtnName();
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        MethodInfo postBtnMethod = InvokeLogic.GetMethod(t, name);
                                        if (postBtnMethod != null && postBtnMethod.Name != InvokeLogic.Default)
                                        {
                                            postBtnMethod.Invoke(this, GetInvokeParas(postBtnMethod));
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
                    if (apiResult[0] == '{' && apiResult[apiResult.Length - 1] == '}')
                    {
                        context.Response.ContentType = "application/json";
                    }
                    context.Response.Write(apiResult.ToString());
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                return;
            }
            catch (Exception err)
            {
                string errMssg = err.InnerException != null ? err.InnerException.Message : err.Message;
                WriteLog(errMssg);
                context.Response.Write(errMssg);
            }
            if (string.IsNullOrEmpty(context.Response.Charset))
            {
                context.Response.Charset = "utf-8";
            }
        }
        /// <summary>
        /// Write log to txt
        /// </summary>
        protected void WriteLog(string msg)
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
        private object[] GetInvokeParas(MethodInfo method)
        {
            object[] paras = null;
            #region 增加处理参数支持
            ParameterInfo[] piList = method.GetParameters();
            if (piList != null && piList.Length > 0)
            {
                paras = new object[piList.Length];
                for (int i = 0; i < piList.Length; i++)
                {
                    ParameterInfo pi = piList[i];
                    Type t = pi.ParameterType;
                    string value = Query<string>(pi.Name, null);
                    if (value == null)
                    {
                        if (t.IsValueType && t.IsGenericType && t.FullName.StartsWith("System.Nullable"))
                        {
                            continue;
                        }
                        if (ReflectTool.GetSystemType(ref t) != SysType.Base)//基础值类型
                        {
                            value = GetJson();
                        }
                    }
                    try
                    {
                        paras[i] = QueryTool.ChangeType(value, t);//类型转换（基础或实体）
                    }
                    catch (Exception err)
                    {
                        string typeName = t.Name;
                        if (typeName.StartsWith("Nullable"))
                        {
                            typeName = Nullable.GetUnderlyingType(t).Name;
                        }
                        string outMsg = string.Format("[{0} {1} = {2}]  [Error : {3}]", typeName, pi.Name, value,  err.Message);
                        context.Response.Write(outMsg);
                        context.Response.End();
                        break;
                    }

                }
            }
            #endregion
            return paras;
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
        private string[] _ParaItems;
        internal string[] ParaItems
        {
            get
            {
                if (_ParaItems == null)
                {
                    string[] items = QueryTool.GetLocalPath().Trim('/').Split('/');
                    int len = RouteConfig.RouteMode + 1;
                    if (items != null && items.Length > len)
                    {
                        _ParaItems = new string[items.Length - len];
                        Array.Copy(items, len, _ParaItems, 0, _ParaItems.Length);
                    }
                    else
                    {
                        _ParaItems = new string[1];
                        _ParaItems[0] = "";
                    }
                }
                return _ParaItems;
            }
        }
        /// <summary>
        /// Para value
        /// </summary>
        public string Para
        {
            get
            {
                return ParaItems[0];
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
        /// 缓存参数值
        /// </summary>
        private Dictionary<string, string> queryCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
            if (queryCache.ContainsKey(key))
            {
                return QueryTool.ChangeValueType<T>(queryCache[key], defaultValue, false);
            }

            T value = QueryTool.Query<T>(key, defaultValue, false);
            if (value == null)
            {
                //尝试从Json中获取
                string result = JsonHelper.GetValue(GetJson(), key);
                if (!string.IsNullOrEmpty(result))
                {
                    value = QueryTool.ChangeValueType<T>(result, defaultValue, false);
                }
                else if (context.Request.Headers[key] != null)
                {
                    value = QueryTool.ChangeValueType<T>(context.Request.Headers[key], defaultValue, false);
                }
            }
            if (value != null && !queryCache.ContainsKey(key))
            {
                queryCache.Add(key, value.ToString());
            }
            return value;
        }
        public T Query<T>(int paraIndex)
        {
            return Query<T>(paraIndex, default(T));

        }
        public T Query<T>(int paraIndex, T defaultValue)
        {
            if (ParaItems.Length > paraIndex)
            {
                return QueryTool.ChangeValueType<T>(ParaItems[paraIndex], defaultValue, false);
            }
            return defaultValue;
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
        private string _Json = null;
        /// <summary>
        /// 获取Get或Post的数据并转换为Json格式。
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            if (_Json == null)
            {
                if (IsHttpPost)
                {
                    if (context.Request.Form.Count > 0)
                    {
                        if (context.Request.Form.Count == 1 && context.Request.Form.Keys[0] == null)
                        {
                            return JsonHelper.ToJson(context.Request.Form[0]);
                        }
                        _Json = JsonHelper.ToJson(context.Request.Form);
                    }
                    else
                    {
                        Stream stream = context.Request.InputStream;
                        if (stream != null && stream.Length > 0)
                        {
                            Byte[] bytes = new Byte[stream.Length];
                            stream.Read(bytes, 0, bytes.Length);
                            string data = System.Text.Encoding.UTF8.GetString(bytes);
                            _Json = JsonHelper.ToJson(data);
                        }
                    }
                }
                else if (IsHttpGet)
                {
                    string para = context.Request.Url.Query.TrimStart('?');
                    if (!string.IsNullOrEmpty(para))
                    {
                        if (para.IndexOf("%") > -1)
                        {
                            para = HttpUtility.UrlDecode(para);
                        }
                        _Json = JsonHelper.ToJson(para);
                    }

                }
                if (string.IsNullOrEmpty(_Json))
                {
                    _Json = "{}";
                }
            }
            return _Json;
        }

        /// <summary>
        /// 格式检测，并返回第一个错误的参数，若正确则返null
        /// </summary>
        /// <param name="errMsg">参数为空时（结束请求返回的错误信息，不想结束请求可以传null）<para>
        /// 示例：{0}不能为空&{0}格式错误</para></param>
        /// <param name="paras">示例：mobile&手机号&^1[3|4|5|8][0-9]\d{8}$</param>
        /// <returns></returns>
        public string CheckFormat(string errMsg, params string[] paras)
        {
            if (paras.Length > 0)
            {
                //if (paras.Length == 1 && paras[0].IndexOf(',') > 0)//"支持"aaa,bbb"这样的写法。
                //{
                //    paras = paras[0].Split(',');
                //}
                string json = GetJson();
                foreach (string para in paras)
                {
                    if (!string.IsNullOrEmpty(para))
                    {
                        string[] items = para.Split('&');//支持"user&用户名&正则表达式"这样的写法.
                        string key = items[0];
                        string value = context.Request.Headers[key];
                        if (string.IsNullOrEmpty(value))
                        {
                            value = JsonHelper.GetValue(json, key);
                        }
                        if (string.IsNullOrEmpty(value))//参数为空
                        {
                            if (!string.IsNullOrEmpty(errMsg))
                            {
                                errMsg = errMsg.Split('&')[0];
                                context.Response.Write(JsonHelper.OutResult(false, string.Format(errMsg, items.Length > 1 ? items[1] : key)));
                                context.Response.End();
                            }
                            return para;
                        }
                        else if (items.Length > 2)//有正则
                        {
                            if (value.IndexOf('%') > -1)
                            {
                                value = HttpUtility.UrlDecode(value);
                            }
                            if (!Regex.IsMatch(value, items[2]))//如果格式错误
                            {
                                if (!string.IsNullOrEmpty(errMsg))
                                {
                                    if (errMsg.IndexOf('&') > 0)
                                    {
                                        errMsg = errMsg.Split('&')[1];
                                    }
                                    context.Response.Write(JsonHelper.OutResult(false, string.Format(errMsg, items.Length > 1 ? items[1] : key)));
                                    context.Response.End();
                                }
                                return para;
                            }
                        }
                        if (!queryCache.ContainsKey(key))
                        {
                            queryCache.Add(key, value);
                        }
                    }
                }
            }
            return null;
        }
    }
}
