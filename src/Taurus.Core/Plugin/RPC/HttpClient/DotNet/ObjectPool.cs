
using CYQ.Data.Json;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;

namespace Taurus.Plugin.MicroService
{
    interface IObjectPool<T> : IDisposable
    {
        /// <summary>
        /// 对象是否存活
        /// </summary>
        bool IsAlive { get; set; }

        /// <summary>
        /// 对象创建时间
        /// </summary>
        DateTime CreateTime { get; set; }

        /// <summary>
        /// 创建新对象
        /// </summary>
        /// <returns></returns>
        T CreateNewObject();

        /// <summary>
        /// 还原对象状态。
        /// </summary>
        void ReSetState();

        /// <summary>
        /// 尝试恢复链接状态
        /// </summary>
        /// <returns></returns>
        bool TryConnection();

    }
    /// <summary>
    /// 通用对象池 
    /// </summary>
    internal partial class ObjectPool<T> where T : class, IObjectPool<T>
    {
        public static ObjectPool<T> Instance = new ObjectPool<T>();

        private T ObjInstance { get; set; }
        public ObjectPool()
        {
            Type type = typeof(T);
            ObjInstance = Activator.CreateInstance(type) as T;
            MinQueue = 10;
            MaxQueue = 64;
            MaxWait = 10;
        }
        public ObjectPool(int min, int max, int wait)
        {
            MinQueue = Math.Max(1, min);
            MaxQueue = Math.Max(10, max);
            MaxWait = Math.Max(10, wait);
        }

        /// <summary>
        /// 对象池队列。
        /// </summary>
        static Queue<T> objectQueue = new Queue<T>(128);
        /// <summary>
        /// 已故障节点
        /// </summary>
        static MDictionary<string, T> deadNode = new MDictionary<string, T>();

        #region 可对外的属性

        public int NewSockets = 0;
        public int CloseSockets = 0;
        public int TimeoutFromSocketPool = 0;
        public int FailedNewSockets = 0;
        public int ReusedSockets = 0;
        public int DeadSocketsInPool = 0;
        public int DeadSocketsOnReturn = 0;
        public int Acquired = 0;


        /// <summary>
        /// 当前池的可用数量。
        /// </summary>
        public int PoolSize { get { return objectQueue.Count; } }

        /// <summary>
        /// 主机节点是不是挂了。
        /// </summary>
        public bool IsEndPointDead = false;
        public DateTime DeadEndPointRetryTime;
        #endregion

        /// <summary>
        /// 备份的Socket池，如果某主机挂了，在配置了备份的情况下，会由备份Socket池提供服务。
        /// </summary>
        // public HostNode HostNodeBak;
        /// <summary>
        /// Socket的挂科时间。
        /// </summary>
        private DateTime socketDeadTime = DateTime.MinValue;

        /// <summary>
        /// 池的最大数量
        /// </summary>
        public int MaxQueue { get; set; }
        /// <summary>
        /// 超出最大链接后的等待时间。
        /// </summary>
        public int MaxWait { get; set; }
        /// <summary>
        /// 最小池数量
        /// </summary>
        public int MinQueue { get; set; }

        /// <summary>
        /// If the host stops responding, we mark it as dead for this amount of seconds, 
        /// and we double this for each consecutive failed retry. If the host comes alive
        /// again, we reset this to 1 again.
        /// </summary>
        private int deadEndPointSecondsUntilRetry = 1;
        private const int maxDeadEndPointSecondsUntilRetry = 60;


        /// <summary>
        /// 从队列获取数据，并可能进行等待。
        /// </summary>
        /// <param name="wait"></param>
        /// <returns></returns>
        private T GetFromQueue(int wait)
        {
            //Do we have free sockets in the pool?
            //if so - return the first working one.
            //if not - create a new one.
            int count = 0;
            while (true)
            {
                count++;
                if (objectQueue.Count > 0)
                {
                    T obj = default(T);
                    lock (objectQueue)
                    {
                        if (objectQueue.Count > 0)
                        {
                            obj = objectQueue.Dequeue();
                        }
                    }
                    if (obj != null)
                    {
                        Interlocked.Increment(ref ReusedSockets);
                        return obj;
                    }
                }
                if (count > wait)
                {
                    return default(T);
                }
                Thread.Sleep(1);
            }
        }

