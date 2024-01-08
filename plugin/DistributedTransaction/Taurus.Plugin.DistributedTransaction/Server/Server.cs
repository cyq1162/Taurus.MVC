using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reflection;
using CYQ.Data;
using CYQ.Data.Lock;
using System.Web;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTC
    {
        /// <summary>
        /// 分布式事务 提供端
        /// </summary>
        public static partial class Server
        {
            #region Subscribe

            /// <summary>
            /// 保存信息，以便订阅回调函数处处理。
            /// </summary>
            /// <param name="content">需要传递到订阅回调处理的内容</param>
            /// <param name="subKey">指定订阅key</param>
            /// <returns></returns>
            public static bool Subscribe(string content, string subKey)
            {
                Table table = new Table();
                table.CallBackKey = subKey;
                table.Content = content;
                table.Retries = 0;
                table.CreateTime = DateTime.Now;
                table.EditTime = DateTime.Now;
                if (System.Web.HttpContext.Current == null)
                {
                    string msg = "HttpContext.Current is null.";
                    throw new Exception(msg);
                }
                table.TraceID = HttpContext.Current.GetTraceID();
                return Worker.Add(table);
            }
            #endregion



            internal static void OnReceived(MQMsg msg)
            {
                //Debug.WriteLine("当前线程ID：" + System.Threading.Thread.CurrentThread.ManagedThreadId);
                var localLock = DistributedLock.Local;
                string key = "DTC.Server." + msg.MsgID;
                bool isLockOK = false;
                try
                {
                    isLockOK = localLock.Lock(key, 10000);
                    if (msg.ExeType == ExeType.Task.ToString())
                    {
                        OnDoTask(msg);
                    }
                    else if (msg.ExeType == ExeType.Commit.ToString() || msg.ExeType == ExeType.RollBack.ToString())
                    {
                        OnCommitOrRollBack(msg);
                    }
                }
                catch (Exception err)
                {
                    Log.Write(err, "DTC.Server");
                }
                finally
                {
                    if (isLockOK)
                    {
                        localLock.UnLock(key);
                    }
                }
            }
          
            private static void OnDoTask(MQMsg msg)
            {
                if (msg.IsDeleteAck.HasValue && msg.IsDeleteAck.Value)
                {
                    //可以删除数据
                    using (Table table = new Table())
                    {
                        table.ConfirmState = 2;
                        table.EditTime = DateTime.Now;
                        if (table.Update(msg.MsgID))
                        {
                            DTCDebug.WriteLine("Server.OnDoTask.IsDeleteAck：更新表：" + msg.MsgID);
                        }
                    }
                    if (Worker.IO.Delete(msg.TraceID, msg.MsgID, msg.ExeType))
                    {
                        DTCDebug.WriteLine("Server.OnDoTask.IsDeleteAck：更新表：" + msg.MsgID);
                    }
                    return;
                }

                msg.CallBackName = DTCConfig.Server.MQ.ConfirmQueue;

                #region 检测是否已执行过。
                using (Table table = new Table())
                {
                    if (table.Exists(msg.MsgID) || Worker.IO.Exists(msg.TraceID, msg.MsgID, msg.ExeType))
                    {
                        msg.IsFirstAck = false;
                        Worker.MQPublisher.Add(msg);
                        DTCDebug.WriteLine("Server.OnDoTask 方法已执行过，发送MQ响应：IsFirstAck = false。");
                        return;
                    }
                }
                #endregion

                MethodInfo method = MethodCollector.GetServerMethod(msg.CallBackKey);
                if (method == null) { return; }//没有对应的绑定信息，直接丢失信息。
                string returnContent = null;
                try
                {
                    DTCServerSubscribePara para = new DTCServerSubscribePara(msg);
                    object obj = method.IsStatic ? null : Activator.CreateInstance(method.DeclaringType);
                    object result = method.Invoke(obj, new object[] { para });
                    if (result is bool && !(bool)result) { return; }
                    returnContent = para.ReturnContent;
                    DTCDebug.WriteLine("Server.OnDoTask 执行方法：。" + method.Name);
                }
                catch (Exception err)
                {
                    Log.Write(err.Message, "DTC.Server");
                    return;
                }

                msg.IsFirstAck = true;
                msg.Content = returnContent;
                Worker.MQPublisher.Add(msg);
                DTCDebug.WriteLine("Server.OnDoTask 首次回应：IsFirstAck = true ，并执行方法：" + method.Name);

                using (Table table = new Table())
                {
                    //开启新任务，上面已经反转，直接赋值即可。
                    table.ExeType = msg.ExeType;
                    table.QueueName = msg.QueueName;
                    table.CallBackName = msg.CallBackName;
                    table.TaskKey = msg.TaskKey;
                    table.CallBackKey = msg.CallBackKey;
                    table.TraceID = msg.TraceID;
                    table.MsgID = msg.MsgID;
                    table.Content = msg.Content;
                    table.ConfirmState = 1;//如果发送失败，则不设置确认，延时被删除。
                    if (table.Insert(InsertOp.ID))
                    {
                        DTCDebug.WriteLine("Server.OnDoTask 首次回应：插入数据表。");
                    }
                    else if (Worker.IO.Write(table))//缓存1份。
                    {
                        DTCDebug.WriteLine("Server.OnDoTask 首次回应：写入缓存。");
                    }
                }

            }

            #region 事务提交或回滚

            private static void OnCommitOrRollBack(MQMsg msg)
            {
                if (msg.IsDeleteAck.HasValue && msg.IsDeleteAck.Value)
                {
                    //可以删除数据
                    using (Table table = new Table())
                    {
                        table.ConfirmState = 2;
                        table.EditTime = DateTime.Now;
                        if (table.Update(msg.MsgID))
                        {
                            DTCDebug.WriteLine("Server.OnCommitOrRollBack 收到MQ Ack：IsFirstAck=true ，更新表。");
                        }
                    }
                    if (Worker.IO.Delete(msg.TraceID, msg.MsgID, msg.ExeType))
                    {
                        DTCDebug.WriteLine("Server.OnCommitOrRollBack 收到MQ Ack：IsFirstAck=true ，删除缓存。");
                    }
                    return;
                }

                List<Table> tables = GetTableList(msg);
                if (tables == null || tables.Count == 0) { return; }
                msg.CallBackName = DTCConfig.Server.MQ.ConfirmQueue;

                foreach (Table item in tables)
                {
                    msg.MsgID = item.MsgID;
                    if (item.ConfirmState.HasValue && item.ConfirmState.Value > 0)
                    {
                        msg.IsFirstAck = false;
                        Worker.MQPublisher.Add(msg);
                        DTCDebug.WriteLine("Server.OnCommitOrRollBack 方法已执行过，直接回应MQ。");
                        continue;
                    }
                    string returnContent = null;
                    try
                    {
                        MethodInfo method = MethodCollector.GetServerMethod(item.CallBackKey);
                        if (method == null) { continue; }
                        DTCServerSubscribePara para = new DTCServerSubscribePara(msg);
                        object obj = method.IsStatic ? null : Activator.CreateInstance(method.DeclaringType);
                        object result = method.Invoke(obj, new object[] { para });
                        if (result is bool && !(bool)result) { continue; }
                        returnContent = para.ReturnContent;
                        DTCDebug.WriteLine("Server.OnCommitOrRollBack 执行方法：。" + method.Name);
                    }
                    catch (Exception err)
                    {
                        Log.Write(err.Message, "DTC.Server");
                        return;
                    }
                    msg.IsFirstAck = true;
                    msg.Content = returnContent;
                    Worker.MQPublisher.Add(msg);

                    item.TaskKey = msg.TaskKey;
                    item.QueueName = msg.QueueName;
                    item.ExeType = msg.ExeType;
                    item.ConfirmState = 1;
                    item.EditTime = DateTime.Now;
                    if (item.Update(item.MsgID))
                    {
                        item.Dispose();
                        DTCDebug.WriteLine("Server.OnCommitOrRollBack 更新数据表。");
                    }
                    else if (Worker.IO.Write(item))//缓存1份。
                    {
                        DTCDebug.WriteLine("Server.OnCommitOrRollBack 更新数据缓存。");
                    }


                }
            }
            private static List<Table> GetTableList(MQMsg msg)
            {
                List<Table> tableList = null;
                using (Table table = new Table())
                {
                    tableList = table.Select<Table>("TraceID='" + msg.TraceID + "'");
                }
                if (tableList != null && tableList.Count > 0)
                {
                    return tableList;
                }
                return Worker.IO.GetListByTraceID(msg.TraceID, msg.ExeType);
            }
            #endregion
        }
    }
}
