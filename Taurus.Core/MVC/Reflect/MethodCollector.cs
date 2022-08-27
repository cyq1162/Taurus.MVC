using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using System.IO;
namespace Taurus.Core
{
    internal static class MethodCollector
    {
        private static Type GetController(string name)
        {
            return ControllerCollector.GetController(name);
        }
        #region GetMethods

        #region 4个全局方法
        private static MethodInfo _GlobalDefault = null;
        /// <summary>
        /// 全局Default方法
        /// </summary>
        public static MethodInfo GlobalDefault
        {
            get
            {
                if (_GlobalDefault == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _GlobalDefault = t.GetMethod(Const.Default, BindingFlags.Instance | BindingFlags.Public);
                    }
                }
                return _GlobalDefault;
            }
        }

        private static MethodInfo _GlobalCheckAck = null;
        /// <summary>
        /// 全局DefaultCheckAck方法
        /// </summary>
        public static MethodInfo GlobalCheckAck
        {
            get
            {
                if (_GlobalCheckAck == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _GlobalCheckAck = t.GetMethod(Const.CheckAck, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _GlobalCheckAck;
            }
        }
        private static MethodInfo _GlobalCheckMicroService = null;
        /// <summary>
        /// 全局DefaultCheckMicroService方法
        /// </summary>
        public static MethodInfo GlobalCheckMicroService
        {
            get
            {
                if (_GlobalCheckMicroService == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _GlobalCheckMicroService = t.GetMethod(Const.CheckMicroService, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _GlobalCheckMicroService;
            }
        }

        private static MethodInfo _GlobalCheckToken = null;
        /// <summary>
        /// 全局CheckToken方法
        /// </summary>
        public static MethodInfo GlobalCheckToken
        {
            get
            {
                if (_GlobalCheckToken == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _GlobalCheckToken = t.GetMethod(Const.CheckToken, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _GlobalCheckToken;
            }
        }
        private static MethodInfo _GlobalBeforeInvoke = null;
        /// <summary>
        ///  全局BeforeInvoke方法
        /// </summary>
        public static MethodInfo GlobalBeforeInvoke
        {
            get
            {
                if (_GlobalBeforeInvoke == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _GlobalBeforeInvoke = t.GetMethod(Const.BeforeInvoke, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _GlobalBeforeInvoke;
            }
        }
        private static MethodInfo _GlobalRouteMapInvoke= null;
        /// <summary>
        ///  全局BeforeInvoke方法
        /// </summary>
        public static MethodInfo GlobalRouteMapInvoke
        {
            get
            {
                if (_GlobalRouteMapInvoke == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _GlobalRouteMapInvoke = t.GetMethod(Const.RouteMapInvoke, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _GlobalRouteMapInvoke;
            }
        }
        private static MethodInfo _GlobalEndInvoke = null;
        /// <summary>
        ///  全局EndInvokeMethod方法
        /// </summary>
        public static MethodInfo GlobalEndInvoke
        {
            get
            {
                if (_GlobalEndInvoke == null)
                {
                    Type t = GetController(Const.Default);
                    if (t != null)
                    {
                        _GlobalEndInvoke = t.GetMethod(Const.EndInvoke, BindingFlags.Static | BindingFlags.Public);
                    }
                }
                return _GlobalEndInvoke;
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
