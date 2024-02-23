using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace Taurus.Mvc.Reflect
{
    /// <summary>
    /// 方法调用：实例委托
    /// </summary>
    public partial class DelegateInvoke
    {
        private bool isVoid = false;
        private bool isStatic = false;
        private bool isReturnBool = false;
        private bool isReturnString = false;
        /// <summary>
        /// 方法委托
        /// </summary>
        private CreateMethodDelegate MethodDelegate { get; set; }
        internal DelegateInvoke(MethodInfo method)
        {
            isStatic = method.IsStatic;
            this.MethodDelegate = DelegateEmit.GetCreateMethodDelegate(method);
        }

        /// <summary>
        /// 调用委托方法
        /// </summary>
        /// <param name="obj">实例对象或Null</param>
        /// <param name="parameters">参数</param>
        /// <returns></returns>
        public object Invoke(object obj, object[] parameters)
        {
            //重组参数：
            if (!isStatic)
            {
                int length = parameters == null ? 1 : parameters.Length + 1;
                object[] objects = new object[length];
                objects[0] = obj;
                if (parameters != null && parameters.Length > 0)
                {
                    for (int i = 0; i < parameters.Length; i++)
                    {
                        objects[i + 1] = parameters[i];
                    }
                }
                return this.MethodDelegate(objects);
            }

            return this.MethodDelegate(parameters);
        }

    }

    /// <summary>
    /// 创建控制器：实例委托
    /// </summary>
    public partial class DelegateInvoke
    {
        /// <summary>
        /// Controller 实例创建委托
        /// </summary>
        private CreateControllerDelegate ControllerDelegate { get; set; }
        internal DelegateInvoke(Type type)
        {
            this.ControllerDelegate = DelegateEmit.GetCreateControllerDelegate(type);
        }
        /// <summary>
        /// 创建控制器实例
        /// </summary>
        /// <returns></returns>
        public Controller CreateController()
        {
            if (this.ControllerDelegate != null)
            {
                return this.ControllerDelegate();
            }
            return null;
        }

    }
}
