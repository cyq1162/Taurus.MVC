using System;
using System.Reflection;
using System.Text;
using System.Web;
using CYQ.Data.Xml;
using CYQ.Data;
using System.IO;
using CYQ.Data.Tool;
using System.Text.RegularExpressions;
using Taurus.Plugin.Doc;
using Taurus.Mvc.Attr;
using System.Threading;
using Taurus.Plugin.MicroService;

namespace Taurus.Mvc
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

        HttpContext context;
        public bool IsReusable
        {
            get { return true; }
        }
        private void InitNameFromUrl()
        {
            _ControllerType = this.GetType();
            _ControllerName = _ControllerType.Name.Replace(ReflectConst.Controller, "").ToLower();
            string[] items = WebTool.GetLocalPath(Request.Url).Trim('/').Split('/');
            int paraStartIndex = MvcConfig.RouteMode + 1;
            string methodName = string.Empty;
            switch (MvcConfig.RouteMode)
            {
                case 0:
                    methodName = items[0];
                    break;
                case 1:
                    if (items.Length > 1)
                    {
                        if (items.Length > 2 && items[0].ToLower() != _ControllerName && items[1].ToLower() == _ControllerName && items[0] == MsConfig.Client.Name.ToLower())
                        {
                            paraStartIndex++;
                            methodName = items[2];//往后兼容一格。
                        }
                        else
                        {
                            methodName = items[1];
                        }
                    }
                    break;
                case 2:
                    _ModuleName = items[0];
                    if (items.Length > 2)
                    {
                        methodName = items[2];
                    }
                    else if (items.Length > 1)
                    {
                        //兼容【路由1=》（变更为）2】
                        methodName = items[1];
                    }
                    break;
            }
            _MethodName = methodName;
            if (string.IsNullOrEmpty(_MethodName))
            {
                _MethodName = ReflectConst.Default;
            }

            if (items.Length > paraStartIndex)
            {
                _ParaItems = new string[items.Length - paraStartIndex];
                Array.Copy(items, paraStartIndex, _ParaItems, 0, _ParaItems.Length);
            }
            else
            {
                _ParaItems = new string[1];
                _ParaItems[0] = "";
            }

        }
        public void ProcessRequest(HttpContext context)
        {
            this.context = context;
            try
            {
                InitNameFromUrl();
                MethodEntity methodEntity = MethodCollector.GetMethod(_ControllerType, MethodName);
                if (methodEntity == null)
                {
                    //检测全局Default方法
                    MethodEntity globalDefault = MethodCollector.GlobalDefault;
                    if (globalDefault != null)
                    {
                        Controller o = (Controller)Activator.CreateInstance(globalDefault.Method.DeclaringType);//实例化
                        o.ProcessRequest(context);
                    }
                    else
                    {
                        Response.StatusCode = 404;
                        Write("404 Not Found.");
                        WriteExeResult();
                    }
                    return;

                }
                else
                {
                    if (CheckMethodAttributeLimit(methodEntity))
                    {
                        LoadHtmlView();
                        if (ExeBeforeInvoke(methodEntity.AttrEntity.HasIgnoreDefaultController))
                        {
                            if (ExeMethodInvoke(methodEntity))
                            {
                                ExeEndInvoke(methodEntity.AttrEntity.HasIgnoreDefaultController);
                            }
                        }
                    }
                    WriteExeResult();
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                return;
            }
            catch (Exception err)
            {
                StringBuilder sb = new StringBuilder();
                string errMsg = Log.GetExceptionMessage(err);
                sb.AppendLine(errMsg);
                if (err.StackTrace != null)
                {
                    sb.AppendLine(err.StackTrace);
                }
                var headers = Request.Headers;
                if (headers.Count > 0)
                {
                    sb.AppendLine("\n-----------Headers-----------");
                    foreach (string key in headers.AllKeys)
                    {
                        sb.AppendLine(key + " : " + headers[key]);
                    }
                }
                var form = Request.Form;
                if (form.Count > 0)
                {
                    sb.AppendLine("-----------Forms-----------");
                    foreach (string key in form.AllKeys)
                    {
                        sb.AppendLine(key + " : " + form[key]);
                    }
                }
                WriteLog("【Taurus.Core.Controller】：" + sb.ToString());
                if (View == null)
                {
                    errMsg = JsonHelper.OutResult(false, errMsg);
                }
                context.Response.Write(errMsg);
            }

        }

        #region 方法分解

        private bool CheckMethodAttributeLimit(MethodEntity methodEntity)
        {
            AttributeEntity attrEntity = methodEntity.AttrEntity;
            if (!attrEntity.HasWebSocket)
            {
                if (Request.Headers["Connection"] == "Upgrade" && Request.Headers["Upgrade"] == "websocket")
                {
                    Write("Current method not support WebSocket.", false);
                    return false;
                }
            }
            bool isGoOn = true;

            if (!attrEntity.IsAllowHttpMethod(Request.HttpMethod))
            {
                Write("Http method not support " + Request.HttpMethod, false);
                return false;
            }

            if (attrEntity.HasAck)//有[Ack]
            {
                #region Validate CheckAck
                MethodEntity checkAck = MethodCollector.GetMethod(_ControllerType, ReflectConst.CheckAck, false);
                if (checkAck != null)
                {
                    isGoOn = Convert.ToBoolean(checkAck.Method.Invoke(this, new object[] { Query<string>("ack") }));
                }
                else if (!attrEntity.HasIgnoreDefaultController)
                {
                    checkAck = MethodCollector.GlobalCheckAck;
                    if (checkAck != null)
                    {
                        isGoOn = Convert.ToBoolean(checkAck.Method.Invoke(null, new object[] { this, Query<string>("ack") }));
                    }
                }
                if (!isGoOn)
                {
                    Write("Check AckAttribute is illegal.", false);
                    return false;
                }
                #endregion
            }
            if (attrEntity.HasToken)//有[Token]
            {
                #region Validate CheckToken
                MethodEntity checkToken = MethodCollector.GetMethod(_ControllerType, ReflectConst.CheckToken, false);
                if (checkToken != null)
                {
                    isGoOn = Convert.ToBoolean(checkToken.Method.Invoke(this, new object[] { Query<string>("token") }));
                }
                else if (!attrEntity.HasIgnoreDefaultController)
                {
                    checkToken = MethodCollector.GlobalCheckToken;
                    if (checkToken != null)
                    {
                        isGoOn = Convert.ToBoolean(checkToken.Method.Invoke(null, new object[] { this, Query<string>("token") }));
                    }
                }
                if (!isGoOn)
                {
                    Write("Check TokenAttribute is illegal.", false);
                    return false;
                }
                #endregion
            }
            if (attrEntity.HasMicroService)//有[MicroService]
            {
                #region Validate CheckMicroService 【如果开启全局，即需要调整授权机制，则原有局部机制失效。】
                MethodEntity checkMicroService = null;
                if (!attrEntity.HasIgnoreDefaultController)
                {
                    checkMicroService = MethodCollector.GlobalCheckMicroService;
                    if (checkMicroService != null)
                    {
                        isGoOn = Convert.ToBoolean(checkMicroService.Method.Invoke(null, new object[] { this, Query<string>(MsConst.HeaderKey) }));
                    }
                }
                if (isGoOn && checkMicroService == null)
                {
                    checkMicroService = MethodCollector.GetMethod(_ControllerType, ReflectConst.CheckMicroService, false);
                    if (checkMicroService != null)
                    {
                        isGoOn = Convert.ToBoolean(checkMicroService.Method.Invoke(this, new object[] { Query<string>(MsConst.HeaderKey) }));
                    }
                }
                if (!isGoOn)
                {
                    Write("Check MicroServiceAttribute is illegal.", false);
                    return false;
                }
                #endregion
            }

            if (attrEntity.HasRequire)
            {
                RequireAttribute[] requires = methodEntity.AttrEntity.RequireAttributes;
                if (requires != null && requires.Length > 0)
                {
                    foreach (RequireAttribute require in requires)
                    {
                        if (require.paraName.IndexOf(',') > -1)
                        {
                            foreach (String name in require.paraName.Split(','))
                            {
                                if (string.IsNullOrEmpty(Query<string>(name)))
                                {
                                    Write(string.Format(require.emptyTip, name), false);
                                    return false;
                                }
                            }
                        }
                        else if (!RequireValidate(require, Query<string>(require.paraName)))
                        {
                            return false;
                        }
                    }
                }
            }
            return true;
        }
        private bool RequireValidate(RequireAttribute require, string paraValue)
        {
            if (require.paraName.Contains(".") && !string.IsNullOrEmpty(paraValue))//json字集
            {
                paraValue = JsonHelper.GetValue(paraValue, require.paraName);
            }
            if (require.isRequired && string.IsNullOrEmpty(paraValue))
            {
                Write(require.emptyTip, false);
                return false;
            }
            else if (!string.IsNullOrEmpty(require.regex) && !string.IsNullOrEmpty(paraValue))
            {
                if (paraValue.IndexOf('%') > -1)
                {
                    paraValue = HttpUtility.UrlDecode(paraValue);
                }
                if (!Regex.IsMatch(paraValue, require.regex))//如果格式错误
                {
                    Write(require.regexTip, false);
                    return false;
                }
            }

            return true;
        }

        private bool ExeBeforeInvoke(bool isIgnoreGlobal)
        {
            bool isGoOn = true;
            MethodEntity beforeInvoke = null;
            if (!isIgnoreGlobal)
            {
                beforeInvoke = MethodCollector.GlobalBeforeInvoke;
                if (beforeInvoke != null)//先调用全局
                {
                    isGoOn = Convert.ToBoolean(beforeInvoke.Method.Invoke(null, new object[] { this }));
                }
            }
            if (isGoOn)
            {
                beforeInvoke = MethodCollector.GetMethod(_ControllerType, ReflectConst.BeforeInvoke, false);
                if (beforeInvoke != null)
                {
                    isGoOn = Convert.ToBoolean(beforeInvoke.Method.Invoke(this, null));
                }
            }
            return isGoOn;
        }
        private void ExeEndInvoke(bool isIgnoreGlobal)
        {
            #region EndInvoke
            MethodEntity endInvoke = MethodCollector.GetMethod(_ControllerType, ReflectConst.EndInvoke, false);
            if (endInvoke != null)
            {
                endInvoke.Method.Invoke(this, null);
            }
            if (!isIgnoreGlobal)
            {
                endInvoke = MethodCollector.GlobalEndInvoke;
                if (endInvoke != null)
                {
                    endInvoke.Method.Invoke(null, new object[] { this });
                }
            }
            #endregion
        }
        private void LoadHtmlView()
        {
            if (!CancelLoadHtml)
            {
                _View = ViewEngine.Create(HtmlFolderName, MethodName);//这里ControllerName用原始大写，兼容Linux下大小写名称。
                if (_View != null)
                {
                    //追加几个全局标签变量
                    _View.KeyValue.Add("module", ModuleName.ToLower());
                    _View.KeyValue.Add("controller", ControllerName);
                    _View.KeyValue.Add("action", MethodName.ToLower());
                    _View.KeyValue.Add("para", Para.ToLower());
                    _View.KeyValue.Add("httphost", Request.Url.AbsoluteUri.Substring(0, Request.Url.AbsoluteUri.Length - Request.Url.PathAndQuery.Length));
                }
            }
        }
        private bool ExeMethodInvoke(MethodEntity methodEntity)
        {
            object[] paras;
            if (!GetInvokeParas(methodEntity, out paras))
            {
                return false;
            }

            methodEntity.Method.Invoke(this, paras);
            if (IsHttpPost && _View != null && !string.IsNullOrEmpty(BtnName))
            {
                #region Button Invoke
                MethodEntity postBtnMethod = MethodCollector.GetMethod(_ControllerType, _BtnName, false);
                if (postBtnMethod != null)
                {
                    if (!GetInvokeParas(postBtnMethod, out paras))
                    {
                        return false;
                    }
                    postBtnMethod.Method.Invoke(this, paras);
                }
                #endregion
            }
            return true;
        }
        private void WriteExeResult()
        {
            if (string.IsNullOrEmpty(context.Response.Charset))
            {
                context.Response.Charset = "utf-8";
            }
            if (View != null)
            {
                context.Response.Write(View.OutXml);
                View = null;
            }
            else if (apiResult.Length > 0)
            {
                string outResult = apiResult.ToString();
                if (string.IsNullOrEmpty(context.Response.ContentType))
                {
                    context.Response.ContentType = "text/html;charset=" + context.Response.Charset;
                }
                if (context.Response.ContentType.StartsWith("text/html"))
                {
                    if (apiResult[0] == '{' && apiResult[apiResult.Length - 1] == '}')
                    {
                        context.Response.ContentType = "application/json;charset=" + context.Response.Charset;
                    }
                    else if (outResult.StartsWith("<?xml") && apiResult[apiResult.Length - 1] == '>')
                    {
                        context.Response.ContentType = "application/xml;charset=" + context.Response.Charset;
                    }
                }
                context.Response.Write(outResult);
                apiResult = null;
            }
        }
        #endregion


        /// <summary>
        /// Write log to txt
        /// </summary>
        protected void WriteLog(string msg)
        {
            Log.Write(msg, LogType.Taurus);
        }
        public virtual bool BeforeInvoke()
        {
            return true;
        }
        public virtual void EndInvoke()
        {

        }
        public virtual void Default()
        {

        }
        /// <summary>
        /// to stop load view html
        /// <para>是否取消加载Html文件</para>
        /// </summary>
        protected virtual bool CancelLoadHtml
        {
            get
            {
                return false;
            }
        }
        /// <summary>
        /// 默认返回控制器名称，可通过重写重定向到自定义html目录名
        /// </summary>
        /// <returns></returns>
        protected virtual string HtmlFolderName
        {
            get
            {
                return ControllerName;
            }
        }
        /// <summary>
        /// if the result is false will stop invoke method
        /// <para>检测身份是否通过</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckToken(string token)
        {
            return true;
        }
        /// <summary>
        /// if the result is false will stop invoke method
        /// <para>检测请求是否合法</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckAck(string ack)
        {
            return true;
        }

        /// <summary>
        /// if the result is false will stop invoke method
        /// <para>检测微服务间的请求是否合法</para>
        /// </summary>
        /// <returns></returns>
        public virtual bool CheckMicroService(string msKey)
        {
            if (MsConfig.IsServer)
            {
                return MsConfig.Server.RcKey == msKey;
            }
            return MsConfig.Client.RcKey == msKey;
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
        private string _BtnName = string.Empty;
        /// <summary>
        /// 获取当前的点击按钮名称
        /// </summary>
        public string BtnName
        {
            get
            {
                if (string.IsNullOrEmpty(_BtnName))
                {
                    _BtnName = GetBtnName();
                }
                return _BtnName;
            }
        }
        private string GetBtnName()
        {
            var queryString = context.Request.QueryString;
            foreach (string name in queryString)
            {
                if (name != null && name.ToLower().StartsWith("btn"))
                {
                    return name;
                }
            }
            var form = context.Request.Form;
            foreach (string name in form)
            {
                if (name != null && name.ToLower().StartsWith("btn"))
                {
                    return name;
                }
            }
            return string.Empty;
        }
        private bool GetInvokeParas(MethodEntity methodEntity, out object[] paras)
        {
            paras = null;
            #region 增加处理参数支持
            ParameterInfo[] piList = methodEntity.Parameters;
            if (piList != null && piList.Length > 0)
            {
                var files = Request.Files;
                paras = new object[piList.Length];
                for (int i = 0; i < piList.Length; i++)
                {
                    ParameterInfo pi = piList[i];
                    Type t = pi.ParameterType;
                    if (t.Name == "HttpFileCollection")
                    {
                        paras[i] = files;
                        continue;
                    }
                    object value = Query<object>(pi.Name, null);
                    if (value == null)
                    {
                        if (t.IsValueType && t.IsGenericType && t.FullName.StartsWith("System.Nullable"))
                        {
                            continue;
                        }
                        if (t.Name == "HttpPostedFile")
                        {
                            if (files != null && files.Count == 1)
                            {
                                value = files[0];
                            }
                        }

                        else if (piList.Length == 1 && ReflectTool.GetSystemType(ref t) != SysType.Base)//基础值类型
                        {
                            value = GetJson();
                        }
                    }
                    try
                    {
                        //特殊值处理
                        if (t.Name == "HttpPostedFile" && value is string)
                        {
                            if (!string.IsNullOrEmpty(DocConfig.DefaultImg) && Convert.ToString(value) == DocConfig.DefaultImg.ToLower())
                            {
                                paras[i] = DocConfig.DefaultImgHttpPostedFile;
                            }
                        }
                        else
                        {
                            paras[i] = ConvertTool.ChangeType(value, t);//类型转换（基础或实体）
                        }
                    }
                    catch (Exception err)
                    {
                        string typeName = t.Name;
                        if (typeName.StartsWith("Nullable"))
                        {
                            typeName = Nullable.GetUnderlyingType(t).Name;
                        }
                        string outMsg = string.Format("[{0} {1} = {2}]  [Error : {3}]", typeName, pi.Name, value, err.Message);
                        WriteLog(outMsg);
                        Write(outMsg, false);
                        return false;
                    }

                }
            }
            #endregion
            return true;
        }
        //private bool GetInvokeParas(MethodInfo method, out object[] paras)
        //{
        //    paras = null;
        //    #region 增加处理参数支持
        //    ParameterInfo[] piList = method.GetParameters();
        //    object[] validateList = method.GetCustomAttributes(typeof(RequireAttribute), true);
        //    if (piList != null && piList.Length > 0)
        //    {
        //        paras = new object[piList.Length];
        //        for (int i = 0; i < piList.Length; i++)
        //        {
        //            ParameterInfo pi = piList[i];
        //            Type t = pi.ParameterType;
        //            if (t.Name == "HttpFileCollection")
        //            {
        //                paras[i] = Request.Files;
        //                if (!ValidateParas(validateList, pi.Name, (Request.Files != null && Request.Files.Count > 0) ? "1" : null))
        //                {
        //                    return false;
        //                }
        //                continue;
        //            }
        //            object value = Query<object>(pi.Name, null);
        //            if (value == null)
        //            {
        //                if (t.IsValueType && t.IsGenericType && t.FullName.StartsWith("System.Nullable"))
        //                {
        //                    continue;
        //                }
        //                if (t.Name == "HttpPostedFile")
        //                {
        //                    if (Request.Files != null && Request.Files.Count == 1)
        //                    {
        //                        value = Request.Files[0];
        //                    }
        //                }

        //                else if (piList.Length == 1 && ReflectTool.GetSystemType(ref t) != SysType.Base)//基础值类型
        //                {
        //                    value = GetJson();
        //                }
        //            }
        //            //检测是否允许为空，是否满足正则格式。
        //            if (!ValidateParas(validateList, pi.Name, Convert.ToString(value)))
        //            {
        //                return false;
        //            }
        //            try
        //            {
        //                //特殊值处理
        //                if (t.Name == "HttpPostedFile" && value is string && Convert.ToString(value) == DocSettings.DocDefaultImg.ToLower())
        //                {
        //                    string path = DocSettings.DefaultImg;
        //                    if (!string.IsNullOrEmpty(path))
        //                    {
        //                        paras[i] = HttpPostedFileExtend.Create(path);
        //                    }
        //                }
        //                else
        //                {
        //                    paras[i] = QueryTool.ChangeType(value, t);//类型转换（基础或实体）
        //                }
        //            }
        //            catch (Exception err)
        //            {
        //                string typeName = t.Name;
        //                if (typeName.StartsWith("Nullable"))
        //                {
        //                    typeName = Nullable.GetUnderlyingType(t).Name;
        //                }
        //                string outMsg = string.Format("[{0} {1} = {2}]  [Error : {3}]", typeName, pi.Name, value, err.Message);
        //                WriteLog(outMsg);
        //                Write(outMsg, false);
        //                return false;
        //            }

        //        }
        //    }
        //    //对未验证过的参数，再进行一次验证。
        //    foreach (object item in validateList)
        //    {
        //        RequireAttribute valid = item as RequireAttribute;
        //        if (!valid.isValidated)
        //        {
        //            if (valid.paraName.IndexOf(',') > -1)
        //            {
        //                foreach (string name in valid.paraName.Split(','))
        //                {
        //                    if (string.IsNullOrEmpty(Query<string>(name)))
        //                    {
        //                        Write(string.Format(valid.emptyTip, name), false);
        //                        return false;
        //                    }
        //                }
        //            }
        //            else if (!ValidateParas(validateList, valid.paraName, Query<string>(valid.paraName)))
        //            {
        //                return false;
        //            }
        //        }
        //    }
        //    validateList = null;
        //    #endregion
        //    return true;
        //}
        //private bool ValidateParas(object[] validateList, string paraName, string paraValue)
        //{
        //    if (validateList != null)
        //    {
        //        foreach (object item in validateList)
        //        {
        //            RequireAttribute valid = item as RequireAttribute;
        //            if (!valid.isValidated && (valid.paraName == paraName || valid.paraName.StartsWith(paraName + ".")))
        //            {
        //                valid.isValidated = true;//设置已经验证过此参数，后续可以跳过。
        //                if (valid.paraName.StartsWith(paraName + ".") && !string.IsNullOrEmpty(paraValue))//json字集
        //                {
        //                    paraValue = JsonHelper.GetValue(paraValue, valid.paraName.Substring(paraName.Length + 1));
        //                }
        //                if (valid.isRequired && string.IsNullOrEmpty(paraValue))
        //                {
        //                    Write(valid.emptyTip, false);
        //                    return false;
        //                }
        //                else if (!string.IsNullOrEmpty(valid.regex) && !string.IsNullOrEmpty(paraValue))
        //                {
        //                    if (paraValue.IndexOf('%') > -1)
        //                    {
        //                        paraValue = HttpUtility.UrlDecode(paraValue);
        //                    }
        //                    if (!Regex.IsMatch(paraValue, valid.regex))//如果格式错误
        //                    {
        //                        Write(valid.regexTip, false);
        //                        return false;
        //                    }
        //                }
        //            }
        //        }

        //    }

        //    return true;
        //}
    }
    public abstract partial class Controller
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

        private string _ModuleName = "";
        /// <summary>
        /// 请求路径中的：模块名称。
        /// </summary>
        public string ModuleName
        {
            get
            {
                return _ModuleName;
            }
        }
        private string _ControllerName = "";
        /// <summary>
        /// 请求路径中的：控制器名称。
        /// </summary>
        public string ControllerName
        {
            get
            {
                return _ControllerName;
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
        private string _MethodName = "";
        /// <summary>
        /// 请求路径中的：方法名称。
        /// </summary>
        public string MethodName
        {
            get
            {
                return _MethodName;
            }
        }
        private string[] _ParaItems;
        internal string[] ParaItems
        {
            get
            {
                return _ParaItems;
            }
        }
        /// <summary>
        ///请求路径中的：参数的第一个值。
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
        /// 缓存参数值，内部字典（Query方法可查。）
        /// </summary>
        private MDictionary<string, string> queryCache = new MDictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        static string[] autoPrefixs = (string.IsNullOrEmpty(AppConfig.UI.AutoPrefixs) ? "" : ("," + AppConfig.UI.AutoPrefixs)).Split(',');
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
                return WebTool.ChangeValueType<T>(queryCache[key], defaultValue, false);
            }
            var files = context.Request.Files;
            var headers = context.Request.Headers;
            T value = default(T);
            foreach (string pre in autoPrefixs)
            {
                string newKey = pre + key;
                if (context.Request[newKey] == null && (files == null || files[newKey] == null))
                {
                    //尝试从Json中获取
                    string result = JsonHelper.GetValue(GetJson(), newKey);
                    if (!string.IsNullOrEmpty(result))
                    {
                        value = WebTool.ChangeValueType<T>(result, defaultValue, false);
                        break;
                    }
                    else if (headers[newKey] != null)
                    {
                        value = WebTool.ChangeValueType<T>(headers[newKey], defaultValue, false);
                        break;
                    }
                    else
                    {
                        value = defaultValue;
                    }
                }
                else
                {
                    value = WebTool.Query<T>(newKey, defaultValue, false);//这里不设置默认值(值类型除外）
                    break;
                }

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
                return WebTool.ChangeValueType<T>(ParaItems[paraIndex], defaultValue, false);
            }
            return defaultValue;
        }
        /// <summary>
        /// 自己构造请求参数(Query方法可查，优先级最高）
        /// </summary>
        /// <param name="name">名称</param>
        /// <param name="value">请求值</param>
        public void SetQuery(string name, string value)
        {
            if (queryCache.ContainsKey(name))
            {
                queryCache[name] = value;
            }
            else
            {
                queryCache.Add(name, value);
            }
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
            if (obj is byte[])
            {
                context.Response.BinaryWrite(obj as byte[]);
            }
            else
            {
                Write(JsonHelper.ToJson(obj));
            }
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
                    var form = context.Request.Form;
                    var files = context.Request.Files;
                    if (form.Count > 0)
                    {
                        if (form.Count == 1 && form.Keys[0] == null)
                        {
                            return JsonHelper.ToJson(form[0]);
                        }
                        _Json = JsonHelper.ToJson(form);
                    }
                    else if (files == null || files.Count == 0)//请求头忘了带Http Type
                    {
                        Stream stream = Context.Request.InputStream;
                        if (stream != null && stream.CanRead)
                        {
                            long len = (long)Context.Request.ContentLength;
                            if (len > 0)
                            {
                                Byte[] bytes = new Byte[len];
                                // ////NetCore 3.0 会抛异常，可配置可以同步请求读取流数据
                                //services.Configure<KestrelServerOptions>(x => x.AllowSynchronousIO = true)
                                //    .Configure<IISServerOptions>(x => x.AllowSynchronousIO = true);
                                stream.Position = 0;// 需要启用：context.Request.EnableBuffering();
                                stream.Read(bytes, 0, bytes.Length);
                                if (stream.Position < len)
                                {
                                    //Linux CentOS-8 大文件下读不全，会延时，导致：Unexpected end of Stream, the content may have already been read by another component.
                                    int max = 0;
                                    int timeout = MsConfig.Server.GatewayTimeout * 1000;
                                    while (stream.Position < len)
                                    {
                                        max++;
                                        if (max > timeout)//60秒超时
                                        {
                                            break;
                                        }
                                        Thread.Sleep(1);
                                        stream.Read(bytes, (int)stream.Position, (int)(len - stream.Position));
                                    }
                                }
                                stream.Position = 0;//重置，允许重复使用。
                                string data = System.Text.Encoding.UTF8.GetString(bytes);
                                if (data.IndexOf("%") > -1)
                                {
                                    data = HttpUtility.UrlDecode(data);
                                }
                                _Json = JsonHelper.ToJson(data);
                            }
                        }
                    }
                }
                else if (IsHttpGet)
                {
                    string para = Context.Request.Url.Query.TrimStart('?');
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
    }
}
