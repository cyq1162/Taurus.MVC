using System;
using System.Data;

namespace Taurus.Mvc.Attr
{
    /// <summary>
    /// 用于路由映射(for method)，允许多条。
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true, Inherited = true)]
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// 路由映射路径
        /// </summary>
        public string MapPath { get; set; }

        /// <summary>
        /// 是否保留原始路径
        /// </summary>
        public bool IsKeepOriginalPath { get; set; }

        /// <summary>
        /// 用于路由映射(for method)，允许多条。
        /// </summary>
        /// <param name="mapPath">相对路径：以"/"开头为独立匹配，否则叠加RoutePrefix头（若类中已配置该属性）。</param>
        public RouteAttribute(string mapPath)
        {
            this.MapPath = mapPath;
        }

        /// <summary>
        /// 用于路由映射(for method)，允许多条。
        /// </summary>
        /// <param name="mapPath">相对路径：以"/"开头为独立匹配，否则叠加RoutePrefix头（若类中已配置该属性）。</param>
        /// <param name="isKeepOriginalPath">是否保留原始路径</param>
        public RouteAttribute(string mapPath,bool isKeepOriginalPath)
        {
            this.MapPath = mapPath;
            this.IsKeepOriginalPath = isKeepOriginalPath;
        }
    }

}
