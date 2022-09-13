using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.MicroService
{
    /// <summary>
    /// 当前任务的执行状态
    /// </summary>
    public enum RpcTaskState
    {
        /// <summary>
        /// 还没有任何操作
        /// </summary>
        None,
        /// <summary>
        /// .Net 版本【进任务队列：待分配到线程中】
        /// </summary>
        InQueueWaiting,
        /// <summary>
        /// .Net 版本【出任务队列：已被线程获取，待执行】
        /// </summary>
        OutQueueWaiting,
        /// <summary>
        /// 任务执行中
        /// </summary>
        Running,
        /// <summary>
        /// 任务执行完成
        /// </summary>
        Complete,
        /// <summary>
        /// 等待超时
        /// </summary>
        Timeout
    }
}
