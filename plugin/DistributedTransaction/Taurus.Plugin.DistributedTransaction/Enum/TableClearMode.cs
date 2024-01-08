using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Plugin.DistributedTransaction
{
    public enum TableClearMode
    {
        /// <summary>
        /// 删除数据
        /// </summary>
        Delete = 0,
        /// <summary>
        /// 转移到历史表
        /// </summary>
        MoveToNewTable = 1
    }
}
