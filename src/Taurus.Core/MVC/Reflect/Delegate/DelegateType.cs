using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Taurus.Mvc.Reflect
{
    /// <summary>
    /// 提供委托类型的方法创建和调用
    /// </summary>
    internal static partial class DelegateType
    {
        /// <summary>
        /// 获取委托类型【实例方法需要+实例对象参数、静态方法不需要】
        /// </summary>
        public static Type Get(MethodInfo mi)
        {
            bool isVoidReturn = mi.ReturnType.Name == "Void";
            var paras = mi.GetParameters();
            int length = paras.Length + (!mi.IsStatic ? 1 : 0) + (isVoidReturn ? 0 : 1);
            var paraTypes = new Type[length];
            if (mi.IsStatic)
            {
                if (paras.Length > 0)
                {
                    for (int i = 0; i < paras.Length; i++)
                    {
                        paraTypes[i] = paras[i].ParameterType;
                    }
                }
            }
            else
            {
                paraTypes[0] = typeof(Controller);//预留第1个控制器位置。
                if (paras.Length > 0)
                {
                    for (int i = 0; i < paras.Length; i++)
                    {
                        paraTypes[i + 1] = paras[i].ParameterType;
                    }
                }
            }
            if (!isVoidReturn)
            {
                paraTypes[paraTypes.Length - 1] = mi.ReturnType;
            }

            if (isVoidReturn)
            {
                return GetActionType(paraTypes.Length).MakeGenericType(paraTypes);
            }
            else
            {
                return GetFuncType(paraTypes.Length).MakeGenericType(paraTypes);
            }
        }
        private static Type GetActionType(int paraCount)
        {
            switch (paraCount)
            {
                case 0: return typeof(Action);
                case 1: return typeof(Action<>);
                case 2: return typeof(Action<,>);
                case 3: return typeof(Action<,,>);
                case 4: return typeof(Action<,,,>);
                case 5: return typeof(Action<,,,,>);
                case 6: return typeof(Action<,,,,,>);
                case 7: return typeof(Action<,,,,,,>);
                case 8: return typeof(Action<,,,,,,,>);
                case 9: return typeof(Action<,,,,,,,,>);
                case 10: return typeof(Action<,,,,,,,,,>);
                case 11: return typeof(Action<,,,,,,,,,,>);
                case 12: return typeof(Action<,,,,,,,,,,,>);
                case 13: return typeof(Action<,,,,,,,,,,,,>);
                case 14: return typeof(Action<,,,,,,,,,,,,,>);
                case 15: return typeof(Action<,,,,,,,,,,,,,,>);
                case 16: return typeof(Action<,,,,,,,,,,,,,,,>);
                default:
                    throw new Exception("parameter count must < 17.");
            }
        }
        private static Type GetFuncType(int paraCount)
        {
            switch (paraCount)
            {
                case 1: return typeof(Func<>);
                case 2: return typeof(Func<,>);
                case 3: return typeof(Func<,,>);
                case 4: return typeof(Func<,,,>);
                case 5: return typeof(Func<,,,,>);
                case 6: return typeof(Func<,,,,,>);
                case 7: return typeof(Func<,,,,,,>);
                case 8: return typeof(Func<,,,,,,,>);
                case 9: return typeof(Func<,,,,,,,,>);
                case 10: return typeof(Func<,,,,,,,,,>);
                case 11: return typeof(Func<,,,,,,,,,,>);
                case 12: return typeof(Func<,,,,,,,,,,,>);
                case 13: return typeof(Func<,,,,,,,,,,,,>);
                case 14: return typeof(Func<,,,,,,,,,,,,,>);
                case 15: return typeof(Func<,,,,,,,,,,,,,,>);
                case 16: return typeof(Func<,,,,,,,,,,,,,,,>);
                case 17: return typeof(Func<,,,,,,,,,,,,,,,,>);
                default:
                    throw new Exception("parameter count must < 18.");

            }
        }
    }
}
