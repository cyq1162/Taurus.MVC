using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Taurus.Mvc.Reflect
{
    /// <summary>
    /// Type 实体
    /// </summary>
    public class TypeEntity
    {
        /// <summary>
        /// 反射方法，Invoke调用
        /// </summary>
        public Type Type { get; set; }

        /// <summary>
        /// 返回创建实例的委托
        /// </summary>
        public DelegateInvoke Delegate { get; set; }
       
        internal TypeEntity(Type type)
        {
            this.Type = type;
            this.Delegate = new DelegateInvoke(type);
        }
    }
}
