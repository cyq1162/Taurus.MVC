using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Mvc.Attr
{
    #region HttpMethod
    /// <summary>
    /// 自动效验是否Get请求
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpGetAttribute : Attribute
    {

    }
    /// <summary>
    /// 自动效验是否Post请求
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPostAttribute : Attribute
    {

    }
    /// <summary>
    /// 自动效验是否Head请求
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpHeadAttribute : Attribute
    {

    }
    /// <summary>
    /// 自动效验是否Put请求
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPutAttribute : Attribute
    {

    }
    /// <summary>
    /// 自动效验是否Delete请求
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpDeleteAttribute : Attribute
    {

    }
    #endregion
}
