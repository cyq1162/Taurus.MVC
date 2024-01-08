using System;
using CYQ.Data.Json;

namespace Taurus.Plugin.DistributedTransaction
{

    /// <summary>
    /// MQ传递消息实体
    /// </summary>
    internal class MQMsg
    {
        public string MsgID { get; set; }
        public string ExeType { get; set; }
        public string Content { get; set; }
        /// <summary>
        /// 任务Key
        /// </summary>
        public string TaskKey { get; set; }

        /// <summary>
        /// 方法绑定Key
        /// </summary>
        public string CallBackKey { get; set; }

        /// <summary>
        /// 用于发送的交换机名称【对应 kafka 的 topic】
        /// </summary>
        public string ExChange { get; set; }
        /// <summary>
        /// 用于发送的队列名称【对应 kafka 的 groupID】
        /// </summary>
        public string QueueName { get; set; }

        /// <summary>
        /// 用于回调的队列名称
        /// </summary>
        public string CallBackName { get; set; }
        /// <summary>
        /// 分布式追踪ID
        /// </summary>
        public string TraceID { get; set; }
        /// <summary>
        /// 是否首次响应Ack
        /// </summary>
        public bool? IsFirstAck { get; set; }

        public bool? IsDeleteAck { get; set; }


        /// <summary>
        /// 设置成响应删除状态，同时减少不需要传递用的数据。
        /// </summary>
        internal void SetDeleteAsk()
        {
            IsDeleteAck = true;
            Content = null;
            TaskKey = null;
            CallBackKey = null;
            ExChange = null;
            IsFirstAck = null;
        }

        /// <summary>
        /// 直接实现，避免反射
        /// </summary>
        /// <returns></returns>
        public string ToJson()
        {
            JsonHelper js = new JsonHelper(false, false);
            js.Add("MsgID", this.MsgID);
            js.Add("ExeType", this.ExeType);
            if (this.Content != null)
            {
                js.Add("Content", this.Content);
            }
            if (this.TaskKey != null)
            {
                js.Add("TaskKey", this.TaskKey);
            }
            if (this.CallBackKey != null)
            {
                js.Add("CallBackKey", this.CallBackKey);
            }
            if (this.ExChange != null)
            {
                js.Add("ExChange", this.ExChange);
            }
            js.Add("QueueName", this.QueueName);
            if (this.CallBackName != null)
            {
                js.Add("CallBackName", this.CallBackName);
            }
            js.Add("TraceID", this.TraceID);
            if (this.IsFirstAck.HasValue)
            {
                js.Add("IsFirstAck", this.IsFirstAck.Value ? "true" : "false", true);
            }
            if (this.IsDeleteAck.HasValue)
            {
                js.Add("IsDeleteAck", this.IsDeleteAck.Value ? "true" : "false", true);
            }
            return js.ToString();
        }
        public static MQMsg Create(string json)
        {
            MQMsg msg = new MQMsg();
            if (!string.IsNullOrEmpty(json))
            {
                var dic = JsonHelper.Split(json);
                if (dic.ContainsKey("MsgID")) { msg.MsgID = dic["MsgID"]; }
                if (dic.ContainsKey("ExeType")) { msg.ExeType = dic["ExeType"]; }
                if (dic.ContainsKey("Content")) { msg.Content = dic["Content"]; }
                if (dic.ContainsKey("TaskKey")) { msg.TaskKey = dic["TaskKey"]; }
                if (dic.ContainsKey("CallBackKey")) { msg.CallBackKey = dic["CallBackKey"]; }
                if (dic.ContainsKey("ExChange")) { msg.ExChange = dic["ExChange"]; }
                if (dic.ContainsKey("QueueName")) { msg.QueueName = dic["QueueName"]; }
                if (dic.ContainsKey("CallBackName")) { msg.CallBackName = dic["CallBackName"]; }
                if (dic.ContainsKey("TraceID")) { msg.TraceID = dic["TraceID"]; }
                if (dic.ContainsKey("IsFirstAck")) { msg.IsFirstAck = Convert.ToBoolean(dic["IsFirstAck"]); }
                if (dic.ContainsKey("IsDeleteAck")) { msg.IsDeleteAck = Convert.ToBoolean(dic["IsDeleteAck"]); }
            }
            return msg;
        }
    }
}
