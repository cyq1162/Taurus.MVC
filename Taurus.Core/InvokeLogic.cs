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
                try
                {
                    _Assembly = Assembly.Load(DllName);
                }
                catch (Exception err)
                {
                    Log.WriteLogToTxt(err);
                }
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
        /// <param name="typeFlag">0：Ajax控制器；1：View控制器</param>
        /// <returns></returns>
        private static Dictionary<string, Type> GetControllers()
        {
            if (_Controllers.Count == 0)
            {
                lock (objLock)
                {
                    if (_Controllers.Count == 0)
                    {
                        Assembly ass = GetAssembly();
                        Type[] typeList = ass.GetExportedTypes();
                        foreach (Type type in typeList)
                        {
                            if (type.BaseType != null && type.BaseType.Name == Controller)
                            {
                                _Controllers.Add(type.Name.Replace(Controller, ""), type);
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
        static Dictionary<string, Dictionary<string, MethodInfo>> typeMethods = new Dictionary<string, Dictionary<string, MethodInfo>>();
        static readonly object objlockMethod = new object();
        internal static MethodInfo GetMethod(Type t, string methodName)
        {
            string key = t.FullName;
            Dictionary<string, MethodInfo> dic = null;
            if (!typeMethods.ContainsKey(key))
            {
                lock (objlockMethod)
                {
                    if (!typeMethods.ContainsKey(key))
                    {
                        MethodInfo[] items = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                        dic = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
                        foreach (MethodInfo item in items)
                        {
                            if (!dic.ContainsKey(item.Name))//对于重载的同名方法，只取第一个空方法。
                            {
                                dic.Add(item.Name, item);
                            }
                        }
                        typeMethods.Add(key, dic);
                    }
                }
            }
            dic = typeMethods[key];
            if (dic.ContainsKey(methodName))
            {
                return dic[methodName];
            }
            if (dic.ContainsKey(Default))
            {
                return dic[Default];
            }
            return null;
        }
        #endregion
    }
}
