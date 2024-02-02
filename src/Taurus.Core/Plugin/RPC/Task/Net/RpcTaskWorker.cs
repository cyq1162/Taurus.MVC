using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using Taurus.Mvc;
using System.Web;
using System.Threading;
using CYQ.Data.Tool;
using System.IO;
namespace Taurus.Plugin.Rpc
{

    /// <summary>
    /// .Net 版本
    /// </summary>
    internal partial class RpcTaskWorker
    {
        public static RpcTask ExecuteTaskAsync(RpcTaskRequest request)
        {
            RpcTask task = new RpcTask();
            task.Request = request;
            if (HttpContext.Current != null && task.Request.Headers["X-Request-ID"] == null)
            {
                //分布式请求追踪ID。
                task.Request.Headers.Add("X-Request-ID", HttpContext.Current.GetTraceID());
            }
           
            MutilQueue.Add(task);
            return task;
        }
        /// <summary>
        /// 通用方法调用（同步）
        /// </summary>
        private static void ExeTask(object taskObj)
        {
            RpcTask task = taskObj as RpcTask;
            if (task.IsExecuted)//任务已被执行过，无需要重复执行。
            {
                return;
            }
            task.IsExecuted = true;
            task.State = RpcTaskState.Running;
            task.Result = Rpc.Rest.StartTask(task.Request);
            //if (result.IsSuccess)
            //{
            //    //去掉多余的属性
            //    if (result.Headers["Transfer-Encoding"] != null)
            //    {
            //        //输出这个会造成时不时的503
            //        result.Headers.Remove("Transfer-Encoding");
            //    }
            //    if (result.Headers["Content-Type"] != null && result.Headers["Content-Type"].Split(';').Length == 1)
            //    {
            //        //输出这个会影响啥，忘了。
            //        result.Headers.Remove("Content-Type");
            //    }
            //}
        }
        #region 注销掉旧方法
        /*
      
        /// <summary>
        /// 通用方法调用（同步）
        /// </summary>
        private static void ExeTask2(object taskObj)
        {
            RpcTask task = taskObj as RpcTask;
            if (task.IsExecuted)//任务已被执行过，无需要重复执行。
            {
                return;
            }
            task.IsExecuted = true;
            task.State = RpcTaskState.Running;
            RpcTaskResult result = Rpc.Rest.StartTask(task.Request);

            RpcTaskResult rpcResult = new RpcTaskResult();

            //var pool = RpcClientPool.Create(task.Request.Uri);

            RpcClient wc = new RpcClient();
            try
            {
                //wc = pool.Acquire();
                //if (wc == null) { return; }
                if (task.Request.Timeout > 0)
                {
                    wc.Timeout = task.Request.Timeout;
                }
                wc.Headers.Add(MsConst.HeaderKey, (MsConfig.IsClient ? MsConfig.Client.RcKey : MsConfig.Server.RcKey));

                //wc.Headers.Add("X-Real-IP", MvcConst.HostIP);
                //if (HttpContext.Current != null && HttpContext.Current.Request != null)
                //{
                //    wc.Headers.Add("Referer", HttpContext.Current.Request.Url.AbsoluteUri);//当前运行地址。
                //}
                //else if (!string.IsNullOrEmpty(MvcConfig.RunUrl))
                //{
                //    wc.Headers.Add("Referer", MvcConfig.RunUrl);//当前运行地址。
                //}
                if (task.Request.Headers != null && task.Request.Headers.Count > 0)
                {
                    foreach (var item in task.Request.Headers)
                    {
                        wc.Headers.Add(item.Key, item.Value);
                    }
                }
                switch (task.Request.Method.ToUpper())
                {
                    case "GET":
                        SetResult(wc, rpcResult, wc.DownloadData(task.Request.Url));
                        break;
                    case "HEAD":
                        wc.Head(task.Request.Url);
                        SetResult(wc, rpcResult, null);
                        break;
                    case "POST":
                    case "PUT":
                    case "DELETE":
                    case "PATCH":
                        if (task.Request.Data == null)
                        {
                            task.Request.Data = new byte[0];
                        }
                        SetResult(wc, rpcResult, wc.UploadData(task.Request.Uri, task.Request.Method, task.Request.Data));
                        break;
                }

            }
            catch (Exception err)
            {
                rpcResult.IsSuccess = false;
                rpcResult.Error = err;
                MsLog.Write(err.Message, task.Request.Url, task.Request.Method);
            }
            finally
            {
                task.State = RpcTaskState.Complete;
                task.Result = rpcResult;
                //if (wc != null)
                //{
                //    pool.Return(wc);
                //}
            }

        }
        private static void SetResult(RpcClient wc, RpcTaskResult result, byte[] resultData)
        {
            result.IsSuccess = true;
            result.ResultByte = resultData;
            try
            {
                foreach (string key in wc.ResponseHeaders.Keys)
                {
                    switch (key)
                    {
                        case "Transfer-Encoding"://输出这个会造成时不时的503
                            continue;
                    }
                    if (key == "Content-Type" && wc.ResponseHeaders[key].Split(';').Length == 1)
                    {
                        continue;
                    }
                    result.Headers.Add(key, wc.ResponseHeaders[key]);
                }
            }
            catch (Exception err)
            {
                CYQ.Data.Log.WriteLogToTxt(err);
            }
        }
        */
        #endregion

    }
    internal partial class RpcTaskWorker
    {
        /// <summary>
        /// 暂时看起来没有MutilQueue好。
        /// </summary>
        private class OneQueue
        {
            static int threadTotalCount = 6;
            static int[] threadExeCountArray = null;
            static int exeTaskTotalCount = 0;

