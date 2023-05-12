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
        /// 获取设置的相对路径。
        /// </summary>
        public string LocalPath { get; set; }

        /// <summary>
        /// 用于路由映射(for method)，允许多条。
        /// </summary>
        /// <param name="localPath">相对路径：以"/"开头为独立匹配，否则叠加RoutePrefix头（若类中已配置该属性）。</param>
        public RouteAttribute(string localPath)
        {
            this.LocalPath = localPath;
        }
    }

}
