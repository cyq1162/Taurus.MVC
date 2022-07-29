using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
namespace Taurus.Core
{
    /// <summary>
    /// 反射Controller类
    /// </summary>
    internal static class InvokeLogic
    {
        public class Const
        {
            internal const string Default = "Default";
            internal const string Controller = "Controller";
            internal const string DefaultController = "DefaultController";
            internal const string DocController = "DocController";
            internal const string AuthController = "AuthController";
            internal const string TaurusCoreController = "Taurus.Core.Controller";
            internal const string MicroServiceController = "MicroServiceController";

            internal const string Doc = "Doc";
            internal const string Auth = "Auth";
            internal const string MicroService = "MicroService";
            internal const string CoreDoc = "Core.Doc";
            internal const string CoreAuth = "Core.Auth";
            internal const string CoreMicroService = "Core.MicroService";
            internal const string Proxy = "Proxy";//MicroService.Proxy

            internal const string CheckToken = "CheckToken";
            internal const string CheckAck = "CheckAck";
            internal const string CheckMicroService = "CheckMicroService";
            internal const string RouteMapInvoke = "RouteMapInvoke";
            internal const string BeforeInvoke = "BeforeInvoke";
            internal const string EndInvoke = "EndInvoke";
            internal const string Record = "Record";

            internal const string TokenAttribute = "TokenAttribute";
            internal const string AckAttribute = "AckAttribute";
            internal const string MicroServiceAttribute = "MicroServiceAttribute";
            internal const string HttpGetAttribute = "HttpGetAttribute";
            internal const string HttpPostAttribute = "HttpPostAttribute";
            internal const string HttpHeadAttribute = "HttpHeadAttribute";
            internal const string HttpPutAttribute = "HttpPutAttribute";
            internal const string HttpDeleteAttribute = "HttpDeleteAttribute";

            internal const string NeedConfigController = "Please make sure config appsettings : add key=\"Taurus.Controllers\" value=\"YourControllerProjectName\" is right!";

            internal static bool IsStartDoc
            {
                get
                {
                    return AppConfig.GetAppBool(AppSettings.IsStartDoc, false);
                }
            }

            internal static bool IsStartAuth
            {
                get
                {
                    return !string.IsNullOrEmpty(AppConfig.GetApp(AppSettings.Auth));
                }
            }
        }

        #region GetAssembly
        private static string _DllNames;
        /// <summary>
        /// 控制器名称（多个时逗号分隔）
        /// </summary>
        public static string DllNames
        {
            get
            {
                if (string.IsNullOrEmpty(_DllNames))
                {
                    _DllNames = AppConfig.GetApp(AppSettings.Controllers, "");
                    if (string.IsNullOrEmpty(_DllNames))
                    {
                        string[] files = Directory.GetFiles(AppConfig.AssemblyPath, "*Controllers.dll", SearchOption.AllDirectories);
                        if (files == null || files.Length == 0)
                        {
                            files = Directory.GetFiles(AppConfig.AssemblyPath, "*.dll", SearchOption.TopDirectoryOnly);//没有配置，搜索所有的dll。
                        }
                        if (files != null)
                        {
                            foreach (string file in files)
                            {
                                _DllNames += Path.GetFileNameWithoutExtension(file) + ",";
                            }
                            _DllNames = _DllNames.TrimEnd(',');
                        }
                    }
                }
                return _DllNames;
            }
        }
        private static List<Assembly> _Assemblys;
        public static List<Assembly> GetAssemblys()
        {
            if (_Assemblys == null)
            {
                string[] dllItems = DllNames.Split(',');
                _Assemblys = new List<Assembly>(dllItems.Length);
                foreach (string item in dllItems)
                {
                    _Assemblys.Add(Assembly.Load(item)); // 可直接抛异常。
                }
                //try
                //{
                //_Assemblys = 
                //}
                //catch (Exception err)
                //{
                //    Log.WriteLogToTxt(err);
                //}
            }
            return _Assemblys;
        }
        //public static string GetClassFullName(string className)
        //{
        //    return DllName + "." + className;
        //}
        #endregion

