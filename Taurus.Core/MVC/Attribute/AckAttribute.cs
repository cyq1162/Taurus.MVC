using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Core
{
    /// <summary>
    /// 用于效验（不需要登陆）请求来源是否合法（存在时触发CheckAck方法）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class AckAttribute : Attribute
    {

    }
   
}
