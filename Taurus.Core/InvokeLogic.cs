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
        internal const string Controller = "Controller";
        internal const string Default = "Default";
        internal const string DefaultController = "DefaultController";
        internal const string AjaxController = "AjaxController";
        internal const string ViewController = "ViewController";
        internal const string DefaultAjaxController = "DefaultAjaxController";
        internal const string DefaultViewController = "DefaultViewController";

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

        private static Dictionary<string, Type> _ViewControllers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, Type> _AjaxControllers = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        private static readonly object objLock = new object();
        /// <summary>
        /// 获取控制器
        /// </summary>
        /// <param name="typeFlag">0：Ajax控制器；1：View控制器</param>
        /// <returns></returns>
        private static Dictionary<string, Type> GetControllers(int typeFlag)
        {
            if (_AjaxControllers.Count == 0 && _ViewControllers.Count == 0)
            {
                lock (objLock)
                {
                    if (_AjaxControllers.Count == 0 && _ViewControllers.Count == 0)
                    {
                        Assembly ass = GetAssembly();
                        Type[] typeList = ass.GetExportedTypes();
                        foreach (Type type in typeList)
                        {
                            if (type.BaseType != null)
                            {
                                bool isDefaultController = type.Name == DefaultAjaxController || type.Name == DefaultViewController;
                                if (isDefaultController)
                                {
                                    if (type.BaseType.Name == AjaxController)
                                    {
                                        _AjaxControllers.Add(DefaultController, type);
                                    }
                                    else
                                    {
                                        _ViewControllers.Add(DefaultController, type);
                                    }

                                }
                                else
                                {
                                    string[] names = type.FullName.ToLower().Split('.');
                                    if (names.Length > 1)
                                    {
                                        string className = names[names.Length - 1];
                                        string key = Controller.ToLower();
                                        if (className.EndsWith(key))
                                        {
                                            int subLen = key.Length;
                                            if (className.EndsWith(AjaxController.ToLower()) || className.EndsWith(ViewController.ToLower()))
                                            {
                                                subLen = key.Length + 4;
                                            }
                                            className = className.Substring(0, className.Length - subLen);//不存Controller
                                        }
                                        if (type.BaseType.Name == AjaxController)
                                        {
                                            _AjaxControllers.Add(names[names.Length - 2] + "." + className, type);
                                        }
                                        else
                                        {
                                            _ViewControllers.Add(names[names.Length - 2] + "." + className, type);
                                        }

                                    }
                                }
                            }

                        }
                    }
                }
            }
            return typeFlag == 0 ? _AjaxControllers : _ViewControllers;
        }
        /// <summary>
        /// 通过XXX.className类名获得对应的Controller类
        /// </summary>
        /// <param name="className"></param>
        /// <param name="typeFlag">0：Ajax控制器；1：View控制器</param>
        /// <returns></returns>
        public static Type GetType(string className, int typeFlag)
        {
            Dictionary<string, Type> controllers = GetControllers(typeFlag);
            if (controllers.ContainsKey(className)) //1：完整匹配【名称空间.类名】
            {
                return controllers[className];
            }
            else
            {
                string[] items = className.Split('.');
                if (typeFlag == 0)
                {
                    #region Ajax映射处理
                    className = items[items.Length - 1];
                    string okKey = string.Empty;
                    className = "." + className;
                    string path = "." + items[0];
                    foreach (string key in controllers.Keys) //2：部分匹配【.类名】
                    {
                        if (key.EndsWith(className))
                        {
                            return controllers[key];
                        }
                        else if (key.EndsWith(path))//匹配 目录
                        {
                            okKey = key;
                        }
                    }
                    if (!string.IsNullOrEmpty(okKey))//3：匹配 目录
                    {
                        return controllers[okKey];
                    }
                    #endregion
                }
                else if (typeFlag == 1)
                {
                    className = "." + items[0];
                    foreach (string key in controllers.Keys) //2：部分匹配【.类名】
                    {
                        if (key.EndsWith(className))
                        {
                            return controllers[key];
                        }
                    }
                }
            }
            if (controllers.ContainsKey(DefaultController))
            {
                return controllers[DefaultController];
            }
            return null;
        }
        public static Type GetDefaultType(int typeFlag)
        {
            Dictionary<string, Type> controllers = GetControllers(typeFlag);
            if (controllers.ContainsKey(DefaultController))
            {
                return controllers[DefaultController];
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
                        MethodInfo[] items = t.GetMethods();
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
            return null;
        }
        #endregion
    }
}
