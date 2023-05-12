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
        /// 获取设置的前缀名称
        /// </summary>
        public string PrefixName { get; set; }
        /// <summary>
        /// 用于路由控制器模块映射(for class)，允许多条
        /// </summary>
        /// <param name="prefixName">前缀名称，配合方法【Route】属性叠加使用。</param>
        public RoutePrefixAttribute(string prefixName)
        {
            this.PrefixName = prefixName;
        }
    }

}
