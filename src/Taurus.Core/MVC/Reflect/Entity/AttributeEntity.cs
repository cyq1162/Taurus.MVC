using Taurus.Mvc.Attr;

namespace Taurus.Mvc.Reflect
{
    /// <summary>
    /// 属性元数据实体
    /// </summary>
    public class AttributeEntity
    {
        public bool HasToken { get; set; }
        public bool HasAck { get; set; }
        public bool HasMicroService { get; set; }
        public bool HasIgnoreGlobalController { get; set; }
        public bool HasWebSocket { get; set; }
        public bool HasGet { get; set; }
        public bool HasPost { get; set; }
        public bool HasHead { get; set; }
        public bool HasPut { get; set; }
        public bool HasDelete { get; set; }

        public bool HasRoute { get; set; }

        public RouteAttribute[] RouteAttributes { get; set; }

        public bool HasRequire { get; set; }
        public RequireAttribute[] RequireAttributes { get; set; }
        /// <summary>
        /// 所有属性值
        /// </summary>
        public object[] Attributes { get; set; }
    }
}
