using System;

namespace Taurus.MicroService
{
    /// <summary>
    /// 存档请求的客户端信息
    /// </summary>
    internal class HostInfo
    {
        /// <summary>
        /// 主机地址：http://localhost:8080
        /// </summary>
        public string Host { get; set; }
        /// <summary>
        /// 版本号：用于版本升级。
        /// </summary>
        public int Version { get; set; }
        /// <summary>
        /// 注册时间（最新）
        /// </summary>
        public DateTime RegTime { get; set; }
        /// <summary>
        /// 记录调用时间，用于隔离无法调用的服务，延时调用。
        /// </summary>
        public DateTime CallTime { get; set; }
        /// <summary>
        /// 记录调用顺序，用于负载均衡
        /// </summary>
        public int CallIndex { get; set; }

    }
}
