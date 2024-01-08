using System;
using System.Collections.Generic;
using CYQ.Data.Tool;
using System.Reflection;
namespace Taurus.Plugin.DistributedTransaction
{
    internal static partial class MethodCollector
    {
        static MethodCollector()
        {
            InitMethods();
        }
        //这里区分大小写，是为了保持用户方法传的key和回调key一致。
        private static MDictionary<string, MethodInfo> dicServerMethods = new MDictionary<string, MethodInfo>();
        private static MDictionary<string, MethodInfo> dicClientMethods = new MDictionary<string, MethodInfo>();
        private static void InitMethods()
        {
            List<Assembly> assList = AssemblyCollector.GetRefAssemblyList();
            if (assList != null && assList.Count > 0)
            {
                foreach (Assembly ass in assList)
                {
                    foreach (Type type in ass.GetExportedTypes())
                    {
                        InitMethod(type);
                    }
                }
            }

        }
        private static void InitMethod(Type classType)
        {
            MethodInfo[] methods = classType.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static);
            if (methods != null && methods.Length > 0)
            {
                foreach (MethodInfo method in methods)
                {
                    #region 服务端订阅


                    object[] items = method.GetCustomAttributes(typeof(DTCServerSubscribeAttribute), true);
                    if (items != null && items.Length > 0)
                    {
                        foreach (var item in items)
                        {
                            var dtcs = item as DTCServerSubscribeAttribute;
                            string key = dtcs.SubKey;
                            if (!dicServerMethods.ContainsKey(key))
                            {
                                dicServerMethods.Add(key, method);
                            }
                        }
                    }
                    #endregion
                    #region 客户端订阅
                    items = method.GetCustomAttributes(typeof(DTCClientCallBackAttribute), true);
                    if (items != null && items.Length > 0)
                    {
                        foreach (var item in items)
                        {
                            var dtcs = item as DTCClientCallBackAttribute;
                            string key = dtcs.CallBackKey;
                            if (!dicClientMethods.ContainsKey(key))
                            {
                                dicClientMethods.Add(key, method);
                            }
                        }
                    }
                    #endregion
                }
            }
        }

    }
    internal static partial class MethodCollector
    {

        public static MethodInfo GetServerMethod(string subKey)
        {
            if (!string.IsNullOrEmpty(subKey) && dicServerMethods.ContainsKey(subKey))
            {
                return dicServerMethods[subKey];
            }
            return null;
        }
        public static MethodInfo GetClientMethod(string subKey)
        {
            if (!string.IsNullOrEmpty(subKey) && dicClientMethods.ContainsKey(subKey))
            {
                return dicClientMethods[subKey];
            }
            return null;
        }
    }
}
