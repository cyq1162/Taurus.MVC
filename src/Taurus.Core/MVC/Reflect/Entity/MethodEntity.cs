using CYQ.Data.Tool;
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
        /// 方法的实体属性
        /// </summary>
        public TypeEntity TypeEntity { get; set; }
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

        internal MethodEntity(TypeEntity typeEntity, MethodInfo method, AttributeEntity attributeEntity)
        {
            this.TypeEntity = typeEntity;
            this.Method = method;
            this.Parameters = method.GetParameters();
            this.AttrEntity = attributeEntity;
            this.Delegate = new DelegateInvoke(method);
            InitParameter();
        }
        private void InitParameter()
        {
            Dictionary<Type, bool> dic = new Dictionary<Type, bool>();
            if (this.Parameters != null && this.Parameters.Length > 0)
            {
                foreach (var item in Parameters)
                {
                    EntityPreheat.InitType(item.ParameterType, dic);
                }
            }
            var rType = this.Method.ReturnType;
            if (!rType.IsValueType)
            {
                EntityPreheat.InitType(rType, dic);
            }
        }

    }



}
