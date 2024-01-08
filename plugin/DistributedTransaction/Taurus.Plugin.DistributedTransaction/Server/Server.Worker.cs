using CYQ.Data;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTC
    {
        /// <summary>
        /// 分布式事务 提供端
        /// </summary>
        public static partial class Server
        {
            /// <summary>
            /// dtc 写数据库、写队列
            /// </summary>
            internal partial class Worker
            {
                public static bool Add(Table table)
                {
                    bool result = false;
                    if (!string.IsNullOrEmpty(DTCConfig.Server.Conn) && table.Insert(InsertOp.None))
                    {
                        result = true;
                        table.Dispose();
                    }
                    if (!result)
                    {
                        result = IO.Write(table);//写 DB => Redis、MemCache，失败则写文本。;
                    }
                    if (result)
                    {
                        DBScanner.Start();//检测未启动则启动，已启动则忽略。
                    }
                    return result;
                }
            }
        }
    }
}