        #region GetControllers
        /// <summary>
        /// 存档一级名称的控制器[Controller]
        /// </summary>
        private static Dictionary<string, Type> _Lv1Controllers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 存档二级名称的控制器[Module.Controller]
        /// </summary>
        private static Dictionary<string, Type> _Lv2Controllers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static readonly object objLock = new object();
        /// <summary>
        /// 获取控制器
        /// </summary>
        public static Dictionary<string, Type> GetControllers(int level)
        {
            if (_Lv1Controllers.Count == 0)
            {
                lock (objLock)
                {
                    if (_Lv1Controllers.Count == 0)
                    {
                        List<Assembly> assList = GetAssemblys();
                        if (assList == null)
                        {
                            throw new Exception(Const.NeedConfigController);
                        }
                        foreach (Assembly ass in assList)
                        {
                            Type[] typeList = ass.GetExportedTypes();
                            foreach (Type type in typeList)
                            {
                                if (type.Name.EndsWith(Const.Controller))
                                {
                                    //三层继承判断，应该够用了。
                                    if (type.BaseType != null && (type.BaseType.FullName == Const.TaurusCoreController
                                        || (type.BaseType.BaseType != null && (type.BaseType.BaseType.FullName == Const.TaurusCoreController
                                        || (type.BaseType.BaseType.BaseType != null && (type.BaseType.BaseType.BaseType.FullName == Const.TaurusCoreController
                                        || (type.BaseType.BaseType.BaseType.BaseType != null && type.BaseType.BaseType.BaseType.BaseType.FullName == Const.TaurusCoreController)
                                        ))
                                         ))
                                         ))
                                    {
                                        string lv1Name = GetLevelName(type.FullName, 1);
                                        string lv2Name = GetLevelName(type.FullName, 2);
                                        if (!_Lv1Controllers.ContainsKey(lv1Name))
                                        {
                                            _Lv1Controllers.Add(lv1Name, type);
                                        }
                                        else
                                        {
                                            int value = string.Compare(lv2Name, GetLevelName(_Lv1Controllers[lv1Name].FullName, 2), true);
                                            if (value == -1)
                                            {
                                                _Lv1Controllers[lv1Name] = type;//值小的优化。
                                            }
                                        }
                                        if (!_Lv2Controllers.ContainsKey(lv2Name))
                                        {
                                            _Lv2Controllers.Add(lv2Name, type);
                                        }
                                    }
                                }
                            }
                        }
                        //追加APIHelp
                        if (Const.IsStartDoc)
                        {
                            if (!_Lv1Controllers.ContainsKey(Const.Doc))
                            {
                                _Lv1Controllers.Add(Const.Doc, typeof(Taurus.Core.DocController));
                            }
                            if (!_Lv2Controllers.ContainsKey(Const.CoreDoc))
                            {
                                _Lv2Controllers.Add(Const.CoreDoc, typeof(Taurus.Core.DocController));
                            }
                        }
                        if (Const.IsStartAuth)
                        {
                            if (!_Lv1Controllers.ContainsKey(Const.Auth))
                            {
                                _Lv1Controllers.Add(Const.Auth, typeof(Taurus.Core.AuthController));
                            }
                            if (!_Lv2Controllers.ContainsKey(Const.CoreAuth))
                            {
                                _Lv2Controllers.Add(Const.CoreAuth, typeof(Taurus.Core.AuthController));
                            }
                        }
                        //微服务API
                        if (!_Lv1Controllers.ContainsKey(Const.MicroService))
                        {
                            _Lv1Controllers.Add(Const.MicroService, typeof(Taurus.Core.MicroServiceController));
                        }
                        if (!_Lv2Controllers.ContainsKey(Const.CoreMicroService))
                        {
                            _Lv2Controllers.Add(Const.CoreMicroService, typeof(Taurus.Core.MicroServiceController));
                        }
                    }
                }
            }
            return level == 1 ? _Lv1Controllers : _Lv2Controllers;
        }
        /// <summary>
        /// 存档N级名称（Module.Controller)
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private static string GetLevelName(string fullName, int level)
        {
            string[] items = fullName.Split('.');
            string lv1Name = items[items.Length - 1].Replace(Const.Controller, "");
            if (level == 2)
            {
                return items[items.Length - 2] + "." + lv1Name;
            }
            return lv1Name;
        }
        /// <summary>
        /// 通过className类名获得对应的Controller类
        /// </summary>
        /// <returns></returns>
        public static Type GetController(string className)
        {
            if (string.IsNullOrEmpty(className))
            {
                className = Const.Default;
            }
            Dictionary<string, Type> controllers = GetControllers(1);
            string[] names = className.Split('.');//home/index
            if (RouteConfig.RouteMode == 1 || names.Length == 1)
            {
                if (controllers.ContainsKey(names[0]))
                {
                    return controllers[names[0]];
                }
                if (names.Length > 1 && controllers.ContainsKey(names[1]))
                {
                    return controllers[names[1]];
                }
            }
            else if (RouteConfig.RouteMode == 2)
            {
                Dictionary<string, Type> controllers2 = GetControllers(2);
                if (controllers2.ContainsKey(className))
                {
                    return controllers2[className];
                }
                //再查一级路径
                if (controllers.ContainsKey(names[1]))
                {
                    return controllers[names[1]];
                }
                //兼容【路由1=》（变更为）2】
                if (controllers.ContainsKey(names[0]))
                {
                    return controllers[names[0]];
                }
            }

            if (controllers.ContainsKey(Const.Default))
            {
                return controllers[Const.Default];
            }
            return null;
        }

