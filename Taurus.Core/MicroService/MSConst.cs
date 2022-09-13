﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.MicroService
{
    /// <summary>
  /// 常量
  /// </summary>
    internal class MSConst
    {
        /// <summary>
        /// 用于锁表。
        /// </summary>
        internal static readonly object tableLockObj = new object();
        /// <summary>
        /// 请求头带上的Header的Key名称
        /// </summary>
        public const string HeaderKey = "mskey";
        /// <summary>
        /// 网关
        /// </summary>
        public const string Gateway = "gateway";
        /// <summary>
        /// 注册中心
        /// </summary>
        public const string RegCenter = "regcenter";

        internal const string ServerHostListJsonPath = "MicroService_Server_HostList.json";
        internal const string ClientHostListJsonPath = "MicroService_Client_HostList.json";
        internal const string ServerHost2Path = "MicroService_Server_Host2.json";
        internal const string ClientHost2Path = "MicroService_Client_Host2.json";
    }
}