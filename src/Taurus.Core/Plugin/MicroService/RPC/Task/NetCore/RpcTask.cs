using System;
using System.Net.Http;
using System.Threading.Tasks;

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
                //if (value == RpcTaskState.Complete)
                //{
                //    if (autoEvent != null)
                //    {
                //        autoEvent.Set();
                //        autoEvent = null;
                //    }
                //}
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
            RpcTaskResult result = new RpcTaskResult();
            try
            {
                if (task.Status != TaskStatus.RanToCompletion && !Wait())
                {
                    State = RpcTaskState.Timeout;
                    result.Error = new Exception("timeout.");
                }
                else
                {
                    HttpResponseMessage message = task.Result;
                    result.IsSuccess = message.IsSuccessStatusCode;
                    HttpContent content = message.Content;
                    result.ResultByte = content.ReadAsByteArrayAsync().Result;
                    foreach (var item in content.Headers)
                    {
                        string value = string.Join(" ", item.Value);
                        //foreach (var v in item.Value)
                        //{
                        //    value = v;
                        //    break;
                        //}
                        result.Header.Add(item.Key, value);
                    }
                    foreach (var item in message.Headers)
                    {
                        string value = string.Join(" ", item.Value);
                        //foreach (var v in item.Value)
                        //{
                        //    value = v;
                        //    break;
                        //}
                        result.Header.Add(item.Key, value);
                    }
                }
            }
            catch (Exception err)
            {
                MsLog.Write(err.Message, Request.Url, Request.Method, "Rpc.RpcTask.SetResult()");
                result.IsSuccess = false;
                result.Error = err;
            }
            _Result = result;
        }
    }
}
