using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Mvc.Attr
{
    /// <summary>
    /// 用于效验（需要登陆）身份是否合法（存在时触发CheckToken方法）
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TokenAttribute : Attribute
    {

    }
   
}
