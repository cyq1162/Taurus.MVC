using System;


namespace Taurus.Plugin.DistributedTransaction
{
    /// <summary>
    /// DTC 运行启动
    /// </summary>
    public partial class DTC
    {
        public static partial class Client
        {
            /// <summary>
            /// 启动定时描述，并监听默认队列。
            /// </summary>
            public static void Start()
            {
                if (DTCConfig.Client.IsEnable)
                {
                    DTCLog.WriteDebugLine("DTC.Client.Start = true");
                    DTC.Client.Worker.DBScanner.Start();
                }
            }
        }
        public static partial class Server
        { /// <summary>
          /// 启动定时描述，并监听默认队列。
          /// </summary>
            public static void Start()
            {
                if (DTCConfig.Server.IsEnable)
                {
                    DTCLog.WriteDebugLine("DTC.Server.Start = true");
                    DTC.Server.Worker.DBScanner.Start();
                }
            }
        }

        /// <summary>
        /// 同时启动客户端和服务端定时扫描程序。
        /// </summary>
        public static void Start()
        {
            DTCLog.WriteDebugLine("--------------------------------------------------");
            Client.Start();
            Server.Start();
            DTCLog.WriteDebugLine("--------------------------------------------------");
        }
    }

}
