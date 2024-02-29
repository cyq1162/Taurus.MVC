using System;

namespace Taurus.Mvc.Attr
{
    /// <summary>
    /// 用于路由控制器模块映射(for class)，允许多条。
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = true)]
    public class RoutePrefixAttribute : Attribute
    {
        /// <summary>
        /// 获取设置的映射前缀名称
        /// </summary>
        public string MapName { get; set; }

        /// <summary>
        /// 获取是否保留原始路径
        /// </summary>
        public bool IsKeepOriginalPath { get; set; }

        /// <summary>
        /// 用于路由控制器模块映射(for class)，允许多条
        /// </summary>
        /// <param name="mapName">映射前缀名称，配合方法【Route】属性叠加使用。</param>
        public RoutePrefixAttribute(string mapName)
        {
            this.MapName = mapName;
        }
        /// <summary>
        /// 用于路由控制器模块映射(for class)，允许多条
        /// </summary>
        /// <param name="mapName">映射前缀名称，配合方法【Route】属性叠加使用。</param>
        /// <param name="isKeepOriginalPath">是否保留原始路径</param>
        public RoutePrefixAttribute(string mapName, bool isKeepOriginalPath)
        {
            this.MapName = mapName;
            this.IsKeepOriginalPath = isKeepOriginalPath;
        }
    }

}
