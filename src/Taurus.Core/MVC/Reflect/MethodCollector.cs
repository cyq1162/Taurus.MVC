using System;
using System.Collections.Generic;
using System.Reflection;
using System.Security.Cryptography;
using Taurus.Mvc.Attr;

namespace Taurus.Mvc.Reflect
{
    //internal static partial class MethodCollector
    //{
    //    private static Type GetController(string name)
    //    {
    //        return ControllerCollector.GetController(name);
    //    }
    //    #region GetMethods


    //    static Dictionary<string, Dictionary<string, MethodInfo>> _TypeMethods = new Dictionary<string, Dictionary<string, MethodInfo>>();
    //    static Dictionary<string, AttributeEntity> _MethodAttrs = new Dictionary<string, AttributeEntity>(StringComparer.OrdinalIgnoreCase);

    //    static readonly object methodObj = new object();
    //    //internal static MethodInfo GetMethod(Type t, string methodName)
    //    //{
    //    //    AttributeEntity hasTokenAttr;
    //    //    return GetMethod(t, methodName, out hasTokenAttr);
    //    //}
    //    internal static MethodInfo GetMethod(Type t, string methodName, out AttributeEntity attrFlags)
    //    {
    //        string key = t.FullName;
    //        Dictionary<string, MethodInfo> dic = null;
    //        attrFlags = null;
    //        if (!_TypeMethods.ContainsKey(key))
    //        {
    //            lock (methodObj)
    //            {
    //                if (!_TypeMethods.ContainsKey(key))
    //                {
    //                    bool hasToken = t.GetCustomAttributes(typeof(TokenAttribute), true).Length > 0;
    //                    bool hasAck = t.GetCustomAttributes(typeof(AckAttribute), true).Length > 0;
    //                    bool hasMicroService = t.GetCustomAttributes(typeof(MicroServiceAttribute), true).Length > 0;
    //                    if (hasToken || hasAck || hasMicroService)
    //                    {
    //                        AttributeEntity al = new AttributeEntity();
    //                        al.HasToken = hasToken;
    //                        al.HasAck = hasAck;
    //                        al.HasMicroService = hasMicroService;
    //                        _MethodAttrs.Add(key, al);
    //                    }
    //                    MethodInfo[] items = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
    //                    dic = new Dictionary<string, MethodInfo>(StringComparer.OrdinalIgnoreCase);
    //                    foreach (MethodInfo item in items)
    //                    {
    //                        if (!dic.ContainsKey(item.Name))//对于重载的同名方法，只取第一个方法。
    //                        {
    //                            dic.Add(item.Name, item);//追加方法名
    //                            object[] attrs = item.GetCustomAttributes(true);
    //                            if (attrs.Length > 0)//追加特性名
    //                            {
    //                                AttributeEntity aFlags = new AttributeEntity();
    //                                foreach (object attr in attrs)
    //                                {
    //                                    string[] names = attr.ToString().Split('.');
    //                                    switch (names[names.Length - 1])
    //                                    {
    //                                        case Const.TokenAttribute:
    //                                            aFlags.HasToken = true; break;
    //                                        case Const.AckAttribute:
    //                                            aFlags.HasAck = true; break;
    //                                        case Const.MicroServiceAttribute:
    //                                            aFlags.HasMicroService = true; break;
    //                                        case Const.HttpGetAttribute:
    //                                            aFlags.HasGet = true; break;
    //                                        case Const.HttpPostAttribute:
    //                                            aFlags.HasPost = true; break;
    //                                        case Const.HttpHeadAttribute:
    //                                            aFlags.HasHead = true; break;
    //                                        case Const.HttpPutAttribute:
    //                                            aFlags.HasPut = true; break;
    //                                        case Const.HttpDeleteAttribute:
    //                                            aFlags.HasDelete = true; break;
    //                                    }

