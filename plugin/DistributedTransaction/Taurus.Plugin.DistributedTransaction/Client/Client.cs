using CYQ.Data;
using CYQ.Data.Json;
using CYQ.Data.Lock;
using CYQ.Data.Tool;
using System;
using System.Reflection;
using System.Web;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTC
    {
        /// <summary>
        /// 分布式事务 调用端
        /// </summary>
        public static partial class Client
        {

            /*
             * 客户端逻辑说明
             * 1、发送消息：DTC.Client.SendAsync(....)
             * 2、异步表任务：生成表、创建数据    =》注意：失败重试、再失败直接写MQ、再失败记录文本数据
             * 3、异步MQ任务：读表、写MQ、发送数据。
             * 4、异步MQ任务：等待接收数据，更新表状态数据。
             */

            #region 发起 PublishTask

            /// <summary>
            /// 提交事务确认
            /// </summary>
            /// <param name="num">需要确认的数量</param>
            public static bool CommitAsync(int num)
            {
                return ExeAsync(ExeType.Commit, null, null, null, num);
            }

            /// <summary>
            /// 提交事务确认
            /// </summary>
            /// <param name="num">需要确认的数量</param>
            /// <param name="callBackKey">如果需要接收回调通知，指定本回调key，回调方法用 DTCClientCallBack 特性标注</param>
            public static bool CommitAsync(int num, string callBackKey)
            {
                return ExeAsync(ExeType.Commit, null, null, callBackKey, num);
            }

            /// <summary>
            /// 提交事务确认
            /// </summary>
            /// <param name="num">需要确认的数量</param>
            /// <param name="callBackKey">如果需要接收回调通知，指定本回调key，回调方法用 DTCClientCallBack 特性标注</param>
            /// <param name="content">传递的信息</param>
            public static bool CommitAsync(int num, string callBackKey, string content)
            {
                return ExeAsync(ExeType.Commit, content, null, callBackKey, num);
            }
            /// <summary>
            /// 提交事务回滚
            /// </summary>
            /// <param name="num">需要回滚的数量</param>
            public static bool RollBackAsync(int num)
            {
                return ExeAsync(ExeType.RollBack, null, null, null, num);
            }
            /// <summary>
            /// 提交事务回滚
            /// </summary>
            /// <param name="num">需要回滚的数量</param>
            /// <param name="callBackKey">如果需要接收回调通知，指定本回调key，回调方法用 DTCClientCallBack 特性标注</param>
            public static bool RollBackAsync(int num, string callBackKey)
            {
                return ExeAsync(ExeType.RollBack, null, null, callBackKey, num);
            }

            /// <summary>
            /// 提交事务回滚
            /// </summary>
            /// <param name="num">需要回滚的数量</param>
            /// <param name="callBackKey">如果需要接收回调通知，指定本回调key，回调方法用 DTCClientCallBack 特性标注</param>
            /// <param name="content">传递的信息</param>
            public static bool RollBackAsync(int num, string callBackKey, string content)
            {
                return ExeAsync(ExeType.RollBack, content, null, callBackKey, num);
            }
            /// <summary>
            /// 发起一个任务消息
            /// </summary>
            /// <param name="content">传递的信息</param>
            /// <param name="taskKey">指定任务key，即Server方监听的subKey</param>
            public static bool PublishTaskAsync(string content, string taskKey)
            {
                return ExeAsync(ExeType.Task, content, taskKey, null, 1);
            }
            /// <summary>
            /// 发起一个任务消息
            /// </summary>
            /// <param name="content">传递的信息</param>
            /// <param name="taskKey">指定任务key，即Server方监听的subKey</param>
            /// <param name="callBackKey">如果需要接收回调通知，指定本回调key，回调方法用 DTCClientCallBack 特性标注</param>
            public static bool PublishTaskAsync(string content, string taskKey, string callBackKey)
            {
                return ExeAsync(ExeType.Task, content, taskKey, callBackKey, 1);
            }

            private static bool ExeAsync(ExeType exeType, string content, string taskKey, string callBackKey, int confirmNum)
            {
                Table table = new Table();
                table.ExeType = exeType.ToString();
                table.Content = content;
                table.TaskKey = taskKey;
                table.CallBackKey = callBackKey;
                table.ConfirmNum = confirmNum;
                table.CreateTime = DateTime.Now;
                table.EditTime = DateTime.Now;
                table.Retries = 0;
                if (System.Web.HttpContext.Current == null)
                {
                    if (exeType == ExeType.Commit || exeType == ExeType.RollBack)
                    {
                        string msg = "HttpContext.Current is null.";
                        throw new Exception(msg);
                    }
                }
                else
                {
                    table.TraceID = HttpContext.Current.GetTraceID();
                }
                table.ExChange = DTCConfig.Server.MQ.DefaultExChange;
                table.CallBackName = DTCConfig.Client.MQ.DefaultQueue;
                return Worker.Add(table);
            }

            #endregion

            #region 接收 Subscribe

            /// <summary>
            /// 消息有回调，说明对方任务已完成
            /// </summary>
            internal static void OnReceived(MQMsg msg)
            {
                var localLock = DistributedLock.Local;
                string key = "DTC.Client." + msg.MsgID;
                bool isLockOK = false;
                try
                {
                    isLockOK = localLock.Lock(key, 1000);
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
                    Log.Write(err, "DTC.Client");
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
                if (!(msg.IsFirstAck.HasValue && msg.IsFirstAck.Value) || (msg.IsDeleteAck.HasValue && msg.IsDeleteAck.Value))
                {
                    DoTaskConfirm(msg);
                    return;
                }

                if (!string.IsNullOrEmpty(msg.CallBackKey))
                {
                    #region 执行方法

                    MethodInfo method = MethodCollector.GetClientMethod(msg.CallBackKey);
                    if (method != null)
                    {
                        try
                        {
                            DTCClientCallBackPara para = new DTCClientCallBackPara(msg);
                            object obj = method.IsStatic ? null : Activator.CreateInstance(method.DeclaringType);
                            object invokeResult = method.Invoke(obj, new object[] { para });
                            if (invokeResult is bool && !(bool)invokeResult) { return; }
                            //DTCLog.WriteDebugLine("Client.OnDoTask 首次执行方法：" + method.Name);
                        }
                        catch (Exception err)
                        {
                            Log.Write(err.Message, "DTC.Client");
                            return;
                        }
                    }
                    #endregion
                }
                DoTaskConfirm(msg);
            }

            private static void DoTaskConfirm(MQMsg msg)
            {
                bool isUpdateOK = false;
                using (Table table = new Table())
                {
                    table.ConfirmState = 1;
                    table.EditTime = DateTime.Now;
                    isUpdateOK = table.Update(msg.MsgID);
                    if (isUpdateOK)
                    {
                        //DTCLog.WriteDebugLine("Client.OnDoTask 首次更新数据表。");
                    }
                    else if (Worker.IO.Delete(msg.TraceID, msg.MsgID, msg.ExeType))
                    {
                        isUpdateOK = true;
                        //DTCLog.WriteDebugLine("Client.OnDoTask 首次删除缓存：" + msg.MsgID);
                    }
                }

                //这边已经删除数据，告诉对方，也可以删除数据了。
                if (isUpdateOK || (msg.IsDeleteAck.HasValue && msg.IsDeleteAck.Value))
                {
                    msg.SetDeleteAsk();
                    Worker.MQPublisher.Add(msg);
                    //DTCLog.WriteDebugLine("Client.OnDoTask IsDeleteAck=true，让服务端确认及删除掉缓存。");
                }
            }


            private static void OnCommitOrRollBack(MQMsg msg)
            {
                if (!(msg.IsFirstAck.HasValue && msg.IsFirstAck.Value) || (msg.IsDeleteAck.HasValue && msg.IsDeleteAck.Value))
                {
                    CommitOrRollBackConfirm(msg);
                    return;
                }

                if (!string.IsNullOrEmpty(msg.CallBackKey))
                {
                    MethodInfo method = MethodCollector.GetClientMethod(msg.CallBackKey);
                    if (method != null)
                    {
                        try
                        {
                            DTCClientCallBackPara para = new DTCClientCallBackPara(msg);
                            object obj = method.IsStatic ? null : Activator.CreateInstance(method.DeclaringType);
                            object invokeResult = method.Invoke(obj, new object[] { para });
                            if (invokeResult is bool && !(bool)invokeResult) { return; }
                            DTCLog.WriteDebugLine("Server.OnCommitOrRollBack 执行方法：。" + method.Name);
                        }
                        catch (Exception err)
                        {
                            Log.Write(err.Message, "DTC.Client");
                            return;
                        }
                    }
                }
                CommitOrRollBackConfirm(msg);
            }

            private static void CommitOrRollBackConfirm(MQMsg msg)
            {
                bool isUpdateOK = false;
                using (Table table = new Table())
                {
                    #region 事务确认和回滚
                    //等待这边确认执行类型
                    bool isConfirm = false;
                    if (table.Fill("TraceID='" + msg.TraceID + "' and ExeType='" + msg.ExeType + "'"))
                    {
                        if (string.IsNullOrEmpty(table.Content))
                        {
                            table.Content = msg.MsgID;
                            if (!table.ConfirmNum.HasValue || table.ConfirmNum.Value <= 1)
                            {
                                table.ConfirmState = 1;
                                isConfirm = true;
                            }
                            isUpdateOK = table.Update();
                            if (isUpdateOK)
                            {
                                DTCLog.WriteDebugLine("Client.OnCommitOrRollBack 更新状态。");
                            }
                        }
                        else if (!table.Content.Contains(msg.MsgID))
                        {
                            table.Content += "," + msg.MsgID;
                            if (!table.ConfirmNum.HasValue || table.ConfirmNum.Value <= table.Content.Split(',').Length)
                            {
                                table.ConfirmState = 1;
                                isConfirm = true;
                            }
                            isUpdateOK = table.Update();
                            if (isUpdateOK)
                            {
                                //DTCLog.WriteDebugLine("Client.OnCommitOrRollBack 更新状态。");
                            }
                        }
                    }
                    if (!isConfirm)
                    {
                        string json = Worker.IO.Read(msg.TraceID, msg.MsgID, msg.ExeType);
                        if (!string.IsNullOrEmpty(json))
                        {
                            var tb = JsonHelper.ToEntity<Table>(json);
                            if (tb != null)
                            {
                                if (string.IsNullOrEmpty(tb.Content))
                                {
                                    tb.Content = msg.MsgID;
                                    if (!tb.ConfirmNum.HasValue || tb.ConfirmNum.Value <= 1)
                                    {
                                        isConfirm = true;
                                    }
                                }
                                else if (!table.Content.Contains(msg.MsgID))
                                {
                                    table.Content += "," + msg.MsgID;
                                    if (!tb.ConfirmNum.HasValue || table.ConfirmNum.Value <= table.Content.Split(',').Length)
                                    {
                                        isConfirm = true;
                                    }
                                }
                                if (!isConfirm)
                                {
                                    //重新写回缓存里
                                    isUpdateOK = Worker.IO.Write(tb);
                                }
                            }
                        }
                    }
                    #endregion
                    if (isConfirm)
                    {
                        if (Worker.IO.Delete(msg.TraceID, msg.TraceID, msg.ExeType))
                        {
                            //DTCLog.WriteDebugLine("Client.OnCommitOrRollBack 删除缓存。");
                        }
                    }

                    //这边已经删除数据，告诉对方，也可以删除数据了。
                    if (isUpdateOK || isConfirm || (msg.IsDeleteAck.HasValue && msg.IsDeleteAck.Value))
                    {
                        msg.SetDeleteAsk();
                        Worker.MQPublisher.Add(msg);
                        //DTCLog.WriteDebugLine("Client.OnDoTask.IsDeleteAck，让服务端确认及删除掉缓存。");
                    }
                }
            }

            #endregion

        }
    }
}
