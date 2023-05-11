

using System.Net;
using System;
using System.Diagnostics;
using CYQ.Data;
using System.Net.NetworkInformation;
using System.Net.Sockets;

namespace Taurus.Mvc
{
    /// <summary>
    /// Taurus Mvc 常量
    /// </summary>
    public static class MvcConst
    {
        private static string _Version;
        /// <summary>
        /// 获取当前 Taurus 版本号
        /// </summary>
        public static string Version
        {
            get
            {
                if (_Version == null)
                {
                    _Version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
                }
                return _Version;
            }
        }
        private static int _ProcessID;
        /// <summary>
        /// 当前进程ID
        /// </summary>
        public static int ProcessID
        {
            get
            {
                if (_ProcessID == 0)
                {
                    _ProcessID = Process.GetCurrentProcess().Id;
                }
                return _ProcessID;
            }
        }

        private static string _HostIP;
        /// <summary>
        /// 本机内网IP，若无，则返回主机名
        /// </summary>
        public static string HostIP
        {
            get
            {
                if (string.IsNullOrEmpty(_HostIP))
                {
                    var nets = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (var item in nets)
                    {
                        var ips = item.GetIPProperties().UnicastAddresses;
                        foreach (var ip in ips)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
                            {
                                string ipAddr = ip.Address.ToString();
                                if (ipAddr.EndsWith(".1") || ipAddr.Contains(":")) // 忽略路由和网卡地址。
                                {
                                    continue;
                                }
                                _HostIP = ipAddr;
                                break;
                            }
                        }
                    }
                }
                return _HostIP ?? "127.0.0.1";
            }
        }

        /// <summary>
        /// 应用程序受保护的目录路径：App_Data目录路径。
        /// </summary>
        public static string AppDataFolderPath
        {
            get
            {
                return AppConfig.WebRootPath + "/App_Data/";
            }
        }
    }
}
