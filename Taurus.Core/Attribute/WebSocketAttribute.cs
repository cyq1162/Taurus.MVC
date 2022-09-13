using System;

namespace Taurus.Mvc.Attr
{
    /// <summary>
    /// WebSocket标识，标记后仅允许该控制器方法使用WebSocket
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = true)]
    public class WebSocketAttribute : Attribute
    {

    }

}
