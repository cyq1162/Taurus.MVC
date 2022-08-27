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
    internal static class ControllerCollector
    {
       

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
                    if (string.IsNullOrEmpty(_DllNames) || _DllNames == "*")
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

      
    }
}
