﻿using System;
using System.Web;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Table;
using CYQ.Data.Tool;

namespace Taurus.MicroService
{
    /// <summary>
    /// 对应【AppSettings】可配置项
    /// </summary>
    public class MSConfig
    {
        #region AppSetting 配置

        /// <summary>
        /// 网关或注册中心配置：服务端模块名称【可配置：Gateway或RegCenter】
        /// </summary>
        public static string ServerName
        {
            get
            {
                return AppConfig.GetApp("MicroService.Server.Name");
            }
            set
            {
                AppConfig.SetApp("MicroService.Server.Name", value);
            }
        }

        /// <summary>
        /// 网关或注册中心配置：注册中心地址【示例：http://localhost:9999】
        /// </summary>
        public static string ServerRegUrl
        {
            get
            {
                return AppConfig.GetApp("MicroService.Server.RegUrl");
            }
            set
            {
                AppConfig.SetApp("MicroService.Server.RegUrl", value);
            }
        }
        /// <summary>
        /// 网关或注册中心配置：系统间调用密钥串【任意字符串】
        /// </summary>
        public static string ServerKey
        {
            get
            {
                return AppConfig.GetApp("MicroService.Server.Key", "Taurus.MicroService");
            }
            set
            {
                AppConfig.SetApp("MicroService.Server.Key", value);
            }
        }
        /// <summary>
        /// 微服务应用配置：系统间调用密钥串【任意字符串】
        /// </summary>
        public static string ClientKey
        {
            get
            {
                return AppConfig.GetApp("MicroService.Client.Key", "Taurus.MicroService");
            }
            set
            {
                AppConfig.SetApp("MicroService.Client.Key", value);
            }
        }
        /// <summary>
        /// 微服务应用配置：客户端模块名称【示例：Test】
        /// </summary>
        public static string ClientName
        {
            get
            {
                return AppConfig.GetApp("MicroService.Client.Name");
            }
            set
            {
                AppConfig.SetApp("MicroService.Client.Name", value);
            }
        }

        /// <summary>
        /// 微服务应用配置：注册中心的Url
        /// </summary>
        public static string ClientRegUrl
        {
            get
            {
                return AppConfig.GetApp("MicroService.Client.RegUrl");
            }
            set
            {
                AppConfig.SetApp("MicroService.Client.RegUrl", value);
            }
        }

        /// <summary>
        /// 微服务应用配置：客户端模块版本号（用于版本间升级）【示例：1】
        /// </summary>
        public static int ClientVersion
        {
            get
            {
                return AppConfig.GetAppInt("MicroService.Client.Version", 1);
            }
            set
            {
                AppConfig.SetApp("MicroService.Client.Version", value.ToString());
            }
        }

        /// <summary>
        /// 应用配置：当前运行Url【Kestrel启动运行需要】
        /// </summary>
        public static string AppRunUrl
        {
            get
            {
                return AppConfig.GetApp("MicroService.App.RunUrl");
            }
            set
            {
                AppConfig.SetApp("MicroService.App.RunUrl", value);
            }
        }
        #endregion


        #region 只读属性

        /// <summary>
        /// 当前程序是否作为客务端运行：微服务应用程序
        /// </summary>
        public static bool IsClient
        {
            get
            {
                return !string.IsNullOrEmpty(MSConfig.ClientName) && !string.IsNullOrEmpty(MSConfig.ClientRegUrl) && MSConfig.ClientRegUrl != MSConfig.AppRunUrl;
            }
        }

        /// <summary>
        /// 当前程序是否作为服务端运行：网关或注册中心
        /// </summary>
        public static bool IsServer
        {
            get
            {
                return IsRegCenter || IsGateway;
            }
        }
        /// <summary>
        /// 是否注册中心
        /// </summary>
        public static bool IsRegCenter
        {
            get
            {
                return MSConfig.ServerName.ToLower() == MSConst.RegCenter;
            }
        }
        /// <summary>
        /// 是否网关中心
        /// </summary>
        public static bool IsGateway
        {
            get
            {
                return MSConfig.ServerName.ToLower() == MSConst.Gateway;
            }
        }
        /// <summary>
        /// 是否注册中心（主）
        /// </summary>
        public static bool IsRegCenterOfMaster
        {
            get
            {
                return MSConfig.ServerName.ToLower() == MSConst.RegCenter && (string.IsNullOrEmpty(MSConfig.ServerRegUrl) || MSConfig.ServerRegUrl == MSConfig.AppRunUrl);
            }
        }
        #endregion
    }
}