            /// <summary>
            /// 待处理的工作队列
            /// </summary>
            static Queue<RpcTask> _TaskQueue = new Queue<RpcTask>(1024);
            static AutoResetEvent auto = new AutoResetEvent(false);

            static bool threadIsWorking = false;
            public static void Add(RpcTask task)
            {
                lock (_TaskQueue)
                {
                    _TaskQueue.Enqueue(task);
                }
                task.State = RpcTaskState.InQueueWaiting;
                if (!threadIsWorking)
                {
                    threadIsWorking = true;
                    threadExeCountArray = new int[threadTotalCount];
                    for (int i = 0; i < threadTotalCount; i++)
                    {
                        ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(DoWork), i);
                    }
                }
                else
                {
                    auto.Set();
                }


            }
            private static readonly object lockObj = new object();
            private static void DoWork(object num)
            {
                int i = int.Parse(num.ToString());
                if (i > 0)
                {
                    auto.WaitOne();
                }

                while (true)
                {
                    if (_TaskQueue.Count > 0)
                    {
                        RpcTask task = null;
                        lock (_TaskQueue)
                        {
                            if (_TaskQueue.Count > 0)
                            {
                                task = _TaskQueue.Dequeue();
                                if (task == null)
                                {
                                    System.Diagnostics.Debug.WriteLine("队列任务消失了：" + i.ToString());
                                }
                                else
                                {
                                    task.State = RpcTaskState.OutQueueWaiting;
                                }
                            }
                        }
                        if (task != null)
                        {
                            ExeTask(task);
                            threadExeCountArray[i]++;
                            exeTaskTotalCount++;
                            //System.Diagnostics.Debug.WriteLine("Thread_" + i.ToString() + " : " + threadExeCountArray[i] + "/" + exeTaskTotalCount);
                        }

                        //try
                        //{
                        //    //以try 替换 lock
                        //    RpcTask task = _TaskQueue.Dequeue();
                        //    if (task != null)
                        //    {
                        //        task.State = RpcTaskState.OutQueueWaiting;
                        //        System.Diagnostics.Debug.WriteLine("thread...." + i.ToString());
                        //        ExeTask(task);
                        //    }
                        //}
                        //catch (Exception err)
                        //{
                        //    System.Diagnostics.Debug.WriteLine("thread...." + i.ToString() + err.Message);
                        //}


                    }
                    else
                    {
                        // System.Diagnostics.Debug.WriteLine("Thread_" + i.ToString() + " : " + threadExeCountArray[i] + "/" + exeTaskTotalCount);
                        auto.WaitOne();
                    }
                }


            }
        }
    }

    internal partial class RpcTaskWorker
    {
        /// <summary>
        /// 多队列机制（每个线程一个队列）
        /// </summary>
        private class MutilQueue
        {
            static AutoResetEvent _Event = new AutoResetEvent(false);
            static MutilQueue()
            {
                if (!threadIsWorking)
                {
                    threadExeCountArray = new int[threadTotalCount];
                    idleThreadArray = new bool[threadTotalCount];
                    autoArray = new AutoResetEvent[threadTotalCount];
                    taskQueueArray = new Queue<RpcTask>[threadTotalCount];
                    for (int i = 0; i < threadTotalCount; i++)
                    {
                        idleThreadArray[i] = true;
                        autoArray[i] = new AutoResetEvent(false);
                        taskQueueArray[i] = new Queue<RpcTask>(64);
                        ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(DoWork), i);
                    }
                    threadIsWorking = true;
                    _Event.Set();
                }
            }
            //static Dictionary<Guid, bool> taskIDDic = new Dictionary<Guid, bool>();
            /// <summary>
            /// 开启的线程数
            /// </summary>
            static int threadTotalCount = 1;
            static bool[] idleThreadArray = null;
            static AutoResetEvent[] autoArray = null;
            static Queue<RpcTask>[] taskQueueArray = null;
            static int[] threadExeCountArray = null;
            static bool threadIsWorking = false;
            static int exeTaskTotalCount = 0;
            public static void Add(RpcTask task)
            {
                if (!threadIsWorking)
                {
                    _Event.WaitOne();
                    _Event = null;
                }
                if (task.State == RpcTaskState.None)
                {
                    int exeThreadID = GetMinTaskThread();
                    idleThreadArray[exeThreadID] = false;
                    Queue<RpcTask> taskQueue = taskQueueArray[exeThreadID];
                    lock (taskQueue)
                    {
                        taskQueue.Enqueue(task);
                    }
                    autoArray[exeThreadID].Set();
                    task.State = RpcTaskState.InQueueWaiting;
                }
            }
            private static int index = -1;
            /// <summary>
            /// 平均安排
            /// </summary>
            /// <returns></returns>
            private static int GetMinTaskThread2()
            {
                index++;
                if (index >= threadTotalCount)
                {
                    index = 0;
                }
                return index;
            }
            /// <summary>
            /// 按任务量最少的安排(遇到任务阻塞时性能好）
            /// </summary>
            /// <returns></returns>
            private static int GetMinTaskThread()
            {
                int lastCount = 0;
                int index = 0;
                for (int i = 0; i < threadTotalCount; i++)
                {
                    int count = taskQueueArray[i].Count;
                    if (count == 0)
                    {
                        return i;
                    }
                    if (i == 0) { lastCount = count; continue; }
                    else
                    {
                        if (count < lastCount)
                        {
                            lastCount = count;
                            index = i;
                        }
                    }
                }
                return index;
            }

            private static void DoWork(object num)
            {
                int i = int.Parse(num.ToString());
                AutoResetEvent auto = autoArray[i];
                Queue<RpcTask> taskQueue = taskQueueArray[i];

                while (true)
                {
                    if (taskQueue.Count > 0)
                    {
                        RpcTask task = taskQueue.Dequeue();
                        if (task != null)
                        {
                            task.State = RpcTaskState.OutQueueWaiting;
                            ExeTask(task);
                            threadExeCountArray[i]++;
                            exeTaskTotalCount++;
                        }
                        //else
                        //{
                        //    //存放的时候，不能有并发，不然有这个问题（在Eequeue处加了锁）。
                        //    System.Diagnostics.Debug.WriteLine("队列任务消失了：" + i.ToString());
                        //}
                        //idleThreadArray[i] = true;
                    }
                    else
                    {
                        //System.Diagnostics.Debug.WriteLine("Thread_" + i.ToString() + " : " + threadExeCountArray[i] + "/" + exeTaskTotalCount);
                        auto.WaitOne();
                    }
                }
            }
        }
    }
}
