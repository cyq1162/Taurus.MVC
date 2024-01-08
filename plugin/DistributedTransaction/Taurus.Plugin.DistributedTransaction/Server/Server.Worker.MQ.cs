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
        public static partial class Server
        {
            internal static partial class Worker
            {
                internal static partial class MQPublisher
                {
                    /// <summary>
                    /// 待处理的工作队列
                    /// </summary>
                    static ConcurrentQueue<MQMsg> _dtcQueue = new ConcurrentQueue<MQMsg>();
                    static object lockObj = new object();
                    static bool threadIsWorking = false;
                    public static void Add(MQMsg msg)
                    {
                        _dtcQueue.Enqueue(msg);
                        if (threadIsWorking) { return; }
                        lock (lockObj)
                        {
                            if (!threadIsWorking)
                            {
                                threadIsWorking = true;
                                ThreadPool.QueueUserWorkItem(new WaitCallback(DoWork), null);
                            }
                        }
                    }

                    private static void DoWork(object p)
                    {
                        try
                        {
                            int empty = 0;
                            while (true)
                            {
                                while (!_dtcQueue.IsEmpty)
                                {
                                    empty = 0;
                                    List<MQMsg> mQMsgs = new List<MQMsg>();
                                    while (!_dtcQueue.IsEmpty && mQMsgs.Count < 500)
                                    {
                                        MQMsg msg;
                                        if (_dtcQueue.TryDequeue(out msg))
                                        {
                                            mQMsgs.Add(msg);
                                        }
                                    }
                                    if (mQMsgs.Count > 0)
                                    {
                                        if (MQ.Server.PublishBatch(mQMsgs))
                                        {
                                            Debug.WriteLine("Server 批量发布MQ信息：" + mQMsgs.Count + "条。");
                                        }
                                        mQMsgs.Clear();
                                    }

                                    Thread.Sleep(1);
                                }
                                empty++;
                                Thread.Sleep(1000);
                                if (empty > 100)
                                {
                                    //超过10分钟没日志产生
                                    threadIsWorking = false;
                                    break;//结束线程。
                                }
                            }
                        }
                        catch (Exception err)
                        {
                            threadIsWorking = false;
                            //数据库异常，不处理。
                            Log.Write(err, "DTC-Client");
                        }
                    }

                    public static void InitQueueListen(object p)
                    {
                        var mq = MQ.Server;
                        if (mq.MQType != MQType.Empty)
                        {
                            //对默认对列绑定交换机。
                            MQ.Server.Listen(DTCConfig.Server.MQ.DefaultQueue, Server.OnReceived, DTCConfig.Server.MQ.DefaultExChange);
                            MQ.Server.Listen(DTCConfig.Server.MQ.RetryQueue, Server.OnReceived, DTCConfig.Server.MQ.RetryExChange);
                            MQ.Server.Listen(DTCConfig.Server.MQ.ConfirmQueue, Server.OnReceived, DTCConfig.Server.MQ.ConfirmExChange);
                        }
                    }
                }
            }

        }
    }
}
