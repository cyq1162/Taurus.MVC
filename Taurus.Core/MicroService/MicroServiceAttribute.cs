using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Core
{
    /// <summary>
    /// 微服务标识，标记后仅允许微服务间内部调用
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class MicroServiceAttribute : Attribute
    {

    }

}
