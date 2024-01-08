using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Plugin.DistributedTransaction
{
    public enum ExeType
    {
        /// <summary>
        /// 事务执行回滚
        /// </summary>
        RollBack = -1,
        /// <summary>
        /// 发布任务
        /// </summary>
        Task = 0,
        /// <summary>
        /// 事务执行提交
        /// </summary>
        Commit = 1
    }

}
