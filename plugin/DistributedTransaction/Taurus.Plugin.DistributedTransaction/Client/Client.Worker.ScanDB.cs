using CYQ.Data;
using CYQ.Data.Table;
using CYQ.Data.Tool;
using CYQ.Data.Lock;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTC
    {
        public static partial class Client
        {
            internal static partial class Worker
            {
                /// <summary>
                /// 1、扫描数据库
                /// 2、发送到MQ
                /// 3、程序运行时启动、服务调用时也检测启动。  
                /// </summary>
                internal class DBScanner
                {
                    static DBScanner()
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(MQPublisher.InitQueueListen), null);
                    }
                    static bool threadIsWorking = false;
                    const string lockKey = "DTC.Client.Lock:Worker.ScanDB";
                    static object lockObj = new object();
                    public static void Start()
                    {
                        if (MQ.Client.MQType == MQType.Empty || threadIsWorking)
                        {
                            empty = 1;//保持任务不退出。
                            return;
                        }
                        lock (lockObj)
                        {
                            if (!threadIsWorking)
                            {
                                threadIsWorking = true;
                                empty = 0;
                                ThreadPool.QueueUserWorkItem(new WaitCallback(DoWork), null);
                            }
                        }



                    }
                    static int empty = 0;
                    static DateTime scanTime = DateTime.Now;
                    private static void DoWork(object p)
                    {
                        while (true)
                        {
                            try
                            {
                                int scanInterval = DTCConfig.Client.Worker.ScanDBSecond;

                                if (empty <= 0 || empty == scanInterval * 10 || DateTime.Now >= scanTime)//进入和退出前都执行1次
                                {
                                    bool isLockOK = false;
                                    try
                                    {
                                        if (string.IsNullOrEmpty(DTCConfig.Client.Conn))
                                        {
                                            isLockOK = DistributedLock.Instance.Lock(lockKey, 1);
                                            if (isLockOK)
                                            {
                                                ScanDB();//数据库仅允许一个在扫描
                                            }
                                        }
                                    }
                                    finally
                                    {
                                        if (isLockOK)
                                        {
                                            DistributedLock.Instance.UnLock(lockKey);
                                        }
                                    }

                                    if (empty > scanInterval)
                                    {
                                        ScanIO();//硬盘每个进程都需要扫描，但延时处理。
                                    }
                                    scanTime = DateTime.Now.AddSeconds(scanInterval * (empty < 1 ? 1 : 2));//按扫描次数增加时间
                                }

                                Thread.Sleep(1000);
                                empty++;
                                if (empty > scanInterval * 10)  //扫描10次都没东西可以扫
                                {
                                    threadIsWorking = false;
                                    break;//结束线程。
                                }
                            }
                            catch (Exception err)
                            {
                                Log.Write(err, "DTC.Client");
                                break;
                            }

                        }
                    }

                    private static void ScanDB()
                    {
                        if (!DBTool.Exists(DTCConfig.Client.TableName, "U", DTCConfig.Client.Conn))
                        {
                            return;
                        }
                        int maxRetries = DTCConfig.Client.Worker.MaxRetries;
                        int scanInterval = Math.Max(60, DTCConfig.Client.Worker.ScanDBSecond);//最短1分钟
                        int noConfirmSecond = DTCConfig.Client.Worker.TimeoutKeepSecond;
                        using (MAction action = new MAction(DTCConfig.Client.TableName, DTCConfig.Client.Conn))
                        {
                            action.IsUseAutoCache = false;

                            #region 扫描数据库、发送到MQ队列
                            string where = "ConfirmState = 0 and Retries<" + maxRetries + " and EditTime<'" + DateTime.Now.AddSeconds(-scanInterval).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            MDataTable dtSend = action.Select(1000, where);
                            while (dtSend != null && dtSend.Rows.Count > 0)
                            {
                                empty = -1;

                                bool isUpdateOK = false;
                                dtSend.Columns.SetValue("ExChangeName", DTCConfig.Server.MQ.RetryExChange);
                                dtSend.Columns.SetValue("CallBackQueueName", DTCConfig.Client.MQ.RetryQueue);
                                List<MQMsg> msgList = dtSend.ToList<MQMsg>();
                                if (MQ.Client.PublishBatch(msgList))
                                {
                                    Debug.WriteLine("Client.ScanDB 已从数据库扫描批量发送到 RetryExChange 队列：" + msgList.Count);
                                    foreach (var row in dtSend.Rows)
                                    {
                                        row.Set("Retries", row.Get<int>("Retries") + 1, 2);
                                        row.Set("EditTime", DateTime.Now, 2);
                                    }
                                    isUpdateOK = dtSend.AcceptChanges(AcceptOp.Update, DTCConfig.Client.Conn, "ID");
                                }

                                if (isUpdateOK)
                                {
                                    dtSend = action.Select(1000, where);
                                }
                                else
                                {
                                    break;
                                }
                                Thread.Sleep(1);
                            }

                            #endregion

                            #region 清空数据、或转移到历史表
                            string whereConfirm = "ConfirmState=1";
                            if (DTCConfig.Client.Worker.ConfirmClearMode == TableClearMode.Delete)
                            {
                                action.Delete(whereConfirm);//不讲道理直接清
                            }
                            else
                            {
                                #region 已确认的数据：清空数据、或转移到历史表
                                MDataTable dt = action.Select(10000, whereConfirm + " order by id asc");
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    dt.TableName = DTCConfig.Client.TableName + "_History";
                                    if (dt.AcceptChanges(AcceptOp.Auto | AcceptOp.InsertWithID, DTCConfig.Client.Conn))//仅插入
                                    {
                                        dt.TableName = DTCConfig.Client.TableName;
                                        dt.AcceptChanges(AcceptOp.Delete, DTCConfig.Client.Conn, "ID");
                                    }
                                }
                                #endregion
                            }
                            string whereTimeout = "ConfirmState=0 and CreateTime<'" + DateTime.Now.AddSeconds(-noConfirmSecond).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            if (DTCConfig.Client.Worker.TimeoutClearMode == TableClearMode.Delete)
                            {
                                action.Delete(whereTimeout);
                            }
                            else
                            {
                                #region 已超时的数据：删除或转移到超时表

                                MDataTable dt = action.Select(10000, whereTimeout + " order by id asc");
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    dt.TableName = DTCConfig.Client.TableName + "_History";
                                    if (dt.AcceptChanges(AcceptOp.Auto | AcceptOp.InsertWithID, DTCConfig.Client.Conn))
                                    {
                                        dt.TableName = DTCConfig.Client.TableName;
                                        dt.AcceptChanges(AcceptOp.Delete, DTCConfig.Client.Conn, "ID");
                                    }
                                }


                                #endregion
                            }

                            #endregion
                        }
                    }
                    private static void ScanIO()
                    {
                        int maxRetries = DTCConfig.Client.Worker.MaxRetries;
                        int scanInterval = Math.Max(60, DTCConfig.Client.Worker.ScanDBSecond);//最短1分钟
                        List<Table> tables = Worker.IO.GetScanTable();
                        if (tables != null && tables.Count > 0)
                        {
                            List<MQMsg> msgList = new List<MQMsg>();
                            //消息重发
                            foreach (var table in tables)
                            {
                                if (!table.Retries.HasValue) { table.Retries = 0; }
                                if (table.Retries >= maxRetries)
                                {
                                    IO.Delete(table.TraceID, table.MsgID, table.ExeType);
                                    continue;
                                }
                                if (table.EditTime.HasValue && table.EditTime.Value > DateTime.Now.AddSeconds(-scanInterval))
                                {
                                    continue;//在一个扫描间隔时间内的不触发重试
                                }
                                table.ExChange = DTCConfig.Server.MQ.RetryExChange;
                                table.CallBackName = DTCConfig.Client.MQ.RetryQueue;
                                table.Retries += 1;
                                table.EditTime = DateTime.Now;
                                IO.Write(table);
                                msgList.Add(table.ToMQMsg());

                            }

                            //批量发送
                            if (msgList.Count > 0 && MQ.Client.PublishBatch(msgList))
                            {
                                Debug.WriteLine("Client.ScanIO 批量发送到 RetryExChange 队列：" + msgList.Count);
                            }
                        }
                    }
                }
            }
        }
    }
}
