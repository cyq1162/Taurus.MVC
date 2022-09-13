using Taurus.Mvc.Attr;

namespace Taurus.Mvc
{
    /// <summary>
    /// 框架内部支持的属性
    /// </summary>
    internal class AttributeEntity
    {
        public bool HasToken { get; set; }
        public bool HasAck { get; set; }
        public bool HasMicroService { get; set; }
        public bool HasIgnoreDefaultController { get; set; }
        public bool HasWebSocket { get; set; }
        public bool HasGet { get; set; }
        public bool HasPost { get; set; }
        public bool HasHead { get; set; }
        public bool HasPut { get; set; }
        public bool HasDelete { get; set; }

        public bool HasRequire { get; set; }
        public RequireAttribute[] RequireAttributes { get; set; }

        /// <summary>
        /// 是否允许指定的HttpMethod请求
        /// </summary>
        /// <param name="httpMethod">get、post、head、put、delete</param>
        /// <returns></returns>
        public bool IsAllowHttpMethod(string httpMethod)
        {
            // internal static string[] HttpMethods = new string[] { "GET", "POST", "HEAD", "PUT", "DELETE" };
            if (string.IsNullOrEmpty(httpMethod)) { return false; }

            if (!HasGet && !HasPost && !HasHead && !HasPut && !HasDelete)//无配置，则都可以。
            {
                return true;
            }
            else
            {
                switch (httpMethod.ToLower())
                {
                    case "get":
                        return HasGet;
                    case "post":
                        return HasPost;
                    case "head":
                        return HasHead;
                    case "put":
                        return HasPut;
                    case "delete":
                        return HasDelete;
                }
            }
            return false;

        }
    }
}
