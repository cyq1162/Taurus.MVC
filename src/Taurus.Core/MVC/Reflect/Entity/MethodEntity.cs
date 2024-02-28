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
                    InitType(item.ParameterType, dic);
                }
            }
            var rType = this.Method.ReturnType;
            if (!rType.IsValueType)
            {
                InitType(rType, dic);
            }
        }
        private void InitType(Type type, Dictionary<Type, bool> dic)
        {
            if (type.IsValueType) { return; }
            if (!type.IsGenericType)
            {
                var name = type.FullName;
                if (name.StartsWith("System.") || name.StartsWith("Microsoft.")) { return; }
            }
            //保存过程，避免死循环。
            if (dic.ContainsKey(type)) { return; }
            dic.Add(type, true);
            var t = type;
            var sysType = ReflectTool.GetSystemType(ref type);
            if (sysType == SysType.Custom)
            {
                ReflectTool.GetPropertyList(type);
                ReflectTool.GetFieldList(type);
            }
            Type[] args;
            ReflectTool.GetArgumentLength(ref t, out args);
            if (args != null && args.Length > 0)
            {
                foreach (var arg in args)
                {
                    InitType(arg, dic);
                }
            }
        }
    }



}