    //                                }
    //                                _MethodAttrs.Add(key + "." + item.Name, aFlags);
    //                            }
    //                        }
    //                    }
    //                    _TypeMethods.Add(key, dic);
    //                }
    //            }
    //        }
    //        dic = _TypeMethods[key];
    //        if (!dic.ContainsKey(methodName))
    //        {
    //            methodName = Const.Default;
    //        }
    //        if (_MethodAttrs.ContainsKey(key + "." + methodName))
    //        {
    //            attrFlags = _MethodAttrs[key + "." + methodName];
    //        }
    //        if (attrFlags == null)
    //        {
    //            attrFlags = new AttributeEntity();
    //        }
    //        if (_MethodAttrs.ContainsKey(key))//如果类级别有，所有方法都继承
    //        {
    //            AttributeEntity al = _MethodAttrs[key];
    //            attrFlags.HasToken = attrFlags.HasToken || al.HasToken;
    //            attrFlags.HasAck = attrFlags.HasAck || al.HasAck;
    //            attrFlags.HasMicroService = attrFlags.HasMicroService || al.HasMicroService;
    //        }
    //        if (dic.ContainsKey(methodName))
    //        {
    //            return dic[methodName];
    //        }
    //        return null;
    //    }
    //    #endregion
    //}

    /// <summary>
    /// 方法搜集器
    /// </summary>
    public static partial class MethodCollector
    {
        static Dictionary<string, Dictionary<string, MethodEntity>> typeMethods = new Dictionary<string, Dictionary<string, MethodEntity>>();

        /// <summary>
        /// 获取类的所有实体方法
        /// </summary>
        /// <param name="t">控制器类型</param>
        /// <returns></returns>
        public static Dictionary<string, MethodEntity> GetMethods(Type t)
        {
            if (typeMethods.ContainsKey(t.FullName))
            {
                return typeMethods[t.FullName];
            }
            return null;
        }
        internal static void InitMethodInfo(TypeEntity entity)
        {
            Type t = entity.Type;
            #region 处理 Controller RoutePrefix 属性映射。
            RoutePrefixAttribute[] rpas = t.GetCustomAttributes(typeof(RoutePrefixAttribute), true) as RoutePrefixAttribute[];
            string moduleName = GetLevelName(t.FullName, MvcConfig.RouteMode);
            #endregion

            bool hasToken = t.GetCustomAttributes(typeof(TokenAttribute), true).Length > 0;
            bool hasAck = t.GetCustomAttributes(typeof(AckAttribute), true).Length > 0;
            bool hasMicroService = t.GetCustomAttributes(typeof(MicroServiceAttribute), true).Length > 0;
            bool hasIgnoreGlobalController = t.GetCustomAttributes(typeof(IgnoreGlobalControllerAttribute), true).Length > 0;
            bool hasWebSocket = t.GetCustomAttributes(typeof(WebSocketAttribute), true).Length > 0;
            Dictionary<string, MethodEntity> dic = new Dictionary<string, MethodEntity>(StringComparer.OrdinalIgnoreCase);
            MethodInfo[] methods = null;
            if (t.FullName.EndsWith(ReflectConst.GlobalController))
            {
                methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public | BindingFlags.DeclaredOnly);
            }
            else
            {
                methods = t.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            }

