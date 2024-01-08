using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Mvc.Attr
{
    /// <summary>
    /// 用于（忽略默认全局控制器的事件方法）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = true)]
    public class IgnoreGlobalControllerAttribute : Attribute
    {

    }
   
}
