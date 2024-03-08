using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Mvc.Reflect;
using Taurus.Plugin.Admin;

namespace Taurus.Plugin.MicroService.Proxy
{
    internal partial class RestProxyCoder
    {
        public static string CreateCode(string proxyName)
        {
            string version;
            return CreateCode(proxyName, out version);
        }
        public static string CreateCode(string proxyName, out string version)
        {
            version = string.Empty;
            var nameList = GetRegModuleList();
            if (nameList == null || nameList.Count == 0) { return string.Empty; }

            if (string.IsNullOrEmpty(proxyName) || proxyName == "RestProxy")
            {
                proxyName = "RpcProxy";
            }
            else if (proxyName.Contains("."))
            {
                var items = proxyName.Split('.');
                proxyName = items[items.Length - 1];
            }
            var ctls = ControllerCollector.GetControllers(1);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System;");
            sb.AppendLine("using System.Web;");
            sb.AppendLine("using System.Xml;");
            sb.AppendLine("using System.Collections;");
            sb.AppendLine("using System.Collections.Generic;");
            sb.AppendLine("using Taurus.Plugin.Rpc;");
            sb.Append(NameSpaceStart("Taurus.Plugin.MicroService.Proxy"));
            sb.Append(StaticClassStart(proxyName, "  "));
            foreach (var ctl in ctls)
            {
                //忽略框架内部插件
                if (ctl.Value.Type.FullName.StartsWith("Taurus.Plugin.")) { continue; }
                //忽略未注册的模块
                if (!nameList.Contains(ctl.Key.ToLower())) { continue; }

                var className = ctl.Key;
                var methods = MethodCollector.GetMethods(ctl.Value.Type);
                if (methods == null || methods.Count == 0) { continue; }

                if (string.IsNullOrEmpty(version))
                {
                    version = ctl.Value.Type.Assembly.GetName().Version.ToString();
                }


                sb.Append(StaticClassStart(className, "     "));

                //自定义实体类生成
                List<Type> entityList = new List<Type>();
                #region 定义方法
                foreach (var method in methods)
                {
                    var methodNameLower = method.Key.ToLower();
                    switch (methodNameLower)
                    {
                        case "default":
                        case "checkack":
                        case "checktoken":
                        case "checkmicroservice":
                        case "beforeinvoke":
                        case "endinvoke":
                            continue;
                    }
                    string paraName;
                    sb.Append(CreateParaClass(className, method.Value, out paraName, entityList));
                    sb.Append(StaticMethodStart(method.Value.Method.Name, paraName));
                    sb.Append(StaticMethodBody(className, method.Value));
                    sb.Append(StaticMethodEnd());
                }

                //生成实体类
                foreach (var entityType in entityList)
                {
                    sb.Append(GetEntityClassByType(entityType));
                }
                #endregion

                sb.Append(StaticClassEnd("      "));

            }
            sb.Append(StaticClassEnd("   "));
            sb.Append(NameSpaceEnd());
            return sb.ToString();
        }

        #region 创建代码
        private static string NameSpaceStart(string name)
        {
            return string.Format("\r\nnamespace {0} \r\n{{", name);
        }
        private static string NameSpaceEnd()
        {
            return "\r\n}";
        }
        private static string StaticClassStart(string name, string space)
        {
            return string.Format("\r\n" + space + "public static partial class {0} \r\n" + space + "{{", name);
        }
        private static string StaticClassEnd(string space)
        {
            return "\r\n" + space + "}";
        }
        private static string StaticMethodStart(string methodName, string paraName)
        {
            string nullPara = string.Empty;
            if (paraName == "RestDefaultPara")
            {
                nullPara = " = null";
            }
            return string.Format("\r\n          public static RpcTask {0}({1} para{2}) {{", methodName, paraName, nullPara);
        }
        private static string StaticMethodBody(string className, MethodEntity entity)
        {
            string routeUrl = entity.RouteUrl;
            string httpMethod = entity.GetHttpMethod();
            return string.Format("return RestProxy.CallAsync(\"{0}\", \"{1}\",\"{2}\", para);", className, routeUrl, httpMethod);
        }
        private static string StaticMethodEnd()
        {
            return "}";
        }

        private static string CreateParaClass(string className, MethodEntity entity, out string name, List<Type> entityTypes)
        {
            if (entity.Parameters.Length == 0)
            {
                name = "RestDefaultPara";
                return "";
            }
            name = entity.Method.Name + "Para";
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("\r\n          public class {0} : RestParaBase", name);
            sb.Append('{');

            foreach (var item in entity.Parameters)
            {
                GetPropertyByType(item.Name, item.ParameterType, sb, entityTypes);
            }
            sb.Append('}');
            return sb.ToString();
        }

