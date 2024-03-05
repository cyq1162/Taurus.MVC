using CYQ.Data;
using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace Taurus.Plugin.Rpc
{
    /// <summary>
    /// RPC 任务 【由：Rpc.Rest.相关方法调用返回。】
    /// </summary>
    public partial class RpcTask
    {
        internal Task<HttpResponseMessage> task;
        /// <summary>
        /// 等待请求【默认30秒超时】
        /// </summary>
        /// <returns></returns>
        public bool Wait()
        {
            return Wait(30000);
        }
        private bool WaitIsThrowException = false;
        /// <summary>
        /// 等待请求【指定超时时间】
        /// </summary>
        /// <param name="millisecondsTimeout">毫秒数</param>
        /// <returns></returns>
        public bool Wait(int millisecondsTimeout)
        {
            if (task != null && !WaitIsThrowException)
            {
                try
                {
                    return task.Wait(millisecondsTimeout);
                }
                catch (Exception err)
                {
                    WaitIsThrowException = true;
                    State = RpcTaskState.Complete;
                    Result = new RpcTaskResult() { Error = err };
                    return false;
                }

            }
            return false;
        }
        private RpcTaskState _State = RpcTaskState.None;
        /// <summary>
        /// 当前任务的状态
        /// </summary>
        public RpcTaskState State
        {
            get
            {
                if (task != null && task.Status == TaskStatus.RanToCompletion)
                {
                    return RpcTaskState.Complete;
                }
                return _State;
            }
            set
            {
                _State = value;
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
                if (_Result == null && task != null)
                {
                    SetResult();
                }
                return _Result;
            }
            internal set
            {
                _Result = value;
            }
        }
        private void SetResult()
        {

            try
            {
                if (task.Status != TaskStatus.RanToCompletion && !Wait())
                {
                    RpcTaskResult result = new RpcTaskResult();
                    if (task.Status == TaskStatus.Faulted)
                    {
                        State = RpcTaskState.Complete;
                        result.Error = task.Exception;
                    }
                    else
                    {
                        State = RpcTaskState.Timeout;
                        result.Error = new Exception("timeout.");
                    }
                    _Result = result;
                }
                else
                {
                    _Result = RpcTaskWorker.GetRpcTaskResult(task).GetAwaiter().GetResult();
                }
            }
            catch (Exception err)
            {
                Log.Write(err);
                _Result = new RpcTaskResult();
                _Result.IsSuccess = false;
                _Result.Error = err;
            }
        }

        private RpcTaskAwaiter _awaiter = null;

        public RpcTaskAwaiter GetAwaiter()
        {
            _awaiter = new RpcTaskAwaiter(Result);
            return _awaiter;
        }
    }

    public partial class RpcTask : IDisposable
    {
        /// <summary>
        /// 关闭请求资源
        /// </summary>
        public void Dispose()
        {
            if (Request != null)
            {
                HttpClient client = HttpClientFactory.Get(Request.Uri, Request.Timeout);
                if (client != null)
                {
                    client.Dispose();
                    HttpClientFactory.Remove(Request.Uri, Request.Timeout);
                }
            }
        }
    }


    public class RpcTaskAwaiter : INotifyCompletion
    {
        public RpcTaskResult Result { get; set; }
        public RpcTaskAwaiter(RpcTaskResult result)
        {
            this.Result = result;
        }
        public bool IsCompleted { get; private set; }

        public void OnCompleted(Action continuation)
        {
            IsCompleted = true;
            if (continuation != null)
            {
                ThreadPool.QueueUserWorkItem(_ =>
                {
                    continuation?.Invoke();
                });
            }

        }

        public RpcTaskResult GetResult()
        {
            return this.Result;
        }
    }
}
