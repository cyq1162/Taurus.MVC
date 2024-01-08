using System;
using System.Collections.Generic;
using System.Text;
using CYQ.Data.Tool;

namespace Taurus.Plugin.DistributedTransaction
{

    /// <summary>
    /// DTCClientCallBack 标注的方法传递参数
    /// </summary>
    public class DTCClientCallBackPara
    {
        internal DTCClientCallBackPara(MQMsg msg)
        {
            this.MsgID = msg.MsgID;
            this.ExeType = ConvertTool.ChangeType<ExeType>(msg.ExeType);
            this.CallBackContent = msg.Content;
            this.TaskKey = msg.TaskKey;
            this.CallBackKey = msg.CallBackKey;
            this.TraceID = msg.TraceID;
        }
        /// <summary>
        /// 唯一消息ID
        /// </summary>
        public string MsgID { get; set; }
        /// <summary>
        /// 执行类型
        /// </summary>
        public ExeType ExeType { get; set; }

        /// <summary>
        /// 回调回来的消息内容
        /// </summary>
        public string CallBackContent { get; set; }
        /// <summary>
        /// 任务Key
        /// </summary>
        public string TaskKey { get; set; }

        /// <summary>
        /// 回调Key
        /// </summary>
        public string CallBackKey { get; set; }

        /// <summary>
        /// 分布式追踪ID
        /// </summary>
        public string TraceID { get; set; }

    }
}