        #endregion

        #region GetMethods

        #region 4个全局方法

        private static MethodInfo _DefaultCheckAck = null;
        /// <summary>
        /// 全局DefaultCheckAck方法
        /// </summary>
        public static MethodInfo DefaultCheckAck
        {
            get
            {
                if (_DefaultCheckAck == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _DefaultCheckAck = t.GetMethod(Const.CheckAck, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _DefaultCheckAck;
            }
        }
        private static MethodInfo _DefaultCheckMicroService = null;
        /// <summary>
        /// 全局DefaultCheckMicroService方法
        /// </summary>
        public static MethodInfo DefaultCheckMicroService
        {
            get
            {
                if (_DefaultCheckMicroService == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _DefaultCheckMicroService = t.GetMethod(Const.CheckMicroService, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _DefaultCheckMicroService;
            }
        }

        private static MethodInfo _DefaultCheckToken = null;
        /// <summary>
        /// 全局CheckToken方法
        /// </summary>
        public static MethodInfo DefaultCheckToken
        {
            get
            {
                if (_DefaultCheckToken == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _DefaultCheckToken = t.GetMethod(Const.CheckToken, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _DefaultCheckToken;
            }
        }
        private static MethodInfo _BeforeInvokeMethod = null;
        /// <summary>
        ///  全局BeforeInvoke方法
        /// </summary>
        public static MethodInfo BeforeInvokeMethod
        {
            get
            {
                if (_BeforeInvokeMethod == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _BeforeInvokeMethod = t.GetMethod(Const.BeforeInvoke, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _BeforeInvokeMethod;
            }
        }
        private static MethodInfo _RouteMapInvokeMethod = null;
        /// <summary>
        ///  全局BeforeInvoke方法
        /// </summary>
        public static MethodInfo RouteMapInvokeMethod
        {
            get
            {
                if (_RouteMapInvokeMethod == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _RouteMapInvokeMethod = t.GetMethod(Const.RouteMapInvoke, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _RouteMapInvokeMethod;
            }
        }
        private static MethodInfo _EndInvokeMethod = null;
        /// <summary>
        ///  全局EndInvokeMethod方法
        /// </summary>
        public static MethodInfo EndInvokeMethod
        {
            get
            {
                if (_EndInvokeMethod == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _EndInvokeMethod = t.GetMethod(Const.EndInvoke, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _EndInvokeMethod;
            }
        }

        private static MethodInfo _AuthCheckToken = null;
        /// <summary>
        /// 默认AuthController.CheckToken方法
        /// </summary>
        public static MethodInfo AuthCheckToken
        {
            get
            {
                if (_AuthCheckToken == null && Const.IsStartAuth)
                {
                    Type t = GetController(Const.Auth);
                    if (t != null)
                    {
                        _AuthCheckToken = t.GetMethod(Const.CheckToken, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _AuthCheckToken;
            }
        }
        private static MethodInfo _DocRecord = null;
        /// <summary>
        /// 默认DocController.Record方法(用于记录所有的请求及参数，后期实现批量执行测试结果)
        /// </summary>
        public static MethodInfo DocRecord
        {
            get
            {
                if (_DocRecord == null && Const.IsStartDoc)
                {
                    Type t = GetController(Const.Doc);
                    if (t != null)
                    {
                        _DocRecord = t.GetMethod(Const.Record, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _DocRecord;
            }
        }
        #endregion
        static Dictionary<string, Dictionary<string, MethodInfo>> typeMethods = new Dictionary<string, Dictionary<string, MethodInfo>>();
        static Dictionary<string, AttributeList> methodAttrs = new Dictionary<string, AttributeList>(StringComparer.OrdinalIgnoreCase);

        static readonly object methodObj = new object();
        internal static MethodInfo GetMethod(Type t, string methodName)
        {
            AttributeList hasTokenAttr;
            return GetMethod(t, methodName, out hasTokenAttr);
        }
        internal static MethodInfo GetMethod(Type t, string methodName, out AttributeList attrFlags)
        {
            string key = t.FullName;
            Dictionary<string, MethodInfo> dic = null;
            attrFlags = null;
            if (!typeMethods.ContainsKey(key))
            {
                lock (methodObj)
                {
                    if (!typeMethods.ContainsKey(key))
                    {
                        bool hasToken = t.GetCustomAttributes(typeof(TokenAttribute), true).Length > 0;
                        bool hasAck = t.GetCustomAttributes(typeof(AckAttribute), true).Length > 0;
                        bool hasMicroService = t.GetCustomAttributes(typeof(MicroServiceAttribute), true).Length > 0;
                        if (hasToken || hasAck || hasMicroService)
                        {
                            AttributeList al = new AttributeList();
                            al.HasToken = hasToken;
                            al.HasAck = hasAck;
                            al.HasMicroService = hasMicroService;
                            methodAttrs.Add(key, al);
                        }
                        MethodInfo[] items = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                        dic = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
                        foreach (MethodInfo item in items)
                        {
                            if (!dic.ContainsKey(item.Name))//对于重载的同名方法，只取第一个方法。
                            {
                                dic.Add(item.Name, item);//追加方法名
                                object[] attrs = item.GetCustomAttributes(true);
                                if (attrs.Length > 0)//追加特性名
                                {
                                    AttributeList aFlags = new AttributeList();
                                    foreach (object attr in attrs)
                                    {
                                        string[] names = attr.ToString().Split('.');
                                        switch (names[names.Length - 1])
                                        {
                                            case Const.TokenAttribute:
                                                aFlags.HasToken = true; break;
                                            case Const.AckAttribute:
                                                aFlags.HasAck = true; break;
                                            case Const.MicroServiceAttribute:
                                                aFlags.HasMicroService = true; break;
                                            case Const.HttpGetAttribute:
                                                aFlags.HasGet = true; break;
                                            case Const.HttpPostAttribute:
                                                aFlags.HasPost = true; break;
                                            case Const.HttpHeadAttribute:
                                                aFlags.HasHead = true; break;
                                            case Const.HttpPutAttribute:
                                                aFlags.HasPut = true; break;
                                            case Const.HttpDeleteAttribute:
                                                aFlags.HasDelete = true; break;
                                        }

                                    }
                                    methodAttrs.Add(key + "." + item.Name, aFlags);
                                }
                            }
                        }
                        typeMethods.Add(key, dic);
                    }
                }
            }
            dic = typeMethods[key];
            if (!dic.ContainsKey(methodName))
            {
                methodName = Const.Default;
            }
            if (methodAttrs.ContainsKey(key + "." + methodName))
            {
                attrFlags = methodAttrs[key + "." + methodName];
            }
            if (attrFlags == null)
            {
                attrFlags = new AttributeList();
            }
            if (methodAttrs.ContainsKey(key))//如果类级别有，所有方法都继承
            {
                AttributeList al = methodAttrs[key];
                attrFlags.HasToken = attrFlags.HasToken || al.HasToken;
                attrFlags.HasAck = attrFlags.HasAck || al.HasAck;
                attrFlags.HasMicroService = attrFlags.HasMicroService || al.HasMicroService;
            }
            if (dic.ContainsKey(methodName))
            {
                return dic[methodName];
            }
            return null;
        }
        #endregion
    }
}
