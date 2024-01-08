using CYQ.Data;
using CYQ.Data.Lock;
using CYQ.Data.Table;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTC
    {
        public static partial class Server
        {
            internal static partial class Worker
            {
                /// <summary>
                /// 1、扫描数据库
                /// 2、发送到MQ
                /// 3、程序运行时启动、服务调用时也检测启动。  
                /// </summary>
                internal static class DBScanner
                {
                    static DBScanner()
                    {
                        ThreadPool.QueueUserWorkItem(new WaitCallback(MQPublisher.InitQueueListen), null);
                    }

                    static bool threadIsWorking = false;
                    const string lockKey = "DTC.Server.Lock:Worker.ScanDB";
                    static readonly object lockObj = new object();
                    public static void Start()
                    {
                        //1 再加个分布式锁，保障只有一个应用在启动表描述
                        //1、数据库参数检测
                        //2、MQ参数检测
                        if (MQ.Server.MQType == MQType.Empty || threadIsWorking)
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

                                if (empty <= 0 || empty == scanInterval * 10 || DateTime.Now >= scanTime)
                                {
                                    bool isLockOK = false;
                                    try
                                    {
                                        if (string.IsNullOrEmpty(DTCConfig.Server.Conn))
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
                                    ScanIO_DeleteEmptyDirectory();
                                    scanTime = DateTime.Now.AddSeconds(scanInterval);//按扫描次数增加时间
                                }
                                Thread.Sleep(1000);
                                empty++;
                                if (empty > scanInterval * 10)  //扫描10次都没东西可以扫
                                {
                                    ScanIO_DeleteTimeout();
                                    threadIsWorking = false;
                                    break;//结束线程。
                                }
                            }
                            catch (Exception err)
                            {
                                threadIsWorking = false;
                                Log.Write(err, "DTC.Server");
                                break;
                            }
                        }
                    }




                    private static void ScanDB()
                    {
                        if (string.IsNullOrEmpty(DTCConfig.Server.Conn) || !DBTool.Exists(DTCConfig.Server.TableName, "U", DTCConfig.Server.Conn))
                        {
                            return;
                        }
                        int maxRetries = DTCConfig.Server.Worker.MaxRetries;
                        int scanInterval = Math.Max(60, DTCConfig.Server.Worker.ScanDBSecond);//最短1分钟

                        using (MAction action = new MAction(DTCConfig.Server.TableName, DTCConfig.Server.Conn))
                        {
                            action.IsUseAutoCache = false;

                            #region 扫描数据库、发送到MQ队列
                            string whereConfirm = "ConfirmState=1 and Retries<" + maxRetries + " and EditTime<'" + DateTime.Now.AddSeconds(-scanInterval).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            MDataTable dtSend = action.Select(1000, whereConfirm);
                            while (dtSend != null && dtSend.Rows.Count > 0)
                            {
                                empty = -1;

                                bool isUpdateOK = false;

                                List<MQMsg> msgList = dtSend.ToList<MQMsg>();
                                foreach (MQMsg msg in msgList)
                                {
                                    msg.SetDeleteAsk();
                                }
                                if (MQ.Client.PublishBatch(msgList))
                                {
                                    Debug.WriteLine("Server.ScanDB 已从数据库扫描批量发送到MQ队列：" + msgList.Count);
                                    foreach (var row in dtSend.Rows)
                                    {
                                        row.Set("Retries", row.Get<int>("Retries") + 1, 2);
                                        row.Set("EditTime", DateTime.Now, 2);
                                    }
                                    isUpdateOK = dtSend.AcceptChanges(AcceptOp.Update, DTCConfig.Server.Conn, "ID");
                                }

                                if (isUpdateOK)
                                {
                                    dtSend = action.Select(1000, whereConfirm);
                                }
                                else
                                {
                                    break;
                                }
                                Thread.Sleep(1);
                            }


                            #endregion

                            #region 清空数据、或转移到历史表

                            string whereDelete = "ConfirmState=2";

                            if (DTCConfig.Server.Worker.ConfirmClearMode == TableClearMode.Delete)
                            {
                                action.Delete(whereDelete);//不讲道理直接清
                            }
                            else
                            {
                                #region 已确认的数据：清空数据、或转移到历史表
                                MDataTable dt = action.Select(1000, whereDelete + " order by id asc");
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    dt.TableName = DTCConfig.Server.TableName + "_History";
                                    if (dt.AcceptChanges(AcceptOp.Auto | AcceptOp.InsertWithID, DTCConfig.Server.Conn))//仅插入
                                    {
                                        dt.TableName = DTCConfig.Server.TableName;
                                        dt.AcceptChanges(AcceptOp.Delete, DTCConfig.Server.Conn, "ID");
                                    }
                                }
                                #endregion
                            }
                            int noConfirmSecond = DTCConfig.Server.Worker.TimeoutKeepSecond;
                            string whereTimeout = "ConfirmState<2 and CreateTime<'" + DateTime.Now.AddSeconds(-noConfirmSecond).ToString("yyyy-MM-dd HH:mm:ss") + "'";
                            if (DTCConfig.Server.Worker.TimeoutClearMode == TableClearMode.Delete)
                            {
                                action.Delete(whereTimeout);
                            }
                            else
                            {
                                #region 已超时的数据：删除或转移到超时表

                                MDataTable dt = action.Select(1000, whereTimeout + " order by id asc");
                                if (dt != null && dt.Rows.Count > 0)
                                {
                                    dt.TableName = DTCConfig.Server.TableName + "_History";
                                    if (dt.AcceptChanges(AcceptOp.Auto | AcceptOp.InsertWithID, DTCConfig.Server.Conn))
                                    {
                                        dt.TableName = DTCConfig.Server.TableName;
                                        dt.AcceptChanges(AcceptOp.Delete, DTCConfig.Server.Conn, "ID");
                                    }
                                }


                                #endregion
                            }

                            #endregion
                        }
                    }

                    private static void ScanIO_DeleteTimeout()
                    {
                        IO.DeleteTimeoutTable();
                    }
                    private static void ScanIO_DeleteEmptyDirectory()
                    {
                        IO.DeleteEmptyDirectory();
                    }
                }
            }
        }
    }
}
