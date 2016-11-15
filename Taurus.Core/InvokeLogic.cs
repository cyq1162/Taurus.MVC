using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Taurus.Core
{
    /// <summary>
    /// 反射Controller类
    /// </summary>
    static class InvokeLogic
    {
        internal const string Default = "Default";
        internal const string Controller = "Controller";
        internal const string DefaultController = "DefaultController";
        internal const string TaurusController = "Taurus.Core.Controller";
        internal const string CheckToken = "CheckToken";

        #region GetAssembly
        private static string _DllName;
        public static string DllName
        {
            get
            {
                if (string.IsNullOrEmpty(_DllName))
                {
                    _DllName = AppConfig.GetApp("Taurus.Controllers", "Taurus.Controllers");
                }
                return _DllName;
            }
        }
        private static Assembly _Assembly;
        public static Assembly GetAssembly()
        {
            if (_Assembly == null)
            {
                //try
                //{
                _Assembly = Assembly.Load(DllName); // 可直接抛异常。
                //}
                //catch (Exception err)
                //{
                //    Log.WriteLogToTxt(err);
                //}
            }
            return _Assembly;
        }
        public static string GetClassFullName(string className)
        {
            return DllName + "." + className;
        }
        #endregion

        #region GetControllers

        private static Dictionary<string, Type> _Controllers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static readonly object objLock = new object();
        /// <summary>
        /// 获取控制器
        /// </summary>
        private static Dictionary<string, Type> GetControllers()
        {
            if (_Controllers.Count == 0)
            {
                lock (objLock)
                {
                    if (_Controllers.Count == 0)
                    {
                        Assembly ass = GetAssembly();
                        if (ass == null)
                        {
                            throw new Exception("Please make sure web.config'appSetting <add key=\"Taurus.Controllers\" value=\"YourControllerProjectName\") is right!");
                        }
                        Type[] typeList = ass.GetExportedTypes();
                        foreach (Type type in typeList)
                        {
                            if (type.Name.EndsWith(Controller))
                            {
                                if (type.BaseType != null && type.BaseType.FullName == TaurusController)
                                {
                                    _Controllers.Add(type.Name.Replace(Controller, ""), type);
                                }
                            }
                        }
                    }
                }
            }
            return _Controllers;
        }
        /// <summary>
        /// 通过className类名获得对应的Controller类
        /// </summary>
        /// <returns></returns>
        public static Type GetType(string className)
        {
            Dictionary<string, Type> controllers = GetControllers();
            if (!string.IsNullOrEmpty(className) && controllers.ContainsKey(className)) //1：完整匹配【名称空间.类名】
            {
                return controllers[className];
            }
            if (controllers.ContainsKey(Default))
            {
                return controllers[Default];
            }
            return null;
        }

        #endregion

        #region GetMethods
        private static MethodInfo _CheckTokenMethod = null;
        public static MethodInfo CheckTokenMethod
        {
            get
            {
                if (_CheckTokenMethod == null)
                {
                    Type t = GetType(DefaultController);
                    if (t != null)
                    {
                        _CheckTokenMethod = t.GetMethod(CheckToken, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _CheckTokenMethod;
            }
        }
        static Dictionary<string, Dictionary<string, MethodInfo>> typeMethods = new Dictionary<string, Dictionary<string, MethodInfo>>();
        static Dictionary<string, bool> checkTokens = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);

        static readonly object methodObj = new object();
        internal static MethodInfo GetMethod(Type t, string methodName)
        {
            bool hasTokenAttr;
            return GetMethod(t, methodName, out hasTokenAttr);
        }
        internal static MethodInfo GetMethod(Type t, string methodName, out bool hasTokenAttr)
        {
            string key = t.FullName;
            Dictionary<string, MethodInfo> dic = null;
            if (!typeMethods.ContainsKey(key))
            {
                lock (methodObj)
                {
                    if (!typeMethods.ContainsKey(key))
                    {
                        Type tokenType = typeof(TokenAttribute);
                        bool hasToken = t.GetCustomAttributes(tokenType, true).Length > 0;
                        if (hasToken)
                        {
                            checkTokens.Add(key, true);
                        }
                        MethodInfo[] items = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                        dic = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
                        foreach (MethodInfo item in items)
                        {
                            if (!dic.ContainsKey(item.Name))//对于重载的同名方法，只取第一个空方法。
                            {
                                if (!hasToken && item.GetCustomAttributes(tokenType, true).Length > 0)
                                {
                                    checkTokens.Add(key + "." + item.Name, true);
                                }
                                dic.Add(item.Name, item);
                            }
                        }
                        typeMethods.Add(key, dic);
                    }
                }
            }
            dic = typeMethods[key];
            if (!dic.ContainsKey(methodName))
            {
                methodName = Default;
            }
            hasTokenAttr = checkTokens.ContainsKey(key);
            if (!hasTokenAttr)
            {
                hasTokenAttr = checkTokens.ContainsKey(key + "." + methodName);
            }
            return dic[methodName];
        }
        #endregion
    }
}
