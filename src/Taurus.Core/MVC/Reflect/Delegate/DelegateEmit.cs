using System;
using System.Collections.Generic;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;

namespace Taurus.Mvc.Reflect
{
    /// <summary>
    /// Emit 实现动态委托
    /// </summary>
    internal class DelegateEmit
    {

        #region 创建实例委托

        /// <summary>
        /// 用于创建 Controller 实例的委托
        /// </summary>
        /// <returns></returns>
        public delegate Controller CreateControllerDelegate();
        /// <summary>
        /// 创建实例委托
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public static CreateControllerDelegate GetCreateControllerDelegate(Type type)
        {
            var cm = CreateControllerEmit(type);
            return cm.CreateDelegate(typeof(CreateControllerDelegate)) as CreateControllerDelegate;
        }

        private static DynamicMethod CreateControllerEmit(Type tType)
        {
            DynamicMethod method = new DynamicMethod("ExecuteCreateController", tType, null, tType);
            ILGenerator gen = method.GetILGenerator();//开始编写IL方法。
            gen.DeclareLocal(typeof(Controller));//0
            gen.Emit(OpCodes.Newobj, tType.GetConstructor(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null, new Type[] { }, null));
            gen.Emit(OpCodes.Stloc_0);//t= new T();
            gen.Emit(OpCodes.Ldloc_0);
            gen.Emit(OpCodes.Ret);
            return method;
        }

        #endregion

        /// <summary>
        /// 创建方法实例委托【非静态方法】
        /// </summary>
        /// <param name="delegateType">委托类型</param>
        /// <param name="mi">方法</param>
        /// <returns></returns>
        public static Delegate CreateDelegate(Type delegateType, MethodInfo mi)
        {
            var mie = MethodInvokeEmit(mi);
            return mie.CreateDelegate(delegateType);
        }

        #region 创建方法委托
        private static DynamicMethod MethodInvokeEmit(MethodInfo mi)
        {
            var paras = mi.GetParameters();
            var types = new Type[paras.Length + 1];
            types[0] = typeof(Controller);
            for (int i = 0; i < paras.Length; i++)
            {
                types[i + 1] = paras[i].ParameterType;
            }

            DynamicMethod method = new DynamicMethod("ExecuteMethodInvoke", mi.ReturnType, types, mi.DeclaringType);
            ILGenerator gen = method.GetILGenerator();//开始编写IL方法。
            for (int i = 0; i < types.Length; i++)
            {
                gen.Emit(OpCodes.Ldarg_S, i);
            }
            gen.Emit(OpCodes.Callvirt, mi);

            gen.Emit(OpCodes.Ret);
            return method;
        }

        #endregion


    }
}
