using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Taurus.Plugin.MicroService
{

    /// <summary>
    /// RPC 任务 【由：Rpc.Rest.相关方法调用返回()】
    /// </summary>
    public partial class RpcTask
    {
        internal RpcTask()
        {

        }
        private AutoResetEvent autoEvent = new AutoResetEvent(false);
        /// <summary>
        /// 已执行标识，用于去重判断。
        /// </summary>
        internal bool IsExecuted = false;
        //internal bool Set()
        //{
        //    return autoEvent.Set();
        //}
        /// <summary>
        /// 等待请求【默认30秒超时】
        /// </summary>
        /// <returns></returns>
        public bool Wait()
        {
            return Wait(30000);
        }
        /// <summary>
        /// 等待请求【指定超时时间】
        /// </summary>
        /// <param name="millisecondsTimeout">毫秒数</param>
        /// <returns></returns>
        public bool Wait(int millisecondsTimeout)
        {
            if (autoEvent != null)
            {
                return autoEvent.WaitOne(millisecondsTimeout);
            }
            return false;
        }
        private RpcTaskState _State = RpcTaskState.None;
        /// <summary>
        /// 当前任务的状态
        /// </summary>
        public RpcTaskState State
        {
            get { return _State; }
            set
            {
                _State = value;
                if (value == RpcTaskState.Complete)
                {
                    if (autoEvent != null)
                    {
                        autoEvent.Set();
                        autoEvent = null;
                    }
                }
            }
        }
        /// <summary>
        /// RPC 任务 请求的参数
        /// </summary>
        public RpcTaskRequest Request { get; set; }

        private RpcTaskResult _Result;
        /// <summary>
        /// RPC 任务 执行后返回的结果
        /// </summary>
        public RpcTaskResult Result
        {
            get
            {
                if (_Result == null)
                {
                    Wait();

                    if (_Result == null)
                    {
                        State = RpcTaskState.Timeout;
                        _Result = new RpcTaskResult() { Error = new Exception("timeout.") };
                    }
                }
                return _Result;
            }
            internal set
            {
                _Result = value;
            }
        }
    }
}
