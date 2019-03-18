using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TokenAttribute : Attribute
    {

    }
    #region HttpMethod

    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpGetAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPostAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpHeadAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPutAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpDeleteAttribute : Attribute
    {

    }
    #endregion
}
