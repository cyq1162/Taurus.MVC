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
        /// 方法的调用路径
        /// </summary>
        public string RouteUrl { get; set; }
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

        internal MethodEntity(TypeEntity typeEntity, MethodInfo method, AttributeEntity attributeEntity, string routeUrl)
        {
            this.TypeEntity = typeEntity;
            this.Method = method;
            this.Parameters = method.GetParameters();
            this.AttrEntity = attributeEntity;
            this.RouteUrl = routeUrl.ToLower();
            this.Delegate = new DelegateInvoke(method);
            InitParameter();

        }

        /// <summary>
        /// 返回一个当前支持的HttpMethod
        /// </summary>
        /// <returns></returns>
        public string GetHttpMethod()
        {
            if (AttrEntity.HasPost) { return "POST"; }
            if (AttrEntity.HasGet) { return "GET"; }
            if (AttrEntity.HasHead) { return "HEAD"; }
            if (AttrEntity.HasPut) { return "PUT"; }
            if (AttrEntity.HasDelete) { return "DELETE"; }
            if (Parameters != null)
            {
                foreach (var para in Parameters)
                {
                    if (para.ParameterType.IsValueType || para.ParameterType.Name == "String")
                    {
                        continue;
                    }
                    return "POST";
                }
            }
            return "GET";
        }

        /// <summary>
        /// 是否允许指定的HttpMethod请求
        /// </summary>
        /// <param name="httpMethod">get、post、head、put、delete</param>
        /// <returns></returns>
        public bool IsAllowHttpMethod(string httpMethod)
        {
            // internal static string[] HttpMethods = new string[] { "GET", "POST", "HEAD", "PUT", "DELETE" };
            if (string.IsNullOrEmpty(httpMethod)) { return false; }

            if (!AttrEntity.HasGet && !AttrEntity.HasPost && !AttrEntity.HasHead && !AttrEntity.HasPut && !AttrEntity.HasDelete)//无配置，则都可以。
            {
                return true;
            }
            switch (httpMethod.ToLower())
            {
                case "get":
                    return AttrEntity.HasGet;
                case "post":
                    return AttrEntity.HasPost;
                case "head":
                    return AttrEntity.HasHead;
                case "put":
                    return AttrEntity.HasPut;
                case "delete":
                    return AttrEntity.HasDelete;
            }
            return false;

        }

        /// <summary>
        /// 参数是否必填项，通过【Requeire参数约束】
        /// </summary>
        /// <param name="parameterInfo"></param>
        /// <returns></returns>
        public bool IsRequire(ParameterInfo parameterInfo)
        {
            if (parameterInfo != null && AttrEntity.HasRequire)
            {
                foreach (var item in AttrEntity.RequireAttributes)
                {
                    if (string.Compare(item.paraName, parameterInfo.Name, true) == 0)
                    {
                        return true;
                    }
                }
            }
            return false;
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