            foreach (MethodInfo method in methods)
            {
                String name = method.Name;
                if (method.IsStatic)
                {
                    name = "Static." + name;
                }
                if (dic.ContainsKey(name)) { continue; }
                AttributeEntity attributeEntity = new AttributeEntity();
                //attributeEntity.
                attributeEntity.Attributes = method.GetCustomAttributes(true);

                attributeEntity.HasToken = hasToken || method.GetCustomAttributes(typeof(TokenAttribute), true).Length > 0;
                attributeEntity.HasAck = hasAck || method.GetCustomAttributes(typeof(AckAttribute), true).Length > 0;
                attributeEntity.HasMicroService = hasMicroService || method.GetCustomAttributes(typeof(MicroServiceAttribute), true).Length > 0;
                attributeEntity.HasIgnoreGlobalController = hasIgnoreGlobalController || method.GetCustomAttributes(typeof(IgnoreGlobalControllerAttribute), true).Length > 0;
                attributeEntity.HasWebSocket = hasWebSocket || method.GetCustomAttributes(typeof(WebSocketAttribute), true).Length > 0;

                attributeEntity.HasGet = method.GetCustomAttributes(typeof(HttpGetAttribute), true).Length > 0;
                attributeEntity.HasPost = method.GetCustomAttributes(typeof(HttpPostAttribute), true).Length > 0;
                attributeEntity.HasHead = method.GetCustomAttributes(typeof(HttpHeadAttribute), true).Length > 0;
                attributeEntity.HasPut = method.GetCustomAttributes(typeof(HttpPutAttribute), true).Length > 0;
                attributeEntity.HasDelete = method.GetCustomAttributes(typeof(HttpDeleteAttribute), true).Length > 0;

                object[] objects = method.GetCustomAttributes(typeof(RequireAttribute), true);
                if (objects != null && objects.Length > 0)
                {
                    attributeEntity.HasRequire = true;
                    attributeEntity.RequireAttributes = objects as RequireAttribute[];
                }
                dic.Add(name, new MethodEntity(entity, method, attributeEntity));

                #region 处理 Method Route 属性映射。
                objects = method.GetCustomAttributes(typeof(RouteAttribute), true);
                if (objects != null && objects.Length > 0)
                {
                    //手动配置了映射路径：
                    RouteAttribute[] ras = objects as RouteAttribute[];
                    attributeEntity.HasRoute = true;
                    attributeEntity.RouteAttributes = ras;
                    foreach (RouteAttribute ra in ras)
                    {
                        if (ra.LocalPath[0] != '/' && rpas != null && rpas.Length > 0)
                        {
                            foreach (RoutePrefixAttribute rpa in rpas)
                            {
                                string fromUrl = "/" + rpa.PrefixName.Trim('/') + "/" + ra.LocalPath.Trim('/');
                                string toUrl = moduleName + name;
                                RouteEngine.Add(fromUrl, toUrl);
                                RouteEngine.AddDenyUrl(toUrl);
                            }
                        }
                        else
                        {
                            string fromUrl = "/" + ra.LocalPath.Trim('/');
                            string toUrl = moduleName + name;
                            RouteEngine.Add(fromUrl, toUrl);
                            RouteEngine.AddDenyUrl(toUrl);
                        }
                    }
                }
                else if (rpas != null && rpas.Length > 0)
                {
                    //未配置，但配置了 RoutePrefixAttribute
                    foreach (RoutePrefixAttribute rpa in rpas)
                    {
                        string fromUrl = "/" + rpa.PrefixName.Trim('/') + "/" + name;
                        string toUrl = moduleName + name;
                        RouteEngine.Add(fromUrl, toUrl);
                        RouteEngine.AddDenyUrl(toUrl);
                    }
                }
                #endregion
            }
            if (!typeMethods.ContainsKey(t.FullName))//有概念会重复。
            {
                typeMethods.Add(t.FullName, dic);
            }
        }
        /// <summary>
        /// 获取方法：找不到时返回默认方法【default】
        /// </summary>
        /// <param name="t">控制器类型</param>
        /// <param name="methodName">方法名</param>
        /// <returns></returns>
        public static MethodEntity GetMethod(Type t, String methodName)
        {
            return GetMethod(t, methodName, true);
        }
        /// <summary>
        /// 获取方法
        /// </summary>
        /// <param name="t">控制器类型</param>
        /// <param name="methodName">方法名</param>
        /// <param name="isReturnDefault">找不到时是否返回默认方法【default】</param>
        /// <returns></returns>
        public static MethodEntity GetMethod(Type t, String methodName, Boolean isReturnDefault)
        {
            Dictionary<String, MethodEntity> methods = GetMethods(t);
            if (methods != null)
            {
                if (!string.IsNullOrEmpty(methodName))
                {
                    if (methods.ContainsKey(methodName))
                    {
                        return methods[methodName];
                    }
                }
                if (isReturnDefault && methods.ContainsKey(ReflectConst.Default))
                {
                    return methods[ReflectConst.Default];
                }
            }
            return null;
        }

