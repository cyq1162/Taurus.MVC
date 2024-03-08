using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.IO;
using CYQ.Data;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Emit;
using System.Xml;
using System.Data;
using System.Collections;
using CYQ.Data.Tool;
using System.Linq;
using System.Runtime.Versioning;
namespace Taurus.Plugin.MicroService.Proxy
{
    /// <summary>
    /// Rpc 客户端代理生成类
    /// </summary>
    internal static class RestProxyCreator
    {
        /// <summary>
        /// 构建并保存程序集
        /// </summary>
        /// <param name="assName">程序集名称</param>
        /// <param name="savePath">保存路径</param>
        public static bool BuildAssembly(string assName, string savePath)
        {
            if (string.IsNullOrEmpty(assName)) { assName = "RpcProxy"; }
            if (string.IsNullOrEmpty(savePath))
            {
                savePath = AppConst.WebRootPath + assName + ".dll";
            }
            else if (!savePath.StartsWith(AppConst.WebRootPath))
            {
                savePath = AppConst.WebRootPath + savePath;
            }
            string version;
            var code = RestProxyCoder.CreateCode(assName, out version);
            var title = assName + " for Taurus MicroService, build on .netcore " + Environment.Version.ToString();
            var versionCode = RestProxyCoder.CreateVersionCode(title, title, version);

            SyntaxTree syntaxVersionTree = CSharpSyntaxTree.ParseText(versionCode);
            SyntaxTree syntaxCodeTree = CSharpSyntaxTree.ParseText(code);


            // 定义编译选项
            CSharpCompilationOptions compilationOptions = new CSharpCompilationOptions(OutputKind.DynamicallyLinkedLibrary)
            .WithOptimizationLevel(OptimizationLevel.Release); // 设置优化级别

            // 创建 Compilation
            CSharpCompilation compilation = CSharpCompilation.Create(assName,
                new[] { syntaxVersionTree, syntaxCodeTree },
                references: AddRef(),
                options: compilationOptions);

            // 编译并生成程序集
            using (MemoryStream ms = new MemoryStream())
            {
                using (Stream win32resStream = compilation.CreateDefaultWin32Resources(
                                                                            versionResource: true, // 生成版本号。
                                                                            noManifest: false,
                                                                            manifestContents: null,
                                                                            iconInIcoFormat: null))
                {

                    EmitResult result = compilation.Emit(ms, win32Resources: win32resStream);

                    if (!result.Success)
                    {
                        //foreach (var diagnostic in result.Diagnostics)
                        //{
                        //    Console.WriteLine(diagnostic);
                        //}
                        //Console.WriteLine("fail....");
                        return false;
                    }
                    else
                    {
                        // 保存程序集到文件
                        using (FileStream file = new FileStream(savePath, FileMode.Create))
                        {
                            ms.Seek(0, SeekOrigin.Begin);
                            ms.CopyTo(file);
                        }
                        //Console.WriteLine("OK....");
                    }
                }
            }

            return true;

        }

        /// <summary>
        /// 添加程序集引用
        /// </summary>
        /// <returns></returns>
        private static PortableExecutableReference[] AddRef()
        {
            List<PortableExecutableReference> exeRefs = new List<PortableExecutableReference>();

            string path = typeof(object).Assembly.Location;

            #region 引用路径修正
            path = Path.GetDirectoryName(path);
            if (path.Contains("\\shared\\Microsoft.NETCore.App"))
            {
                // windows 平台，这是nuget 实现程序集路径
                string refPath = path.Replace("\\shared\\Microsoft.NETCore.App", "\\\\packs\\Microsoft.NETCore.App.Ref") + "\\ref";
                if (Directory.Exists(refPath))
                {
                    string[] files = Directory.GetDirectories(refPath);
                    if (files.Length > 0) { path = files[0] + "\\"; }
                }
            }
            #endregion
            foreach (string dllFilePath in Directory.GetFiles(path, "*.dll"))
            {
                var dll = Path.GetFileName(dllFilePath);

                if (dll.Split(".").Length > 4) { continue; }
                if (dll.StartsWith("Microsoft.")) { continue; }
                if (dll.StartsWith("System.Drawing.")) { continue; }
                if (dll.StartsWith("System.IO.")) { continue; }
                if (dll.StartsWith("System.Linq.")) { continue; }
                if (dll.StartsWith("System.Net.")) { continue; }
                if (dll.StartsWith("System.Reflection.")) { continue; }
                if (dll.StartsWith("System.Security.")) { continue; }
                if (dll.StartsWith("System.Text.")) { continue; }
                if (dll.StartsWith("System.Threading.")) { continue; }
                if (dll.StartsWith("System.Globalization.")) { continue; }
                if (dll.StartsWith("System.Resources.")) { continue; }
                if (dll.StartsWith("System.Transactions.")) { continue; }
                if (dll.StartsWith("System.Memory.")) { continue; }
                if (dll.StartsWith("System.Formats.")) { continue; }
                if (dll.StartsWith("System.ComponentModel.")) { continue; }
                if (dll.StartsWith("System.Windows.")) { continue; }
                if (dll.StartsWith("System.Diagnostics.")) { continue; }
                if (dll.Contains("VisualBasic")) { continue; }


                exeRefs.Add(MetadataReference.CreateFromFile(dllFilePath));
            }


            //添加引用程序集
            string cyqdata = AppConst.AssemblyPath + "CYQ.Data.dll";
            string taurus = AppConst.AssemblyPath + "Taurus.Core.dll";

            //string lib = path + "mscorlib.dll";
            //string netstandard = path + "netstandard.dll";
            //string system = path + "System.dll";
            //string xml = path + "System.Xml.dll";
            //string runtime = path + "System.Runtime.dll";

            //exeRefs.Add(MetadataReference.CreateFromFile(lib));
            //exeRefs.Add(MetadataReference.CreateFromFile(netstandard));
            //exeRefs.Add(MetadataReference.CreateFromFile(system));
            //exeRefs.Add(MetadataReference.CreateFromFile(xml));
            //exeRefs.Add(MetadataReference.CreateFromFile(runtime));
            exeRefs.Add(MetadataReference.CreateFromFile(cyqdata));
            exeRefs.Add(MetadataReference.CreateFromFile(taurus));
            //exeRefs.Add(MetadataReference.CreateFromFile(typeof(object).Assembly.Location));
            //exeRefs.Add(MetadataReference.CreateFromFile(typeof(XmlDocument).Assembly.Location));
            //exeRefs.Add(MetadataReference.CreateFromFile(typeof(DataTable).Assembly.Location));

            //            MetadataReference.CreateFromFile(taurus)

            // 获取当前应用程序的目标框架

            return exeRefs.ToArray();
        }
    }


}
