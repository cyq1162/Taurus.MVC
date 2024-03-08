using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.IO;
using CYQ.Data;
using System.CodeDom.Compiler;
using System.Runtime.InteropServices;

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
            var title = assName + " for Taurus MicroService, build on .net " + Environment.Version.ToString();
            var versionCode = RestProxyCoder.CreateVersionCode(title, title, version);

            CodeDomProvider provider = CodeDomProvider.CreateProvider("C#");
            CompilerParameters cp = new CompilerParameters();
            cp.ReferencedAssemblies.Add("System.dll");
            cp.ReferencedAssemblies.Add("System.Web.dll");
            cp.ReferencedAssemblies.Add("System.Xml.dll");
            cp.ReferencedAssemblies.Add("System.Data.dll");
            cp.ReferencedAssemblies.Add(AppConst.AssemblyPath + "CYQ.Data.dll");
            cp.ReferencedAssemblies.Add(AppConst.AssemblyPath + "Taurus.Core.dll");
            cp.GenerateExecutable = false;
            cp.GenerateInMemory = false;
            cp.OutputAssembly = savePath;
            CompilerResults cr = provider.CompileAssemblyFromSource(cp, versionCode, code);
            if (cr == null || cr.Errors.Count > 0) { return false; }
            return true;

        }
    }
}
