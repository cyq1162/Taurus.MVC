using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Core
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class TokenAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpGetAttribute : Attribute
    {

    }
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class HttpPostAttribute : Attribute
    {

    }
}
