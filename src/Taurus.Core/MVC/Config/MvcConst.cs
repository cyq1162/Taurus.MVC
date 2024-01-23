

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
        private static Process _Proc;
        /// <summary>
        /// 当前进程ID
        /// </summary>
        internal static Process Proc
        {
            get
            {
                if (_Proc == null)
                {
                    _Proc = Process.GetCurrentProcess();
                }
                return _Proc;
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
                return Proc.Id;
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
                    bool isSupportDADS = true;
                    var nets = NetworkInterface.GetAllNetworkInterfaces();
                    foreach (var item in nets)
                    {
                        // 跳过虚拟机网卡
                        if (item.Description.StartsWith("VirtualBox ") || item.Description.StartsWith("Hyper-V") || item.Description.StartsWith("VMware ") || item.Description.StartsWith("Bluetooth "))
                        {
                            continue;
                        }
                        var ips = item.GetIPProperties().UnicastAddresses;
                        foreach (var ip in ips)
                        {
                            if (ip.Address.AddressFamily == AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip.Address))
                            {
                                try
                                {
                                    if (isSupportDADS && ip.DuplicateAddressDetectionState != DuplicateAddressDetectionState.Preferred)
                                    {
                                        continue;
                                    }
                                }
                                catch (PlatformNotSupportedException err)
                                {
                                    isSupportDADS = false;
                                }
                                string ipAddr = ip.Address.ToString();
                                if (ipAddr.EndsWith(".1") || ipAddr.Contains(":")) // 忽略路由和网卡地址。
                                {
                                    continue;
                                }
                                _HostIP = ipAddr;
                                return _HostIP;
                            }
                        }
                    }
                }
                return _HostIP ?? "127.0.0.1";
            }
        }
    }
}
