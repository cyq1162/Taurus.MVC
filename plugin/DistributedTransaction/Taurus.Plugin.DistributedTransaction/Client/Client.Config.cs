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
        /// 客户端【调用端】配置项
        /// </summary>
        public static class Client
        {
            /// <summary>
            /// 配置是否启用 客户端【调用端】
            /// 如 DTC.Client.IsEnable ：true， 默认值：true
            /// </summary>
            public static bool IsEnable
            {
                get
                {
                    return AppConfig.GetAppBool("DTC.Client.IsEnable", true);
                }
                set
                {
                    AppConfig.SetApp("DTC.Client.IsEnable", value.ToString());
                }
            }
          
            /// <summary>
            /// DTC 类记录本地消息数据库 - 数据库链接配置
            /// 配置项：DTCClientConn：server=.;database=x;uid=s;pwd=p;
            /// </summary>
            public static string Conn
            {
                get
                {
                    return AppConfig.GetConn("DTCClientConn");
                }
                set
                {
                    AppConfig.SetConn("DTCClientConn", value);
                }
            }
            /// <summary>
            /// DTC 类记录本地消息数据库 - 表名
            /// 配置项：DTC.Client.TableName ：DTC_Client
            /// </summary>
            public static string TableName
            {
                get
                {
                    return AppConfig.GetApp("DTC.Client.TableName", "DTC_Client");
                }
                set
                {
                    AppConfig.SetApp("DTC.Client.TableName", value);
                }
            }

            /// <summary>
            /// RabbitMQ 链接配置
            /// 配置项：DTC.Client.Rabbit=127.0.0.1;guest;guest;/
            /// </summary>
            public static string Rabbit
            {
                get
                {
                    return AppConfig.GetApp("DTC.Client.Rabbit");
                }
                set
                {
                    AppConfig.SetApp("DTC.Client.Rabbit", value);
                }
            }
            /// <summary>
            /// Kafka 链接配置
            /// 配置项：DTC.Client.Kafka=127.0.0.1:9092
            /// </summary>
            public static string Kafka
            {
                get
                {
                    return AppConfig.GetApp("DTC.Client.Kafka");
                }
                set
                {
                    AppConfig.SetApp("DTC.Client.Kafka", value);
                }
            }

            /// <summary>
            /// RabbitMQ相关配置项
            /// </summary>
            internal static class MQ
            {
                /// <summary>
                /// DTC 默认交换机名称，绑定所Default队列
                /// </summary>
                internal static string DefaultExChange
                {
                    get
                    {
                        return "DTC_Client_Default";
                    }
                }
                /// <summary>
                /// DTC 默认交换机名称，绑定所有Retry队列
                /// </summary>
                internal static string RetryExChange
                {
                    get
                    {
                        return "DTC_Client_Retry";
                    }
                }
                /// <summary>
                /// DTC 默认交换机名称，绑定所有Confirm队列
                /// </summary>
                internal static string ConfirmExChange
                {
                    get
                    {
                        return "DTC_Client_Confirm";
                    }
                }
                /// <summary>
                /// 首次队列往这发，比较急。
                /// </summary>
                internal static string DefaultQueue
                {
                    get
                    {
                        return ProjectName + "_DTC_Client_Default";
                    }
                }
                /// <summary>
                /// 定时扫描队列往这发
                /// </summary>
                internal static string RetryQueue
                {
                    get
                    {
                        return ProjectName + "_DTC_Client_Retry";
                    }
                }
                /// <summary>
                /// 确认删除队列往这发。
                /// </summary>
                internal static string ConfirmQueue
                {
                    get
                    {
                        return ProjectName + "_DTC_Client_Confirm";
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
                        return AppConfig.GetAppInt("DTC.Client.ScanDBSecond", 10);
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Client.ScanDBSecond", ((int)value).ToString());
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
                //        return AppConfig.GetAppInt("DTC.Client.ConfirmKeepSecond", 60);
                //    }
                //    set
                //    {
                //        AppConfig.SetApp("DTC.Client.ConfirmKeepSecond", ((int)value).ToString());
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
                        return (TableClearMode)AppConfig.GetAppInt("DTC.Client.ConfirmClearMode", (int)TableClearMode.MoveToNewTable);
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Client.ConfirmClearMode", ((int)value).ToString());
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
                        return AppConfig.GetAppInt("DTC.Client.TimeoutKeepSecond", 3 * 24 * 3600);//7 * 24 * 3600
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Client.TimeoutKeepSecond", ((int)value).ToString());
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
                        return (TableClearMode)AppConfig.GetAppInt("DTC.Client.TimeoutClearMode", (int)TableClearMode.MoveToNewTable);
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Client.TimeoutClearMode", ((int)value).ToString());
                    }
                }
                /// <summary>
                /// 最大重试次数。
                /// </summary>
                public static int MaxRetries
                {
                    get
                    {
                        return AppConfig.GetAppInt("DTC.Client.MaxRetries", 50);
                    }
                    set
                    {
                        AppConfig.SetApp("DTC.Client.MaxRetries", ((int)value).ToString());
                    }
                }
            }
        }
    }
}