        /// <summary>
        /// 存档N级名称（/Module/Controller/)
        /// </summary>
        /// <param name="fullName"></param>
        /// <returns></returns>
        private static string GetLevelName(string fullName, int level)
        {
            string[] items = fullName.Split('.');
            string lv1Name = items[items.Length - 1].Replace(ReflectConst.Controller, "");
            if (level == 2)
            {
                return "/" + items[items.Length - 2] + "/" + lv1Name + "/";
            }
            return "/" + lv1Name + "/";
        }
    }

    /// <summary>
    /// 全局静态方法
    /// </summary>
    public static partial class MethodCollector
    {
        private static MethodEntity GetGlobalMethod(String name)
        {
            TypeEntity entity = ControllerCollector.GetController(ReflectConst.Global);
            if (entity != null)
            {
                return GetMethod(entity.Type, "Static." + name, false);
            }
            return null;
        }
        #region 4个全局方法
        /// <summary>
        /// 全局Default方法
        /// </summary>
        public static MethodEntity GlobalDefault
        {
            get
            {
                TypeEntity entity = ControllerCollector.GetController(ReflectConst.Global);
                if (entity != null)
                {
                    return GetMethod(entity.Type, ReflectConst.Default, false);
                }
                return null;
            }
        }

        /// <summary>
        /// 全局DefaultCheckAck方法
        /// </summary>
        public static MethodEntity GlobalCheckAck
        {
            get
            {
                return GetGlobalMethod(ReflectConst.CheckAck);
            }
        }
        /// <summary>
        /// 全局DefaultCheckMicroService方法
        /// </summary>
        public static MethodEntity GlobalCheckMicroService
        {
            get
            {
                return GetGlobalMethod(ReflectConst.CheckMicroService);
            }
        }

        /// <summary>
        /// 全局CheckToken方法
        /// </summary>
        public static MethodEntity GlobalCheckToken
        {
            get
            {
                return GetGlobalMethod(ReflectConst.CheckToken);
            }
        }
        /// <summary>
        ///  全局BeforeInvoke方法
        /// </summary>
        public static MethodEntity GlobalBeforeInvoke
        {
            get
            {
                return GetGlobalMethod(ReflectConst.BeforeInvoke);
            }
        }
        /// <summary>
        ///  全局RouteMapInvoke方法
        /// </summary>
        public static MethodEntity GlobalRouteMapInvoke
        {
            get
            {
                return GetGlobalMethod(ReflectConst.RouteMapInvoke);
            }
        }
        /// <summary>
        ///  全局EndInvokeMethod方法
        /// </summary>
        public static MethodEntity GlobalEndInvoke
        {
            get
            {
                return GetGlobalMethod(ReflectConst.EndInvoke);
            }
        }

        /// <summary>
        ///  全局GlobalOnError方法
        /// </summary>
        public static MethodEntity GlobalOnError
        {
            get
            {
                return GetGlobalMethod(ReflectConst.OnError);
            }
        }

        //private static MethodInfo _AuthCheckToken = null;
        ///// <summary>
        ///// 默认AuthController.CheckToken方法
        ///// </summary>
        //public static MethodInfo AuthCheckToken
        //{
        //    get
        //    {
        //        if (_AuthCheckToken == null && Const.IsStartAuth)
        //        {
        //            Type t = GetController(Const.Auth);
        //            if (t != null)
        //            {
        //                _AuthCheckToken = t.GetMethod(Const.CheckToken, BindingFlags.Static | BindingFlags.Public);
        //            }
        //        }
        //        return _AuthCheckToken;
        //    }
        //}

        #endregion
    }
}
