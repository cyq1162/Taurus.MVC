using CYQ.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Taurus.Mvc.Reflect
{
    ///// <summary>
    ///// 搜索控制器的程序集
    ///// </summary>
    //internal partial class AssemblyCollector
    //{
    //    #region GetAssembly
    //    private static bool _IsSearchAll = false;
    //    private static string _DllNames;
    //    /// <summary>
    //    /// 控制器名称（多个时逗号分隔）
    //    /// </summary>
    //    internal static string DllNames
    //    {
    //        get
    //        {
    //            if (string.IsNullOrEmpty(_DllNames))
    //            {
    //                _DllNames = MvcConfig.Controllers;
    //                if (string.IsNullOrEmpty(_DllNames) || _DllNames == "*")
    //                {
    //                    _DllNames = string.Empty;
    //                    string[] files = Directory.GetFiles(AppConfig.AssemblyPath, "*Controller*.dll", SearchOption.TopDirectoryOnly);
    //                    if (files == null || files.Length == 0)
    //                    {
    //                        _IsSearchAll = true;
    //                        files = Directory.GetFiles(AppConfig.AssemblyPath, "*.dll", SearchOption.TopDirectoryOnly);//没有配置，搜索所有的dll。
    //                    }
    //                    if (files != null)
    //                    {
    //                        StringBuilder sb = new StringBuilder();
    //                        foreach (string file in files)
    //                        {
    //                            string name = Path.GetFileName(file);
    //                            if (name == "CYQ.Data.dll" || name == "Taurus.Core.dll" || name == "sni.dll" || name.StartsWith("System.") || name.StartsWith("Microsoft."))
    //                            {
    //                                continue;
    //                            }
    //                            sb.Append(file);
    //                            sb.Append(',');
    //                        }
    //                        _DllNames = sb.ToString().TrimEnd(',');
    //                    }
    //                }
    //            }
    //            return _DllNames;
    //        }
    //    }
    //    private static List<Assembly> _Assemblys;
    //    public static List<Assembly> GetAssemblys()
    //    {
    //        if (_Assemblys == null)
    //        {
    //            string[] dllItems = DllNames.Split(',');
    //            _Assemblys = new List<Assembly>(dllItems.Length);
    //            foreach (string dll in dllItems)
    //            {
    //                try
    //                {
    //                    if (AppConfig.IsNetCore && dll.IndexOfAny(new char[] { '\\', '/' }) > 0 && dll[0] != '/')
    //                    {
    //                        //1、NetCore 程序 部署在Linux 环境，有些无理要求此方式才能正常加载。
    //                        //2、NetCore 程序 部署在Window IIS，反正会被W3wp.exe 锁定，因此用此法也无啥影响。
    //                        //3、传统.Net Framewok 应避开此方式加载（会独站锁定dll文件），重复停止应用程序影响开发效率。
    //                        _Assemblys.Add(Assembly.LoadFile(dll));
    //                    }
    //                    else
    //                    {
    //                        _Assemblys.Add(Assembly.Load(Path.GetFileName(dll.Replace(".dll", "")))); // 可直接抛异常。
    //                    }
    //                }
    //                catch (Exception err)
    //                {
    //                    Log.WriteLogToTxt(err, LogType.Taurus);
    //                    if (!_IsSearchAll)
    //                    {
    //                        throw err;
    //                    }
    //                }

    //            }
    //        }
    //        return _Assemblys;
    //    }

    //    #endregion
    //}
    /// <summary>
    /// 搜索控制器的程序集 新方法
    /// </summary>
    internal partial class AssemblyCollector
    {
        //private static readonly object lockObj = new object();
        //private static List<Assembly> _ControllerAssemblyList;
        //public static List<Assembly> ControllerAssemblyList
        //{
        //    get
        //    {
        //        if (_ControllerAssemblyList == null)
        //        {
        //            lock (lockObj)
        //            {
        //                if (_ControllerAssemblyList == null)
        //                {
        //                    _ControllerAssemblyList = GetRefAssemblyList();
        //                }
        //            }
        //        }
        //        return _ControllerAssemblyList;
        //    }
        //}
        /// <summary>
        /// 存储 控制器 的程序集列表。
        /// </summary>
        internal static List<Assembly> ControllerAssemblyList = new List<Assembly>();
        /// <summary>
        /// 获取引用自身的程序集列表
        /// </summary>
        internal static List<Assembly> GetRefAssemblyList()
        {
            string current = Assembly.GetExecutingAssembly().GetName().Name;
            //获取所有程序集，netcore 有些可能后期有动态加载情况，比如项目运行就丢失了控制器程序集
            Assembly[] assList = AppDomain.CurrentDomain.GetAssemblies();
            string[] files = Directory.GetFiles(AppConst.AssemblyPath, "*.dll", SearchOption.TopDirectoryOnly);//搜索所有的dll。
            List<string> dllFileList = new List<string>(files);
            List<Assembly> refAssemblyList = new List<Assembly>();
            foreach (Assembly assembly in assList)
            {
                if (dllFileList.Contains(assembly.Location))
                {
                    dllFileList.Remove(assembly.Location);
                }

                if (assembly.GlobalAssemblyCache || assembly.GetName().GetPublicKeyToken().Length > 0)
                {
                    //过滤一下系统标准库
                    continue;
                }
                else
                {
                    switch (assembly.GetName().Name)
                    {
                        case "sni":
                        case "Newtonsoft.Json":
                        case "DynamicExpresso.Core":
                        case "Confluent.Kafka":
                        case "RabbitMQ.Client":
                        case "CYQ.Data":
                        case "Taurus.View":
                        case "Taurus.Core":
                            continue;

                    }
                    refAssemblyList.Add(assembly);
                    ////搜索引用自身的程序集，不启用这个，因为可能中间隔一个 Logic 层
                    //foreach (AssemblyName item in assembly.GetReferencedAssemblies())
                    //{
                    //    if (current == item.Name)
                    //    {
                    //        //引用了自身
                    //        refAssemblyList.Add(assembly);
                    //        break;
                    //    }
                    //}
                }
            }

            if (dllFileList.Count > 0)
            {
                foreach (string dll in dllFileList)
                {
                    string name = Path.GetFileName(dll);
                    switch (name)
                    {
                        case "sni.dll":
                        case "Newtonsoft.Json.dll":
                        case "DynamicExpresso.Core.dll":
                        case "Confluent.Kafka.dll":
                        case "RabbitMQ.Client.dll":
                            continue;
                        default:
                            if (name.StartsWith("System.") || name.StartsWith("Microsoft."))
                            {
                                continue;
                            }
                            break;
                    }

                    //加载漏掉的
                    try
                    {
                        Assembly ass = null;
                        if (AppConst.IsNetCore && dll.IndexOfAny(new char[] { '\\', '/' }) > 0 && dll[0] != '/')
                        {
                            //1、NetCore 程序 部署在Linux 环境，有些无理要求此方式才能正常加载。
                            //2、NetCore 程序 部署在Window IIS，反正会被W3wp.exe 锁定，因此用此法也无啥影响。
                            //3、传统.Net Framewok 应避开此方式加载（会独站锁定dll文件），重复停止应用程序影响开发效率。
                            ass = Assembly.LoadFile(dll);
                        }
                        else
                        {
                            ass = Assembly.Load(name.Replace(".dll", ""));
                        }
                        if (ass != null)
                        {
                            refAssemblyList.Add(ass);
                            //foreach (AssemblyName item in ass.GetReferencedAssemblies())
                            //{
                            //    if (current == item.Name)
                            //    {
                            //        //引用了自身
                            //        refAssemblyList.Add(ass);
                            //        break;
                            //    }
                            //}
                        }
                    }
                    catch (Exception err)
                    {
                        Log.WriteLogToTxt(err, LogType.Taurus);
                    }
                }
            }
            return refAssemblyList;
        }
       
    }
}
