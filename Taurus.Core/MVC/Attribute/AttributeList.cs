using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Core
{
    /// <summary>
    /// 框架内部支持的属性
    /// </summary>
    internal class AttributeList
    {
        public bool HasToken { get; set; }
        public bool HasAck { get; set; }
        public bool HasMicroService { get; set; }
        public bool HasGet { get; set; }
        public bool HasPost { get; set; }
        public bool HasHead { get; set; }
        public bool HasPut { get; set; }
        public bool HasDelete { get; set; }

        /// <summary>
        /// 是否包含指定的key
        /// </summary>
        /// <param name="key">token、ack、get、post、head、put、delete</param>
        /// <returns></returns>
        public bool HasKey(string key)
        {
            // internal static string[] HttpMethods = new string[] { "GET", "POST", "HEAD", "PUT", "DELETE" };
            if (string.IsNullOrEmpty(key)) { return false; }
            switch (key.ToLower())
            {
                case "token":
                    return HasToken;
                case "ack":
                    return HasAck;
                case "microservice":
                    return HasMicroService;
                default:
                    if (!HasGet && !HasPost && !HasHead && !HasPut && !HasDelete)//无配置，则都可以。
                    {
                        return true;
                    }
                    else
                    {
                        switch (key.ToLower())
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
}
