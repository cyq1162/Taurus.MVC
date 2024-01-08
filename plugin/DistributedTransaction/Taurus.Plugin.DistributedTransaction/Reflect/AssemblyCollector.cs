using CYQ.Data;
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;

namespace Taurus.Plugin.DistributedTransaction
{
    /// <summary>
    /// 搜索控制器的程序集【不缓存，因为只用1次】
    /// </summary>
    internal class AssemblyCollector
    {
        #region GetAssembly

        ///// <summary>
        ///// 控制器名称（多个时逗号分隔）
        ///// </summary>
        //internal static string DllNames
        //{
        //    get
        //    {
        //        string[] files = Directory.GetFiles(AppConfig.AssemblyPath, "*Controller*.dll", SearchOption.TopDirectoryOnly);
        //        if (files == null || files.Length == 0)
        //        {
        //            files = Directory.GetFiles(AppConfig.AssemblyPath, "*.dll", SearchOption.TopDirectoryOnly);//没有配置，搜索所有的dll。
        //        }
        //        if (files != null)
        //        {
        //            StringBuilder sb = new StringBuilder();
        //            foreach (string file in files)
        //            {
        //                string name = Path.GetFileName(file);
        //                if (name == "CYQ.Data.dll" || name == "Taurus.Core.dll" || name == "sni.dll" || name.StartsWith("System.") || name.StartsWith("Microsoft."))
        //                {
        //                    continue;
        //                }
        //                sb.Append(file);
        //                sb.Append(',');
        //            }
        //            return sb.ToString().TrimEnd(',');
        //        }
        //        return string.Empty;
        //    }
        //}

        //public static List<Assembly> GetAssemblys2()
        //{
        //    string[] dllItems = DllNames.Split(',');
        //    List<Assembly> _Assemblys = new List<Assembly>(dllItems.Length);
        //    foreach (string dll in dllItems)
        //    {
        //        try
        //        {
        //            if (AppConfig.IsNetCore && dll.IndexOfAny(new char[] { '\\', '/' }) > 0 && dll[0] != '/')
        //            {
        //                //1、NetCore 程序 部署在Linux 环境，有些无理要求此方式才能正常加载。
        //                //2、NetCore 程序 部署在Window IIS，反正会被W3wp.exe 锁定，因此用此法也无啥影响。
        //                //3、传统.Net Framewok 应避开此方式加载（会独站锁定dll文件），重复停止应用程序影响开发效率。
        //                _Assemblys.Add(Assembly.LoadFile(dll));
        //            }
        //            else
        //            {
        //                _Assemblys.Add(Assembly.Load(Path.GetFileName(dll.Replace(".dll", "")))); // 可直接抛异常。
        //            }
        //        }
        //        catch (Exception err)
        //        {
        //            Log.WriteLogToTxt(err, "Taurus.Plugin.DistributedTransaction");
        //        }

        //    }

        //    return _Assemblys;
        //}

        /// <summary>
        /// 获取引用自身的程序集列表
        /// </summary>
        public static List<Assembly> GetRefAssemblyList()
        {
            string current = Assembly.GetExecutingAssembly().GetName().Name;
            //获取所有程序集
            Assembly[] assList = AppDomain.CurrentDomain.GetAssemblies();
            List<Assembly> refAssemblyList = new List<Assembly>();
            foreach (Assembly assembly in assList)
            {
                AssemblyName assName = assembly.GetName();
                if (assName.Name.StartsWith("Microsoft") || assName.Name.StartsWith("System") || assName.Name.StartsWith("netstandard")
                    || assName.Name.StartsWith("mscorlib") || assName.Name.StartsWith("Anonymously"))
                {
                    //过滤一下系统标准库
                    continue;
                }
                else
                {
                    //搜索引用自身的程序集
                    foreach (AssemblyName item in assembly.GetReferencedAssemblies())
                    {
                        if (current == item.Name)
                        {
                            //引用了自身
                            refAssemblyList.Add(assembly);
                            break;
                        }
                    }
                }
            }
            return refAssemblyList;
        }
        #endregion
    }
}
