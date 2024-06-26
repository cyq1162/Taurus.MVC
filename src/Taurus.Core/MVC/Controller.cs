﻿using System;
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
using Taurus.Mvc.Reflect;
using CYQ.Data.Json;
using System.Diagnostics;
using System.Collections.Generic;

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
        /// 获取【或重写】待发送的缓冲区的数据
        /// </summary>
        public virtual string APIResult
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
            _MethodName = methodName.Split('.')[0];//去后缀。
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
                if (MvcConfig.IsPrintRequestSql)
                {
                    AppDebug.Start();
                }

                InitNameFromUrl();

                MethodEntity methodEntity = MethodCollector.GetMethod(_ControllerType, MethodName);

                if (methodEntity == null)
                {
                    //检测全局Default方法
                    MethodEntity globalDefault = MethodCollector.GlobalDefault;
                    if (globalDefault != null)
                    {
                        Controller o = globalDefault.TypeEntity.Delegate.CreateController();
                        if (o != null)
                        {
                            o.ProcessRequest(context);
                        }
                    }
                    return;
                }
                else
                {
                    //Stopwatch sw = Stopwatch.StartNew();
                    if (CheckMethodAttributeLimit(methodEntity))
                    {
                        //sw.Stop();
                        //Console.WriteLine("CheckMethodAttributeLimit : " + sw.ElapsedTicks);
                        //sw.Restart();
                        LoadHtmlView();
                        //sw.Stop();
                        //Console.WriteLine("LoadHtmlView : " + sw.ElapsedTicks);
                        //sw.Restart();
                        if (ExeBeforeInvoke(methodEntity.AttrEntity.HasIgnoreGlobalController))
                        {
                            //sw.Stop();
                            //Console.WriteLine("ExeBeforeInvoke : " + sw.ElapsedTicks);
                            //sw.Restart();
                            if (ExeMethodInvoke(methodEntity))
                            {
                                //sw.Stop();
                                //Console.WriteLine("ExeMethodInvoke : " + sw.ElapsedTicks);
                                ExeEndInvoke(methodEntity.AttrEntity.HasIgnoreGlobalController);
                            }
                        }
                    }
                    //sw.Restart();
                    WriteExeResult();
                    //sw.Stop();
                    //Console.WriteLine("WriteExeResult Total : " + sw.ElapsedTicks);
                }
            }
            catch (System.Threading.ThreadAbortException)
            {
                return;
            }
            catch (Exception err)
            {
                string outErr = err.Message;
                WebTool.PrintRequestLog(context, err);
                MethodEntity onError = MethodCollector.GlobalOnError;
                if (onError != null)
                {
                    object result = onError.Method.Invoke(null, new object[] { context, err });
                    if (result != null && result is string)
                    {
                        outErr = result.ToString();
                    }
                }
                if (!string.IsNullOrEmpty(outErr))
                {
                    context.Response.Write(outErr);
                }
            }
            finally
            {
                if (MvcConfig.IsPrintRequestSql)
                {
                    string info = AppDebug.Info;
                    if (!string.IsNullOrEmpty(info))
                    {
                        Log.WriteLogToTxt(AppDebug.Info, LogType.Debug + "_PrintRequestSql");
                    }
                    AppDebug.Stop();
                }
            }

        }

        #region 方法分解

        private bool CheckMethodAttributeLimit(MethodEntity methodEntity)
        {
            AttributeEntity attrEntity = methodEntity.AttrEntity;
            if (!attrEntity.HasWebSocket)
            {
                if (Request.GetHeader("Upgrade") == "websocket")
                {
                    if (Request.GetHeader("Connection") == "Upgrade")
                    {
                        Write("Current method not support WebSocket.", false);
                        return false;
                    }
                }
            }
            bool isGoOn = true;

            if (!methodEntity.IsAllowHttpMethod(Request.HttpMethod))
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
                    //checkAck.MethodDelegate.DynamicInvoke(null, null);
                    isGoOn = Convert.ToBoolean(checkAck.Delegate.Invoke(this, new object[] { Query<string>("ack") }));
                }
                else if (!attrEntity.HasIgnoreGlobalController)
                {
                    checkAck = MethodCollector.GlobalCheckAck;
                    if (checkAck != null)
                    {
                        isGoOn = Convert.ToBoolean(checkAck.Delegate.Invoke(null, new object[] { this, Query<string>("ack") }));
                    }
                }
                if (!isGoOn)
                {
                    if (apiResult.Length == 0)
                    {
                        Write("Check AckAttribute is illegal.", false);
                    }
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
                    isGoOn = Convert.ToBoolean(checkToken.Delegate.Invoke(this, new object[] { Query<string>("token") }));
                }
                else if (!attrEntity.HasIgnoreGlobalController)
                {
                    checkToken = MethodCollector.GlobalCheckToken;
                    if (checkToken != null)
                    {
                        isGoOn = Convert.ToBoolean(checkToken.Delegate.Invoke(null, new object[] { this, Query<string>("token") }));
                    }
                }
                if (!isGoOn)
                {
                    if (apiResult.Length == 0)
                    {
                        Write("Check TokenAttribute is illegal.", false);
                    }
                    return false;
                }
                #endregion
            }
            if (attrEntity.HasMicroService)//有[MicroService]
            {
                #region Validate CheckMicroService 【如果开启全局，即需要调整授权机制，则原有局部机制失效。】
                MethodEntity checkMicroService = null;
                if (!attrEntity.HasIgnoreGlobalController)
                {
                    checkMicroService = MethodCollector.GlobalCheckMicroService;
                    if (checkMicroService != null)
                    {
                        isGoOn = Convert.ToBoolean(checkMicroService.Delegate.Invoke(null, new object[] { this, Query<string>(MsConst.HeaderKey) }));
                    }
                }
                if (isGoOn && checkMicroService == null)
                {
                    checkMicroService = MethodCollector.GetMethod(_ControllerType, ReflectConst.CheckMicroService, false);
                    if (checkMicroService != null)
                    {
                        isGoOn = Convert.ToBoolean(checkMicroService.Delegate.Invoke(this, new object[] { Query<string>(MsConst.HeaderKey) }));
                    }
                }
                if (!isGoOn)
                {
                    if (apiResult.Length == 0)
                    {
                        Write("Check MicroServiceAttribute is illegal.", false);
                    }
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
                    isGoOn = Convert.ToBoolean(beforeInvoke.Delegate.Invoke(null, new object[] { this }));
                }
            }
            if (isGoOn)
            {
                beforeInvoke = MethodCollector.GetMethod(_ControllerType, ReflectConst.BeforeInvoke, false);
                if (beforeInvoke != null)
                {
                    isGoOn = Convert.ToBoolean(beforeInvoke.Delegate.Invoke(this, null));
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
                endInvoke.Delegate.Invoke(this, null);
            }
            if (!isIgnoreGlobal)
            {
                endInvoke = MethodCollector.GlobalEndInvoke;
                if (endInvoke != null)
                {
                    endInvoke.Delegate.Invoke(null, new object[] { this });
                }
            }
            #endregion
        }
        private void LoadHtmlView()
        {
            if (IsLoadHtml)
            {
                _View = ViewEngine.Create(HtmlFolderName, HtmlFileName, IsLoadHtmlWithReadOnly);//这里ControllerName用原始大写，兼容Linux下大小写名称。
                if (_View != null)
                {
                    //追加几个全局标签变量
                    _View.KeyValue.Add("module", ModuleName.ToLower());
                    _View.KeyValue.Add("controller", ControllerName);
                    _View.KeyValue.Add("action", MethodName.ToLower());
                    _View.KeyValue.Add("para", Para.ToLower());
                    var url = Request.Url;
                    _View.KeyValue.Add("suffix", Path.GetExtension(url.LocalPath));
                    _View.KeyValue.Add("httphost", url.AbsoluteUri.Substring(0, url.AbsoluteUri.Length - url.PathAndQuery.Length));
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
            //执行页面方法
            object result = methodEntity.Delegate.Invoke(this, paras);
            if (result != null && apiResult.Length == 0)
            {
                var str = result.ToString();
                if (result is string)
                {
                    Write(str);
                }
                else
                {
                    bool isTask = str.StartsWith("System.Threading.") || str.StartsWith("System.Runtime.");
                    //跳过异步方法。
                    if (!isTask)
                    {
                        Write(result);
                    }
                }
            }
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
                    //执行页面控制点击事件
                    postBtnMethod.Delegate.Invoke(this, paras);
                }
                #endregion
            }
            return true;
        }
        private void WriteExeResult()
        {

            //if (string.IsNullOrEmpty(context.Response.Charset))
            //{
            //    context.Response.Charset = "utf-8";
            //}

            if (View != null)
            {
                if (string.IsNullOrEmpty(Response.ContentType))
                {
                    Response.ContentType = "text/html; charset=utf-8";
                }
                //Stopwatch sw = Stopwatch.StartNew();
                var html = View.OutXml;
                //sw.Stop();
                //Console.WriteLine("Get Html : " + sw.ElapsedTicks);
                Response.Write(html);
                //context.Response.Write(View.OutXml);
                View = null;

            }
            else
            {
                string outResult = APIResult;
                if (outResult.Length > 0)
                {
                    var ct = Response.ContentType;
                    if (string.IsNullOrEmpty(ct) || ct == "text/html")
                    {
                        var charset = Response.Charset;
                        if ((outResult[0] == '{' && outResult.EndsWith("}")) || (outResult[0] == '[' && outResult.EndsWith("]")))
                        {
                            Response.ContentType = "application/json; charset=" + charset;
                        }
                        else if (outResult.StartsWith("<?xml") && outResult.EndsWith(">"))
                        {
                            Response.ContentType = "application/xml; charset=" + charset;
                        }
                        else
                        {
                            Response.ContentType = "text/html; charset=" + charset;
                        }
                    }
                    //Stopwatch sw = Stopwatch.StartNew();
                    Response.Write(outResult);
                    //sw.Stop();
                    //Console.WriteLine("Response Write : " + sw.ElapsedTicks);
                }

            }
            //else if (context.Response.Body.Length > 0)
            //{
            //    if (string.IsNullOrEmpty(context.Response.ContentType))
            //    {
            //        context.Response.ContentType = "application/octet-stream;charset=" + context.Response.Charset;
            //    }
            //}
        }
        #endregion
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
        /// is allow load html view
        /// <para>是否加载Html文件</para>
        /// </summary>
        protected virtual bool IsLoadHtml
        {
            get
            {
                return true;
            }
        }
        /// <summary>
        /// is load html view with readonly
        /// <para>是否加载只读 Html文件</para>
        /// </summary>
        protected virtual bool IsLoadHtmlWithReadOnly
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
        /// 默认返回方法名称，可通过重写重定向到自定义html文件名
        /// </summary>
        /// <returns></returns>
        protected virtual string HtmlFileName
        {
            get
            {
                return MethodName;
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
            if (context == null || context.Request == null) { return string.Empty; }
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


                paras = new object[piList.Length];
                for (int i = 0; i < piList.Length; i++)
                {
                    ParameterInfo pi = piList[i];
                    Type t = pi.ParameterType;
                    string value = null; ;
                    try
                    {
                        #region 参数获取

                        if (t.IsValueType)
                        {
                            value = Query<string>(pi.Name);
                            paras[i] = ConvertTool.ChangeType(value, t);
                        }
                        else if (t.Name == "String")
                        {
                            paras[i] = Query<string>(pi.Name);
                        }
                        else
                        {
                            switch (t.Name)
                            {
                                case "HttpFileCollection":
                                    paras[i] = Request.Files;
                                    break; ;
                                case "HttpPostedFile":
                                    var files = Request.Files;
                                    if (files != null)
                                    {
                                        if (files.Count > 0)
                                        {
                                            if (files.Count == 1)
                                            {
                                                paras[i] = files[0];
                                            }
                                            else
                                            {
                                                paras[i] = files[pi.Name];
                                            }

                                        }
                                        else
                                        {
                                            var name = Query<string>(pi.Name);
                                            if (!string.IsNullOrEmpty(name) && name == DocConfig.DefaultImg)
                                            {
                                                paras[i] = DocConfig.DefaultImgHttpPostedFile;
                                            }
                                        }
                                    }
                                    break;
                                default:
                                    value = Query<string>(pi.Name);
                                    if (value == null && piList.Length == 1)
                                    {
                                        value = GetJson();
                                    }
                                    paras[i] = ConvertTool.ChangeType(value, t);//类型转换（基础或实体）
                                    break;
                            }
                        }
                        #endregion
                    }
                    catch (Exception err)
                    {
                        string typeName = t.Name;
                        if (typeName.StartsWith("Nullable"))
                        {
                            typeName = Nullable.GetUnderlyingType(t).Name;
                        }
                        string outMsg = string.Format("[{0} {1} = {2}]  [Error : {3}]", typeName, pi.Name, value, err.Message);
                        Log.Write(outMsg, LogType.Taurus);
                        Write(outMsg, false);
                        return false;
                    }
                    //object value = Query<object>(pi.Name, null);
                    //if (value == null)
                    //{
                    //    if (t.IsValueType && t.IsGenericType && t.FullName.StartsWith("System.Nullable"))
                    //    {
                    //        continue;
                    //    }
                    //    if (t.Name == "HttpPostedFile")
                    //    {
                    //        if (files != null && files.Count == 1)
                    //        {
                    //            value = files[0];
                    //        }
                    //    }

                    //    else if (piList.Length == 1 && ReflectTool.GetSystemType(ref t) != SysType.Base)//基础值类型
                    //    {
                    //        value = GetJson();
                    //    }
                    //}
                    //try
                    //{
                    //    //特殊值处理
                    //    if (t.Name == "HttpPostedFile" && value is string)
                    //    {
                    //        if (!string.IsNullOrEmpty(DocConfig.DefaultImg) && Convert.ToString(value) == DocConfig.DefaultImg.ToLower())
                    //        {
                    //            paras[i] = DocConfig.DefaultImgHttpPostedFile;
                    //        }
                    //    }
                    //    else
                    //    {
                    //        paras[i] = ConvertTool.ChangeType(value, t);//类型转换（基础或实体）
                    //    }
                    //}
                    //catch (Exception err)
                    //{
                    //    string typeName = t.Name;
                    //    if (typeName.StartsWith("Nullable"))
                    //    {
                    //        typeName = Nullable.GetUnderlyingType(t).Name;
                    //    }
                    //    string outMsg = string.Format("[{0} {1} = {2}]  [Error : {3}]", typeName, pi.Name, value, err.Message);
                    //    Log.Write(outMsg, LogType.Taurus);
                    //    Write(outMsg, false);
                    //    return false;
                    //}

                }
            }
            #endregion
            return true;
        }

    }
    public abstract partial class Controller
    {
        ///// <summary>
        ///// 类型属性
        ///// </summary>
        //internal TypeEntity typeEntity;

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
                if (ParaItems == null || ParaItems.Length == 0) { return string.Empty; }
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

        #region SetQuery、Query

        /// <summary>
        /// 缓存参数值，内部字典（Query方法可查。）
        /// </summary>
        private Dictionary<string, string> queryCache = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
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
                try
                {
                    queryCache.Add(name, value);
                }
                catch
                {

                }

            }
        }

        public T Query<T>(string key)
        {
            return Query<T>(key, default(T));
        }
        public T Query<T>(string key, T defaultValue)
        {
            if (queryCache.ContainsKey(key))
            {
                return ConvertTool.ChangeType<T>(queryCache[key]);
            }
            T value = WebTool.Query<T>(key, defaultValue, context);
            if (value != null)
            {
                string str = value.ToString();
                if (str.Length > 0)
                {
                    SetQuery(key, str);
                }
            }
            return value;

            //if (context == null || context.Request == null) { return defaultValue; }
            //var files = context.Request.Files;
            //var headers = context.Request.Headers;
            //T value = default(T);
            //foreach (string pre in autoPrefixs)
            //{
            //    string newKey = pre + key;
            //    if (context.Request[newKey] == null && (files == null || files[newKey] == null))
            //    {
            //        //尝试从Json中获取
            //        string result = JsonHelper.GetValue(GetJson(), newKey);
            //        if (!string.IsNullOrEmpty(result))
            //        {
            //            value = WebTool.ChangeValueType<T>(result, defaultValue, false);
            //            break;
            //        }
            //        else if (headers[newKey] != null)
            //        {
            //            value = WebTool.ChangeValueType<T>(headers[newKey], defaultValue, false);
            //            break;
            //        }
            //        else
            //        {
            //            value = defaultValue;
            //        }
            //    }
            //    else
            //    {
            //        value = WebTool.Query<T>(newKey, defaultValue);//这里不设置默认值(值类型除外）
            //        break;
            //    }

            //}
            //return value;
        }
        #endregion
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
            if (obj == null) { return; }
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
            string json = obj == null ? "" : JsonHelper.ToJson(obj);
            Write(json, isSuccess);
        }

        /// <summary>
        /// 响应输出文件
        /// </summary>
        /// <param name="filePath">文件路径</param>
        public void WriteFile(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) { return; }
            if (!filePath.StartsWith(AppConst.WebRootPath))
            {
                filePath = AppConst.WebRootPath + filePath;
            }

            var bytes = IOHelper.ReadAllBytes(filePath);
            if (bytes == null || bytes.Length == 0) { return; }
            Response.AppendHeader("Content-Disposition", "attachment;filename=" + Path.GetFileName(filePath));
            Response.AppendHeader("Content-Length", bytes.Length.ToString());
            Response.AppendHeader("Content-Transfer-Encoding", "binary");
            Response.ContentType = "application/octet-stream;charset=" + Response.Charset;
            Response.BinaryWrite(bytes);
            Response.End();
        }


        /// <summary>
        /// Get entity from post form
        /// <para>从Post过来的数据中获得实体类型的转换</para>
        /// </summary>
        /// <returns></returns>
        public T GetEntity<T>() where T : class
        {
            return JsonHelper.ToEntity<T>(GetJson());
        }
        /// <summary>
        /// 获取Get或Post的数据并转换为Json格式。
        /// </summary>
        /// <returns></returns>
        public string GetJson()
        {
            return WebTool.GetJson(context);
        }
    }
}
