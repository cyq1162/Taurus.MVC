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
        private Delegate MethodDelegate { get; set; }
        internal DelegateInvoke(MethodInfo method)
        {
            var returnType = method.ReturnType;
            isVoid = returnType.Name == "Void";
            isStatic = method.IsStatic;
            if (!isVoid)
            {
                isReturnBool = returnType == typeof(bool);
                isReturnString = !isReturnBool && returnType == typeof(string);
            }
            //创建方法委托
            var dType = DelegateType.Get(method);
            if (method.IsStatic)
            {
                this.MethodDelegate = System.Delegate.CreateDelegate(dType, method);
            }
            else
            {
                this.MethodDelegate = DelegateEmit.CreateDelegate(dType, method);
            }
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
                parameters = objects;
            }
            if (isVoid)
            {
                InvokeAction(parameters);
                return null;
            }
            else
            {
                return InvokeFunc(parameters);
            }
        }

        private void InvokeAction(params object[] parameters)
        {
            var d = this.MethodDelegate;
            //Default
            if (d is Action)
            {
                ((Action)d)();
                return;
            }
            //EndInvoke，无参数
            if (d is Action<Controller>)
            {
                ((Action<Controller>)d)(parameters[0] as Controller);
                return;
            }

            if (d is Action<Controller,string>)
            {
                ((Action<Controller, string>)d)(parameters[0] as Controller,parameters[1] as string);
                return;
            }
            if (d is Action<Controller, string, string>)
            {
                ((Action<Controller, string, string>)d)(parameters[0] as Controller, parameters[1] as string, parameters[2] as string);
                return;
            }
            if (d is Action<Controller, string, string, string>)
            {
                ((Action<Controller, string, string, string>)d)(parameters[0] as Controller, parameters[1] as string, parameters[2] as string, parameters[3] as string);
                return;
            }
            if (d is Action<Controller, int>)
            {
                ((Action<Controller, int>)d)(parameters[0] as Controller, Convert.ToInt32(parameters[1]));
                return;
            }
            if (d is Action<Controller, int, int>)
            {
                ((Action<Controller, int, int>)d)(parameters[0] as Controller, Convert.ToInt32(parameters[1]), Convert.ToInt32(parameters[2]));
                return;
            }
            if (d is Action<Controller, int, int, int>)
            {
                ((Action<Controller, int, int, int>)d)(parameters[0] as Controller, Convert.ToInt32(parameters[1]), Convert.ToInt32(parameters[2]), Convert.ToInt32(parameters[3]));
                return;
            }

            //其它的用动态调用处理。
            d.DynamicInvoke(parameters);
        }

        private object InvokeFunc(params object[] parameters)
        {
            var d = this.MethodDelegate;

            //先处理系统定义的
            if (isReturnBool)
            {
                //Static： BeforeInvoke
                if (d is Func<Controller, bool>)
                {
                    var fun = (Func<Controller, bool>)d;
                    return fun(parameters[0] as Controller);
                }
                //Static：CheckAck、CheckToken、CheckMicroService
                if (d is Func<Controller, string, bool>)
                {
                    var fun = (Func<Controller, string, bool>)d;
                    return fun(parameters[0] as Controller, parameters[1] as string);
                }
                //BeforeInvoke
                if (d is Func<bool>)
                {
                    var fun = (Func<bool>)d;
                    return fun();
                }
                //CheckAck、CheckToken、CheckMicroService
                if (d is Func<string, bool>)
                {
                    var fun = (Func<string, bool>)d;
                    return fun(parameters[0] as string);
                }

            }
            if (isReturnString)
            {
                //RouteMapInvoke
                if (d is Func<HttpRequest, string>)
                {
                    var fun = (Func<HttpRequest, string>)d;
                    return fun(parameters[0] as HttpRequest);
                }
            }
            return d.DynamicInvoke(parameters);
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
        private DelegateEmit.CreateControllerDelegate ControllerDelegate { get; set; }
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
            return this.ControllerDelegate();
        }

    }
}
