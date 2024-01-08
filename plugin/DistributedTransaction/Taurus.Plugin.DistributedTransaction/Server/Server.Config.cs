using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTCConfig
    {
        /// <summary>
        /// 服务端【接口提供端】配置项
        /// </summary>
        public static class Server
        {
            /// <summary>
            /// 配置是否启用 服务端【接口提供端】
            /// 如 DTC.Server.IsEnable ：true， 默认值：true
            /// </summary>
            public static bool IsEnable
            {
                get
                {
                    return AppConfig.GetAppBool("DTC.Server.IsEnable", true);
                }
                set
                {
                    AppConfig.SetApp("DTC.Server.IsEnable", value.ToString());
                }
            }


            /// <summary>
            /// DTC 类记录本地消息数据库 - 数据库链接配置
            /// 配置项：DTCServerConn：server=.;database=x;uid=s;pwd=p;
            /// </summary>
            public static string Conn
            {
                get
                {
                    return AppConfig.GetConn("DTCServerConn");
                }
                set
                {
                    AppConfig.SetConn("DTCServerConn", value);
                }
            }

            /// <summary>
            /// DTC 类记录本地消息数据库 - 表名
            /// 配置项：DTC.Server.TableName ：DTC_Server
            /// </summary>
            public static string TableName
            {
                get
                {
                    return AppConfig.GetApp("DTC.Server.TableName", "DTC_Server");
                }
                set
                {
                    AppConfig.SetApp("DTC.Server.TableName", value);
                }
            }
            /// <summary>
            /// RabbitMQ 链接配置
            /// 配置项：DTC.Server.Rabbit=127.0.0.1;guest;guest;/
            /// </summary>
            public static string Rabbit
            {
                get
                {
                    return AppConfig.GetApp("DTC.Server.Rabbit");
                }
                set
                {
                    AppConfig.SetApp("DTC.Server.Rabbit", value);
                }
            }
            /// <summary>
            /// Kafka 链接配置
            /// 配置项：DTC.Server.Kafka=127.0.0.1:9092
            /// </summary>
            public static string Kafka
            {
                get
                {
                    return AppConfig.GetApp("DTC.Server.Kafka");
                }
                set
                {
                    AppConfig.SetApp("DTC.Server.Kafka", value);
                }
            }
            /// <summary>
            /// MQ相关配置项
            /// </summary>
            public static class MQ
            {

                /// <summary>
                /// DTC 默认交换机名称，绑定所Default队列
                /// </summary>
                internal static string DefaultExChange
                {
                    get
                    {
                        return "DTC_Server_Default";
                    }
                }
                /// <summary>
                /// DTC 默认交换机名称，绑定所有Retry队列
                /// </summary>
                internal static string RetryExChange
                {
                    get
                    {
                        return "DTC_Server_Retry";
                    }
                }
                /// <summary>
                /// DTC 默认交换机名称，绑定所有Confirm队列
                /// </summary>
                internal static string ConfirmExChange
                {
                    get
                    {
                        return "DTC_Server_Confirm";
                    }
                }
                /// <summary>
                /// 首次队列往这发，比较急。
                /// </summary>
                internal static string DefaultQueue
                {
                    get
                    {
                        return ProjectName + "_DTC_Server_Default";
                    }
                }
                /// <summary>
                /// 定时扫描队列往这发
                /// </summary>
                internal static string RetryQueue
                {
                    get
                    {
                        return ProjectName + "_DTC_Server_Retry";
                    }
                }
                /// <summary>
                /// 确认删除队列往这发。
                /// </summary>
                internal static string ConfirmQueue
                {
                    get
                    {
                        return ProjectName + "_DTC_Server_Confirm";
                    }
                }
            }


            /// <summary>
            /// 工作线程处理模式
            /// </summary>
            public static class Worker
            {
                /// <summary>
                /// 扫描数据库表的间隔时间：单位（秒）
                /// </summary>
                public static int ScanDBSecond
                {
                    get
                    {
                        return AppConfig.GetAppInt("DTC.Server.ScanDBSecond", 10);
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Server.ScanDBSecond", ((int)value).ToString());
                    }
                }
                ///// <summary>
                ///// DTC 本地消息数据表：已确认数据保留时间：（单位秒）
                ///// 配置项：DTC.Client.ConfirmKeepSecond ：60
                ///// </summary>
                //public static int ConfirmKeepSecond
                //{
                //    get
                //    {
                //        return AppConfig.GetAppInt("DTC.Server.ConfirmKeepSecond", 60);
                //    }
                //    set
                //    {
                //        AppConfig.SetApp("DTC.Server.ConfirmKeepSecond", ((int)value).ToString());
                //    }
                //}
                /// <summary>
                /// DTC 本地消息数据表清除模式：0删除、1转移到历史表
                /// 配置项：DTC.Client.ConfirmClearMode ：0
                /// </summary>
                public static TableClearMode ConfirmClearMode
                {
                    get
                    {
                        return (TableClearMode)AppConfig.GetAppInt("DTC.Server.ConfirmClearMode", (int)TableClearMode.MoveToNewTable);
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Server.ConfirmClearMode", ((int)value).ToString());
                    }
                }
                /// <summary>
                /// DTC 本地消息数据表：未确认数据保留时间：（单位秒）
                /// 配置项：DTC.Client.TimeoutKeepSecond ：7 * 24 * 3600
                /// </summary>
                public static int TimeoutKeepSecond
                {
                    get
                    {
                        return AppConfig.GetAppInt("DTC.Server.TimeoutKeepSecond", 3 * 24 * 3600);//7 * 24 * 3600
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Server.TimeoutKeepSecond", ((int)value).ToString());
                    }
                }
                /// <summary>
                /// DTC 本地消息数据表清除模式：0删除、1转移到历史表
                /// 配置项：DTC.Client.NoConfirmClearMode ：1
                /// </summary>
                public static TableClearMode TimeoutClearMode
                {
                    get
                    {
                        return (TableClearMode)AppConfig.GetAppInt("DTC.Server.TimeoutClearMode", (int)TableClearMode.MoveToNewTable);
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Server.TimeoutClearMode", ((int)value).ToString());
                    }
                }
                /// <summary>
                /// 最大重试次数。
                /// </summary>
                public static int MaxRetries
                {
                    get
                    {
                        return AppConfig.GetAppInt("DTC.Server.MaxRetries", 50);
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Server.MaxRetries", ((int)value).ToString());
                    }
                }
            }

        }
    }
}
