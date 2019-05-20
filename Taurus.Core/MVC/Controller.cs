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
        /// 获取待发送的缓冲区的数据
        /// </summary>
        public string APIResult
        {
            get
            {
                return apiResult.ToString();
            }
        }
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
                #region GetMethodName
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
                        _Module = items[0];
                        if (items.Length > 2)
                        {
                            methodName = items[2];
                        }
                        break;
                }
                _Action = methodName;
                #endregion
                bool isGoOn = true;

                char[] attrFlags;
                MethodInfo method = InvokeLogic.GetMethod(t, methodName, out attrFlags);
                if (method != null)
                {
                    if (attrFlags[0] == '1')
                    {
                        #region Validate CheckToken
                        MethodInfo checkToken = InvokeLogic.GetMethod(t, InvokeLogic.Const.CheckToken);
                        if (checkToken != null && checkToken.Name == InvokeLogic.Const.CheckToken)
                        {
                            isGoOn = Convert.ToBoolean(checkToken.Invoke(this, null));
                        }
                        else if (InvokeLogic.DefaultCheckToken != null)
                        {
                            isGoOn = Convert.ToBoolean(InvokeLogic.DefaultCheckToken.Invoke(null, new object[] { this, methodName }));
                        }
                        else if (InvokeLogic.AuthCheckToken != null)
                        {
                            isGoOn = Convert.ToBoolean(InvokeLogic.AuthCheckToken.Invoke(null, new object[] { this }));
                        }
                        #endregion
                    }
                    if (isGoOn)//配置了HttpGet或HttpPost
                    {
                        #region Validate Http methods
                        string[] httpMethods = InvokeLogic.HttpMethods;
                        for (int i = 0; i < httpMethods.Length; i++)
                        {
                            if (attrFlags[i + 1] == '1')
                            {
                                isGoOn = false;
                                if (httpMethods[i] == Request.HttpMethod)
                                {
                                    isGoOn = true;
                                    break;
                                }
                            }
                        }
                        if (!isGoOn)
                        {
                            Write("Http method not support " + Request.HttpMethod, false);
                        }
                        #endregion

                    }
                    if (isGoOn)
                    {
                        #region Method Invoke


                        #region BeforeInvoke
                        MethodInfo beforeInvoke = InvokeLogic.GetMethod(t, InvokeLogic.Const.BeforeInvoke);
                        if (beforeInvoke != null && beforeInvoke.Name == InvokeLogic.Const.BeforeInvoke)
                        {
                            isGoOn = Convert.ToBoolean(beforeInvoke.Invoke(this, new object[] { method.Name }));
                        }
                        else if (InvokeLogic.BeforeInvokeMethod != null)
                        {
                            isGoOn = Convert.ToBoolean(InvokeLogic.BeforeInvokeMethod.Invoke(null, new object[] { this, methodName }));
                        }
                        #endregion

                        //BeforeInvoke(method.Name);

                        if (!CancelLoadHtml)
                        {
                            _View = ViewEngine.Create(t.Name, method.Name);
                            if (_View != null)
                            {
                                //追加几个全局标签变量
                                _View.KeyValue.Add("module", Module.ToLower());
                                _View.KeyValue.Add("controller", ControllerType.Name.ToLower());
                                _View.KeyValue.Add("action", Action.ToLower());
                                _View.KeyValue.Add("para", Para.ToLower());
                            }
                        }

                        if (isGoOn)
                        {
                            object[] paras;
                            if (GetInvokeParas(method, out paras))
                            {
                                method.Invoke(this, paras);
                                if (IsHttpPost)
                                {
                                    #region Button Invoke
                                    string name = GetBtnName();
                                    if (!string.IsNullOrEmpty(name))
                                    {
                                        MethodInfo postBtnMethod = InvokeLogic.GetMethod(t, name);
                                        if (postBtnMethod != null && postBtnMethod.Name != InvokeLogic.Const.Default)
                                        {
                                            GetInvokeParas(postBtnMethod, out paras);
                                            postBtnMethod.Invoke(this, paras);
                                        }
                                    }
                                    #endregion
                                }
                                if (isGoOn)
                                {
                                    #region EndInvoke
                                    MethodInfo endInvoke = InvokeLogic.GetMethod(t, InvokeLogic.Const.EndInvoke);
                                    if (endInvoke != null && endInvoke.Name == InvokeLogic.Const.EndInvoke)
                                    {
                                        isGoOn = Convert.ToBoolean(endInvoke.Invoke(this, new object[] { method.Name }));
                                    }
                                    else if (InvokeLogic.EndInvokeMethod != null)
                                    {
                                        InvokeLogic.EndInvokeMethod.Invoke(null, new object[] { this, methodName });
                                    }
                                    #endregion
                                    if (InvokeLogic.DocRecord != null)
                                    {
                                        InvokeLogic.DocRecord.Invoke(null, new object[] { this, methodName });
                                    }
                                }
                            }
                        }
                        #endregion
                    }
                }
                if (string.IsNullOrEmpty(context.Response.Charset))
                {
                    context.Response.Charset = "utf-8";
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
                if (View == null)
                {
                    errMssg = JsonHelper.OutResult(false, errMssg);
                }
                context.Response.Write(errMssg);
            }

        }
        /// <summary>
        /// Write log to txt
        /// </summary>
        protected void WriteLog(string msg)
        {
            Log.WriteLogToTxt(msg);
        }
        public virtual bool BeforeInvoke(string methodName)
        {
            return true;
        }
        public virtual void EndInvoke(string methodName)
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
        private bool GetInvokeParas(MethodInfo method, out object[] paras)
        {
            paras = null;
            #region 增加处理参数支持
            ParameterInfo[] piList = method.GetParameters();
            object[] validateList = method.GetCustomAttributes(typeof(RequireAttribute), true);
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
                    //检测是否允许为空，是否满足正则格式。
                    if (!ValidateParas(validateList, pi.Name, value))
                    {
                        return false;
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
                        string outMsg = string.Format("[{0} {1} = {2}]  [Error : {3}]", typeName, pi.Name, value, err.Message);
                        Write(outMsg, false);
                        return false;
                    }

                }
            }
            //对未验证过的参数，再进行一次验证。
            foreach (object item in validateList)
            {
                RequireAttribute valid = item as RequireAttribute;
                if (!valid.isValidated)
                {
                    if (valid.paraName.IndexOf(',') > -1)
                    {
                        foreach (string name in valid.paraName.Split(','))
                        {
                            if (string.IsNullOrEmpty(Query<string>(name)))
                            {
                                Write(string.Format(valid.emptyTip, name), false);
                                return false;
                            }
                        }
                    }
                    else if (!ValidateParas(validateList, valid.paraName, Query<string>(valid.paraName)))
                    {
                        return false;
                    }
                }
            }
            validateList = null;
            #endregion
            return true;
        }
        private bool ValidateParas(object[] validateList, string paraName, string paraValue)
        {
            if (validateList != null)
            {
                foreach (object item in validateList)
                {
                    RequireAttribute valid = item as RequireAttribute;
                    if (!valid.isValidated && (valid.paraName == paraName || valid.paraName.StartsWith(paraName + ".")))
                    {
                        valid.isValidated = true;//设置已经验证过此参数，后续可以跳过。
                        if (valid.paraName.StartsWith(paraName + ".") && !string.IsNullOrEmpty(paraValue))
                        {
                            paraValue = JsonHelper.GetValue(paraValue, valid.paraName.Substring(paraName.Length + 1));
                        }
                        if (valid.isRequired && string.IsNullOrEmpty(paraValue))
                        {
                            Write(valid.emptyTip, false);
                            return false;
                        }
                        else if (!string.IsNullOrEmpty(valid.regex) && !string.IsNullOrEmpty(paraValue))
                        {
                            if (paraValue.IndexOf('%') > -1)
                            {
                                paraValue = HttpUtility.UrlDecode(paraValue);
                            }
                            if (!Regex.IsMatch(paraValue, valid.regex))//如果格式错误
                            {
                                Write(valid.regexTip, false);
                                return false;
                            }
                        }
                    }
                }

            }

            return true;
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

        private string _Module = "";
        /// <summary>
        /// Module value
        /// </summary>
        public string Module
        {
            get
            {
                return _Module;
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
        private string _Action = "";
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
        public HttpRequest Request
        {
            get
            {
                return Context == null ? null : Context.Request;
            }
        }

        public HttpResponse Response
        {
            get { return Context == null ? null : Context.Response; }
        }
        #region Http Method

        public bool IsHttpGet
        {
            get { return Context.Request.HttpMethod == "GET"; }
        }

        public bool IsHttpPost
        {
            get { return Context.Request.HttpMethod == "POST"; }
        }

        public bool IsHttpHead
        {
            get { return Context.Request.HttpMethod == "HEAD"; }
        }

        public bool IsHttpPut
        {
            get { return Context.Request.HttpMethod == "PUT"; }
        }

        public bool IsHttpDelete
        {
            get { return Context.Request.HttpMethod == "DELETE"; }
        }
        #endregion
        /// <summary>
        /// 缓存参数值
        /// </summary>
        private Dictionary<string, string> queryCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        static string[] autoPrefixs = ("," + AppConfig.UI.AutoPrefixs).Split(',');
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

            T value = default(T);
            foreach (string pre in autoPrefixs)
            {
                string newKey = pre + key;
                if (Context.Request[newKey] == null)
                {
                    //尝试从Json中获取
                    string result = JsonHelper.GetValue(GetJson(), newKey);
                    if (!string.IsNullOrEmpty(result))
                    {
                        value = QueryTool.ChangeValueType<T>(result, defaultValue, false);
                        break;
                    }
                    else if (context.Request.Headers[newKey] != null)
                    {
                        value = QueryTool.ChangeValueType<T>(context.Request.Headers[newKey], defaultValue, false);
                        break;
                    }
                    else
                    {
                        value = defaultValue;
                    }
                }
                else
                {
                    value = QueryTool.Query<T>(newKey, defaultValue, false);//这里不设置默认值(值类型除外）
                    break;
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
                        if (stream != null && stream.CanRead)
                        {
                            long len = (long)context.Request.ContentLength;

                            Byte[] bytes = new Byte[len];
                            stream.Read(bytes, 0, bytes.Length);
                            string data = System.Text.Encoding.UTF8.GetString(bytes);
                            if (data.IndexOf("%") > -1)
                            {
                                data = HttpUtility.UrlDecode(data);
                            }
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
        /// 格式检测看是否满足验证条件(错误信息可获取APIResult属性)
        /// </summary>
        /// <param name="formatter">示例：{0}不能为空,{0}格式错误</param>
        /// <param name="paras">示例：mobile,手机号,^1[3|4|5|8][0-9]\d{8}$</param>
        /// <returns></returns>
        public bool CheckFormat(string formatter, params string[] paras)
        {
            if (paras.Length > 0)
            {
                string json = GetJson();
                foreach (string para in paras)
                {
                    if (!string.IsNullOrEmpty(para))
                    {
                        string[] items = para.Split(',', '&');//支持"user&用户名&正则表达式"这样的写法.
                        string key = items[0];
                        string value = Query<string>(key);
                        if (string.IsNullOrEmpty(value))
                        {
                            value = JsonHelper.GetValue(json, key);
                        }
                        if (string.IsNullOrEmpty(value))//参数为空
                        {
                            if (!string.IsNullOrEmpty(formatter))
                            {

                                formatter = formatter.Split(',', '&')[0];
                                Write(string.Format(formatter, items.Length > 1 ? items[1] : key), false);
                            }
                            return false;
                        }
                        else if (items.Length > 2)//有正则
                        {
                            if (value.IndexOf('%') > -1)
                            {
                                value = HttpUtility.UrlDecode(value);
                            }
                            if (!Regex.IsMatch(value, items[2]))//如果格式错误
                            {
                                if (!string.IsNullOrEmpty(formatter))
                                {
                                    if (formatter.IndexOfAny(new char[] { ',', '&' }) > 0)
                                    {
                                        formatter = formatter.Split(',', '&')[1];
                                    }
                                    Write(string.Format(formatter, items.Length > 1 ? items[1] : key), false);
                                }
                                return false;
                            }
                        }
                        if (!queryCache.ContainsKey(key))
                        {
                            queryCache.Add(key, value);
                        }
                    }
                }
            }
            return true;
        }
    }
}