        /// <summary>
        /// 获取一个池对象。
        /// </summary>
        /// <returns></returns>
        public T Acquire()
        {
            T obj = Acquire2();
            if (obj != null)
            {
                obj.ReSetState();
            }
            return obj;
        }
        private T Acquire2()
        {
            if (IsEndPointDead) { return null; }

            Interlocked.Increment(ref Acquired);

            //步骤1：尝试从池里拿，有就拿，没有就走人。
            T obj = GetFromQueue(0);
            if (obj != null)
            {
                return obj;
            }

            //步骤2：池已经没有了：如果运行中的数量<最大池，直接New
            var runCount = NewSockets - CloseSockets;
            if (runCount <= MaxQueue)
            {
                obj = ObjInstance.CreateNewObject();
                Interlocked.Increment(ref NewSockets);
                return obj;
            }
            //步骤3：池已经没有了：如果运行中的数量>最大池，等待一下下。
            else if (runCount <= MaxQueue)
            {
                obj = GetFromQueue(MaxWait);
                if (obj != null)
                {
                    return obj;
                }
            }

            //步骤4：池还是不够用，等待1秒了还是没可用链接。
            obj = ObjInstance.CreateNewObject();
            Interlocked.Increment(ref NewSockets);
            return obj;
        }

        internal void Return(T obj)
        {
            //If the socket is dead, destroy it.
            if (!obj.IsAlive || hasDisponse)
            {
                Interlocked.Increment(ref CloseSockets);
                obj.Dispose();
            }
            else
            {
                var isAllowClose = objectQueue.Count > MinQueue && obj.CreateTime.AddMinutes(10) < DateTime.Now;
                isAllowClose = isAllowClose || objectQueue.Count > MaxQueue && obj.CreateTime.AddMinutes(1) < DateTime.Now;
                if (isAllowClose)
                {
                    obj.Dispose();
                    Interlocked.Increment(ref CloseSockets);
                }
                else
                {
                    lock (objectQueue)
                    {
                        objectQueue.Enqueue(obj);
                    }
                }
            }
        }

        /// <summary>
        /// 线程池里重试链接。
        /// </summary>
        /// <returns></returns>
        internal bool TryConnection()
        {
            if (DateTime.Now > DeadEndPointRetryTime)
            {
                T socket = ObjInstance.CreateNewObject();
                if (socket.IsAlive)
                {
                    IsEndPointDead = false;
                    deadEndPointSecondsUntilRetry = 1; //Reset retry timer on success.
                    Return(socket);//不浪费，丢到池里重用。
                    return true;
                }
                else
                {
                    //Retry in 1 minutes
                    DeadEndPointRetryTime = DateTime.Now.AddSeconds(deadEndPointSecondsUntilRetry);
                    if (deadEndPointSecondsUntilRetry < maxDeadEndPointSecondsUntilRetry)
                    {
                        deadEndPointSecondsUntilRetry += 1; //Double retry interval until next time
                    }
                }
            }
            return false;
        }


        static bool isTaskDoing = false;
        /// <summary>
        /// 添加故障节点
        /// </summary>
        public void AddToDeadPool()
        {
            if (!IsEndPointDead)
            {
                IsEndPointDead = true;
                socketDeadTime = DateTime.Now;
                //if (!deadNode.ContainsKey(Host))
                //{
                //    deadNode.Add(Host, this);
                //}
                if (!isTaskDoing)
                {
                    lock (deadNode)
                    {
                        isTaskDoing = true;
                        ThreadBreak.AddGlobalThread(new ParameterizedThreadStart(DoNodeRetryTask));
                    }
                }
            }
        }

        #region IDisposable 成员
        bool hasDisponse = false;
        public void Dispose()
        {
            hasDisponse = true;
            while (objectQueue.Count > 0)
            {
                Return(objectQueue.Dequeue());
            }
        }

        #endregion

        /// <summary>
        /// 线程检测已故障节点。
        /// </summary>
        /// <param name="threadID"></param>
        static void DoNodeRetryTask(object threadID)
        {
            while (true)
            {
                Thread.Sleep(1000);
                if (deadNode.Count > 0)
                {
                    List<string> keys = deadNode.GetKeys();
                    foreach (string key in keys)
                    {
                        if (deadNode[key].TryConnection())
                        {
                            deadNode.Remove(key);
                        }
                    }
                }
            }
        }


    }

    internal partial class ObjectPool<T>
    {
        public string WorkInfo
        {
            get
            {
                JsonHelper result = new JsonHelper(false, false);
                result.Add("Acquired sockets", this.Acquired.ToString());
                result.Add("Acquired timeout from socket pool", TimeoutFromSocketPool.ToString());
                result.Add("New sockets created", NewSockets.ToString());
                result.Add("New sockets failed", FailedNewSockets.ToString());
                result.Add("Sockets in pool", PoolSize.ToString());
                result.Add("Sockets reused", ReusedSockets.ToString());
                result.Add("Sockets died in pool", DeadSocketsInPool.ToString());
                result.Add("Sockets died on return", DeadSocketsOnReturn.ToString());
                result.Add("Sockets close", CloseSockets.ToString());
                return result.ToString();
            }
        }
    }
}
