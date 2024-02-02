
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
        /// �����Ƿ���
        /// </summary>
        bool IsAlive { get; set; }

        /// <summary>
        /// ���󴴽�ʱ��
        /// </summary>
        DateTime CreateTime { get; set; }

        /// <summary>
        /// �����¶���
        /// </summary>
        /// <returns></returns>
        T CreateNewObject();

        /// <summary>
        /// ��ԭ����״̬��
        /// </summary>
        void ReSetState();

        /// <summary>
        /// ���Իָ�����״̬
        /// </summary>
        /// <returns></returns>
        bool TryConnection();

    }
    /// <summary>
    /// ͨ�ö���� 
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
        /// ����ض��С�
        /// </summary>
        static Queue<T> objectQueue = new Queue<T>(128);
        /// <summary>
        /// �ѹ��Ͻڵ�
        /// </summary>
        static MDictionary<string, T> deadNode = new MDictionary<string, T>();

        #region �ɶ��������

        public int NewSockets = 0;
        public int CloseSockets = 0;
        public int TimeoutFromSocketPool = 0;
        public int FailedNewSockets = 0;
        public int ReusedSockets = 0;
        public int DeadSocketsInPool = 0;
        public int DeadSocketsOnReturn = 0;
        public int Acquired = 0;


        /// <summary>
        /// ��ǰ�صĿ���������
        /// </summary>
        public int PoolSize { get { return objectQueue.Count; } }

        /// <summary>
        /// �����ڵ��ǲ��ǹ��ˡ�
        /// </summary>
        public bool IsEndPointDead = false;
        public DateTime DeadEndPointRetryTime;
        #endregion

        /// <summary>
        /// ���ݵ�Socket�أ����ĳ�������ˣ��������˱��ݵ�����£����ɱ���Socket���ṩ����
        /// </summary>
        // public HostNode HostNodeBak;
        /// <summary>
        /// Socket�Ĺҿ�ʱ�䡣
        /// </summary>
        private DateTime socketDeadTime = DateTime.MinValue;

        /// <summary>
        /// �ص��������
        /// </summary>
        public int MaxQueue { get; set; }
        /// <summary>
        /// ����������Ӻ�ĵȴ�ʱ�䡣
        /// </summary>
        public int MaxWait { get; set; }
        /// <summary>
        /// ��С������
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
        /// �Ӷ��л�ȡ���ݣ������ܽ��еȴ���
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
        /// ��ȡһ���ض���
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

            //����1�����Դӳ����ã��о��ã�û�о����ˡ�
            T obj = GetFromQueue(0);
            if (obj != null)
            {
                return obj;
            }

            //����2�����Ѿ�û���ˣ���������е�����<���أ�ֱ��New
            var runCount = NewSockets - CloseSockets;
            if (runCount <= MaxQueue)
            {
                obj = ObjInstance.CreateNewObject();
                Interlocked.Increment(ref NewSockets);
                return obj;
            }
            //����3�����Ѿ�û���ˣ���������е�����>���أ��ȴ�һ���¡�
            else if (runCount <= MaxQueue)
            {
                obj = GetFromQueue(MaxWait);
                if (obj != null)
                {
                    return obj;
                }
            }

            //����4���ػ��ǲ����ã��ȴ�1���˻���û�������ӡ�
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
        /// �̳߳����������ӡ�
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
                    Return(socket);//���˷ѣ������������á�
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
        /// ��ӹ��Ͻڵ�
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

        #region IDisposable ��Ա
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
        /// �̼߳���ѹ��Ͻڵ㡣
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
