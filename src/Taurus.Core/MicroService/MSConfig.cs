using System;
using System.Web;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using CYQ.Data;
using CYQ.Data.Table;
using CYQ.Data.Tool;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace Taurus.MicroService
{
    /// <summary>
    /// 对应【AppSettings】可配置项
    /// </summary>
    public class MsConfig
    {
        /// <summary>
        /// 是否退出应用程序
        /// </summary>
        internal static bool IsApplicationExit = false;

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
                return AppConfig.GetApp("MicroService.Server.RegUrl", "").TrimEnd('/');
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
                return AppConfig.GetApp("MicroService.Client.RegUrl", "").TrimEnd('/');
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
                return AppConfig.GetApp("MicroService.App.RunUrl", "").TrimEnd('/');
            }
            set
            {
                AppConfig.SetApp("MicroService.App.RunUrl", value);
            }
        }
        /// <summary>
        /// 应用配置：开启应用程序远程退出功能【客户端默认开启、服务端默认关闭】
        /// </summary>
        public static bool RemoteExit
        {
            get
            {
                return AppConfig.GetAppBool("MicroService.App.RemoteExit", IsClient);
            }
            set
            {
                AppConfig.SetApp("MicroService.App.RemoteExit", value.ToString());
            }
        }
        /// <summary>
        /// 应用配置：Https 证书 存放路径【客户端默认开启、服务端默认关闭】
        /// </summary>
        public static string SslPath
        {
            get
            {
                return AppConfig.GetApp("MicroService.App.SslPath", "/App_Data/ssl");
            }
            set
            {
                AppConfig.SetApp("MicroService.App.SslPath", value);
            }
        }
        /// <summary>
        /// 获取应用证书【证书路径由SslPath配置】（只读）
        /// </summary>
        public static Dictionary<string, X509Certificate2> SslCertificate
        {
            get
            {
                var certificates = new Dictionary<string, X509Certificate2>(StringComparer.OrdinalIgnoreCase);
                string sslFolder = AppConfig.WebRootPath + SslPath;
                if (Directory.Exists(sslFolder))
                {
                    string[] files = Directory.GetFiles(sslFolder, "*.pfx", SearchOption.TopDirectoryOnly);
                    foreach (string file in files)
                    {
                        string pwdPath = file.Replace(".pfx", ".txt");
                        if (File.Exists(pwdPath))
                        {
                            string pwd = IOHelper.ReadAllText(pwdPath);
                            string domain = Path.GetFileName(pwdPath).Replace(".txt", "");
                            certificates.Add(domain, new X509Certificate2(file, pwd));
                        }
                    }
                }
                return certificates;
            }
        }

        ///// <summary>
        ///// 应用配置：应用程序绑定域名
        ///// </summary>
        //public static bool Domain
        //{
        //    get
        //    {
        //        return AppConfig.GetAppBool("MicroService.App.Domain", false);
        //    }
        //    set
        //    {
        //        AppConfig.SetApp("MicroService.App.Domain", value.ToString());
        //    }
        //}
        #endregion


        #region 只读属性

        /// <summary>
        /// 当前程序是否作为客务端运行：微服务应用程序
        /// </summary>
        public static bool IsClient
        {
            get
            {
                return !string.IsNullOrEmpty(MsConfig.ClientName) && !string.IsNullOrEmpty(MsConfig.ClientRegUrl) && MsConfig.ClientRegUrl != MsConfig.AppRunUrl;
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
                return MsConfig.ServerName.ToLower() == MsConst.RegCenter;
            }
        }
        /// <summary>
        /// 是否网关中心
        /// </summary>
        public static bool IsGateway
        {
            get
            {
                return MsConfig.ServerName.ToLower() == MsConst.Gateway;
            }
        }
        /// <summary>
        /// 是否注册中心（主）
        /// </summary>
        public static bool IsRegCenterOfMaster
        {
            get
            {
                return MsConfig.ServerName.ToLower() == MsConst.RegCenter && (string.IsNullOrEmpty(MsConfig.ServerRegUrl) || MsConfig.ServerRegUrl == MsConfig.AppRunUrl);
            }
        }
        #endregion


        #region 注册中心 - 数据库配置

        //private static string _MsConn = null;
        ///// <summary>
        ///// 微服务 - 注册中心  数据库链接配置
        ///// </summary>
        //public static string MsConn
        //{
        //    get
        //    {
        //        if (_MsConn == null)
        //        {
        //            _MsConn = AppConfig.GetConn("MsConn");
        //        }
        //        return _MsConn;
        //    }
        //    set
        //    {
        //        _MsConn = value;
        //    }
        //}

        //private static string _MsTableName;
        ///// <summary>
        ///// 异常日志表名（默认为MsRegCenter，可配置）
        ///// </summary>
        //public static string MsTableName
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(_MsTableName))
        //        {
        //            _MsTableName = AppConfig.GetApp("MsTableName", "MsRegCenter");
        //        }
        //        return _MsTableName;
        //    }
        //    set
        //    {
        //        _MsTableName = value;
        //    }
        //}
        #endregion
    }
}
