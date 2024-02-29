using CYQ.Data.Emit;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Mvc.Reflect
{
    internal class EntityPreheat
    {
        public static void InitType(Type type, Dictionary<Type, bool> dic)
        {
            if (type.IsValueType) { return; }
            if (dic == null)
            {
                //处理继承自 CYQ.Data.Orm 的实体类
                ReflectTool.GetPropertyList(type);
                ReflectTool.GetFieldList(type);
                return;
            }
            if (!type.IsGenericType)
            {
                var name = type.FullName;
                if (name.StartsWith("System.") || name.StartsWith("Microsoft.") || name.StartsWith("Taurus.Mvc.")) { return; }
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
        public static void InitDelegate(Type type)
        {
            EmitPreheat.Add(type);
        }
    }
}
