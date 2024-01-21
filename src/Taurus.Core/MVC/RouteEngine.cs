using System;
using System.Collections.Generic;

namespace Taurus.Mvc
{
    /// <summary>
    /// 手工操作路由映射
    /// </summary>
    public static class RouteEngine
    {
        /// <summary>
        /// 手工添加方法（For Method）映射地址。
        /// </summary>
        /// <param name="localUrlFrom">原始访问相对路径</param>
        /// <param name="localUrlTo">映射访问相对路径</param>
        /// <returns></returns>
        public static bool Add(string localUrlFrom, string localUrlTo)
        {
            if (!string.IsNullOrEmpty(localUrlFrom) && !string.IsNullOrEmpty(localUrlTo))
            {
                localUrlFrom = localUrlFrom.ToLower();
                localUrlTo = localUrlTo.ToLower();
                if (localUrlFrom != localUrlTo && !mappingUrl.ContainsKey(localUrlFrom))
                {
                    mappingUrl.Add(localUrlFrom, localUrlTo);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 存档方法映射Url
        /// </summary>
        private static Dictionary<string, string> mappingUrl = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 获取映射后的Url
        /// </summary>
        /// <param name="localUrl">访问的Url（相对地址）</param>
        /// <returns></returns>
        public static string Get(string localUrl)
        {
            if (mappingUrl.ContainsKey(localUrl))
            {
                return mappingUrl[localUrl];
            }
            return null;
        }
        /*
        /// <summary>
        /// 手工添加控制器（For Class）模块名称映射地址。
        /// </summary>
        /// <param name="controllerNameFrom">原始访问控制器模块名称</param>
        /// <param name="controllerType">映射访问控制器模块名</param>
        /// <returns></returns>
        public static bool AddPrefix(string controllerNameFrom, Type controllerType)
        {
            if (!string.IsNullOrEmpty(controllerNameFrom) && controllerType != null)
            {
                controllerNameFrom = controllerNameFrom.Replace("/", ".");
                if (!mappingUrl.ContainsKey(controllerNameFrom))
                {
                    mappingController.Add(controllerNameFrom, controllerType);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// 存档控制器映射
        /// </summary>
        private static Dictionary<string, Type> mappingController = new Dictionary<string, Type>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 获取映射后的控制器名称
        /// </summary>
        /// <param name="controllerName">原始访问控制器模块名称</param>
        /// <returns></returns>
        public static Type GetPrefix(string controllerName)
        {
            controllerName= controllerName.Replace("/", ".");
            if (mappingController.ContainsKey(controllerName))
            {
                return mappingController[controllerName];
            }
            return null;
        }
        **/
    }
}
