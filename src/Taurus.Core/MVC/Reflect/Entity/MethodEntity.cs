using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Taurus.Mvc.Reflect
{
    /// <summary>
    /// 方法元数据实体
    /// </summary>
    public class MethodEntity
    {
        /// <summary>
        /// 反射方法，Invoke调用
        /// </summary>
        public MethodInfo Method { get; set; }
        /// <summary>
        /// 方法的特性
        /// </summary>
        public AttributeEntity AttrEntity { get; set; }
        /// <summary>
        /// 方法的参数
        /// </summary>
        public ParameterInfo[] Parameters;

        /// <summary>
        /// 方法委托
        /// </summary>
        public DelegateInvoke Delegate { get; set; }

        internal MethodEntity(MethodInfo method, AttributeEntity attributeEntity)
        {
            Method = method;
            Parameters = method.GetParameters();
            AttrEntity = attributeEntity;
            Delegate = new DelegateInvoke(method);
        }
    }



}
