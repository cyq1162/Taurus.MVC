using CYQ.Data;
using CYQ.Data.Table;
using System;
using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading;
using System.Collections;
using System.Collections.Generic;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTC
    {
        public static partial class Client
        {
            /// <summary>
            /// dtc 写数据库、写队列
            /// </summary>
            internal static partial class Worker
            {
                public static bool Add(Table table)
                {
                    bool result = false;
                    if (!string.IsNullOrEmpty(DTCConfig.Client.Conn) && table.Insert(InsertOp.None))
                    {
                        result = true;
                        table.Dispose();
                    }
                    if (!result)
                    {
                        result = IO.Write(table);//写 DB => Redis、MemCache，失败则写文本。
                    }
                    if (result)
                    {
                        MQPublisher.Add(table.ToMQMsg());//异步发送MQ
                        DBScanner.Start();//检测未启动则启动，已启动则忽略。
                    }
                    return result;
                }
            }

        }
    }
}
