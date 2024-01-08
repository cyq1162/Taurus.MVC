using System;
using System.Collections.Generic;
using System.Text;
using CYQ.Data.Tool;

namespace Taurus.Plugin.DistributedTransaction
{

    /// <summary>
    /// DTCServerSubscribe标注的方法传递参数
    /// </summary>
    public class DTCServerSubscribePara
    {
        internal DTCServerSubscribePara(MQMsg msg)
        {
            this.MsgID = msg.MsgID;
            this.ExeType = ConvertTool.ChangeType<ExeType>(msg.ExeType);
            this.Content = msg.Content;
            this.SubKey = msg.CallBackKey;
            this.TraceID = msg.TraceID;
        }
        /// <summary>
        /// 消息唯一ID
        /// </summary>
        public string MsgID { get; set; }
        /// <summary>
        /// 执行类型
        /// </summary>
        public ExeType ExeType { get; set; }
        /// <summary>
        /// 传递的消息内容
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// 如果需要写入内容发往回调处，可以对此赋值。
        /// </summary>
        public string CallBackContent { get; set; }
        /// <summary>
        /// 方法绑定Key
        /// </summary>
        public string SubKey { get; set; }

        /// <summary>
        /// 分布式追踪ID
        /// </summary>
        public string TraceID { get; set; }
    }
}