        private static string GetEntityClassByType(Type entityType)
        {
            var proList = ReflectTool.GetPropertyList(entityType);
            var filedList = ReflectTool.GetFieldList(entityType);
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            sb.AppendFormat("public class {0}", entityType.Name);
            sb.Append('{');
            if (proList != null && proList.Count > 0)
            {
                foreach (var pro in proList)
                {
                    GetPropertyByType(pro.Name, pro.PropertyType, sb, null);
                }
            }
            if (filedList != null && filedList.Count > 0)
            {
                foreach (var field in filedList)
                {
                    GetPropertyByType(field.Name, field.FieldType, sb, null);
                }
            }
            sb.Append('}');
            sb.AppendLine();
            return sb.ToString();
        }


        private static void GetPropertyByType(string name, Type itemType, StringBuilder sb, List<Type> entityTypes)
        {
            if (entityTypes != null)
            {
                GetEntityType(itemType, entityTypes);
            }
            string ptName = itemType.Name;
            if (itemType.IsValueType)
            {
                if (itemType.IsGenericType)
                {
                    ptName = Nullable.GetUnderlyingType(itemType).Name + "?";
                }
            }
            else
            {
                Type type = itemType;
                Type[] args;
                SysType sysType = ReflectTool.GetSystemType(ref type);
                ReflectTool.GetArgumentLength(ref type, out args);
                switch (sysType)
                {
                    case SysType.Array:
                        ptName = args[0].Name + "[]";
                        break;
                    case SysType.Generic:
                        if (args.Length == 1)
                        {
                            ptName = "List<" + args[0].Name + ">";
                        }
                        else if (args.Length == 2)
                        {
                            ptName = "Dictionary<" + args[0].Name + "," + args[1].Name + ">";
                        }
                        break;
                }

            }
            var paraName = name[0].ToString().ToUpper() + name.Substring(1, name.Length - 1);
            sb.AppendFormat("public {0} {1} {{ get; set; }}", ptName, paraName);
        }

        private static void GetEntityType(Type type, List<Type> entityTypes)
        {
            if (type.IsValueType || type.Name == "String") { return; }
            SysType sys = ReflectTool.GetSystemType(ref type);
            switch (sys)
            {
                case SysType.Custom:
                    if (!entityTypes.Contains(type) && !type.FullName.StartsWith("System.") && !type.FullName.StartsWith("Microsoft."))
                    {
                        entityTypes.Add(type);

                        var pList = ReflectTool.GetPropertyList(type);
                        if (pList != null)
                        {
                            foreach (var item in pList)
                            {
                                if (item.PropertyType.IsValueType || item.PropertyType.Name == "String") { continue; }
                                GetEntityType(item.PropertyType, entityTypes);
                            }
                        }
                        var fList = ReflectTool.GetFieldList(type);
                        if (fList != null)
                        {
                            foreach (var item in fList)
                            {
                                if (item.FieldType.IsValueType || item.FieldType.Name == "String") { continue; }
                                GetEntityType(item.FieldType, entityTypes);
                            }
                        }
                    }
                    break;
                default:
                    Type[] args;
                    if (ReflectTool.GetArgumentLength(ref type, out args) > 0)
                    {
                        foreach (var arg in args)
                        {
                            GetEntityType(arg, entityTypes);//递归
                        }
                    }
                    break;
            }
        }
        #endregion

        #region 获取注册模块名称列表

        private static List<string> GetRegModuleList()
        {
            string regName = MsConfig.Client.Name;
            if (string.IsNullOrEmpty(regName)) { return null; }
            List<string> nameList = new List<string>();
            string[] names = regName.Trim(' ', '/', '\r', '\n').ToLower().Split(',');//允许一次注册多个模块。
            foreach (var name in names)
            {
                nameList.Add(name.Split('|')[0]);
            }

            return nameList;
        }
        #endregion

    }
    internal partial class RestProxyCoder
    {
        public static string CreateVersionCode(string title, string description, string version)
        {
            if (string.IsNullOrEmpty(title)) { title = "RpcProxy for Taurus MicroService."; }
            if (string.IsNullOrEmpty(description)) { description = "RpcProxy for Taurus MicroService."; }
            if (string.IsNullOrEmpty(version)) { version = "1.0.0.0"; }
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("using System.Reflection;");
            sb.AppendLine("using System.Runtime.CompilerServices;");
            sb.AppendLine("using System.Runtime.InteropServices;");
            sb.AppendFormat("[assembly: AssemblyTitle(\"{0}\")]", title);
            sb.AppendFormat("[assembly: AssemblyDescription(\"{0}\")]", description);
            sb.AppendFormat("[assembly: AssemblyCompany(\"路过秋天 （{0}）\")]", DateTime.Now.ToString("yyyy-MM-dd"));

            sb.AppendLine("[assembly: ComVisible(false)]");

            sb.AppendFormat("[assembly: AssemblyVersion(\"{0}\")]", version);
            return sb.ToString();
        }
    }


    //public static partial class RestProxyCreator
    //{
    //    public static class API
    //    {
    //        public static RpcTask HelloAsync(string name)
    //        {
    //            string host = Taurus.Plugin.MicroService.Gateway.GetHost("API");
    //            string url = host + "/hello";
    //            RpcTaskRequest rpcTaskRequest = new RpcTaskRequest();
    //            return Rest.StartTaskAsync(rpcTaskRequest);
    //        }
    //    }
    //}
}
