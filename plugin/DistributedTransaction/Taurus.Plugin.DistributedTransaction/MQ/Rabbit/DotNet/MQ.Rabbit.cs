using CYQ.Data;
using CYQ.Data.Tool;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading;

namespace Taurus.Plugin.DistributedTransaction
{

    internal class MQRabbit : MQ
    {
        #region 错误链接断开重连机制处理
        public class ListenPara
        {
            public OnReceivedDelegate onReceivedDelegate { get; set; }
            public string BindExName { get; set; }
        }
        MDictionary<string, ListenPara> listenFailDic = new MDictionary<string, ListenPara>(StringComparer.OrdinalIgnoreCase);
        private bool _IsConnectOK = false;
        public bool IsConnectOK
        {
            get
            {
                if (!_IsConnectOK)
                {
                    TryConnect();
                }
                else if (_Connection != null && !_Connection.IsOpen)
                {
                    _IsConnectOK = false;
                    TryConnect();
                }
                return _IsConnectOK;
            }
            set { _IsConnectOK = value; }
        }
        private object lockObj = new object();
        private bool isThreadWorking = false;
        private void TryConnect()
        {
            if (isThreadWorking) { count = 0; return; }
            lock (lockObj)
            {
                if (isThreadWorking) { return; }
                isThreadWorking = true;
                count = 0;
                ThreadPool.QueueUserWorkItem(new WaitCallback(ConnectAgain), null);
            }
        }
        int count = 0;
        private void ConnectAgain(object p)
        {
            if (_IsConnectOK) { return; }
            while (true)
            {
                Thread.Sleep(3000);
                try
                {
                    _Connection = factory.CreateConnection();
                    _IsConnectOK = _Connection.IsOpen;
                    if (_IsConnectOK)
                    {
                        //重新开启监听
                        if (listenFailDic.Count > 0)
                        {
                            List<string> keys = listenFailDic.GetKeys();
                            foreach (string key in keys)
                            {
                                ListenPara para = listenFailDic[key];
                                if (Listen(key, para.onReceivedDelegate, para.BindExName))
                                {
                                    listenFailDic.Remove(key);
                                }
                            }
                        }
                        isThreadWorking = false;
                        break;
                    }
                }
                catch
                {

                }
                count++;
                if (count > 10)
                {
                    isThreadWorking = false;
                    break;
                }
            }

        }
        #endregion
        public override MQType MQType
        {
            get
            {
                return MQType.Rabbit;
            }
        }

        ConnectionFactory factory;

        public MQRabbit(string mqConn)
        {
            InitFactory(mqConn);
        }
        private void InitFactory(string mqConn)
        {
            try
            {
                string[] items = mqConn.Split(new char[] { ';', ',' });
                if (items.Length >= 4)
                {
                    factory = new ConnectionFactory()
                    {
                        HostName = items[0],
                        UserName = items[1],
                        Password = items[2],
                        VirtualHost = items[3]
                    };
                    factory.AutomaticRecoveryEnabled = true;
                }
                _Connection = factory.CreateConnection();
                _IsConnectOK = _Connection.IsOpen;
            }
            catch (Exception err)
            {
                Log.Write(err, "MQ.Rabbit");
            }
        }

        private IConnection _Connection;
        public IConnection DefaultConnection
        {
            get
            {
                return _Connection;
            }
        }

        public override bool Publish(MQMsg msg)
        {
            if (msg == null || (string.IsNullOrEmpty(msg.QueueName) && string.IsNullOrEmpty(msg.ExChange)))
            {
                return false;
            }
            if (!IsConnectOK) { return false; }
            try
            {
                var channel = DefaultConnection.CreateModel();
                if (!string.IsNullOrEmpty(msg.QueueName))
                {
                    channel.QueueDeclare(msg.QueueName, true, false, false, null);//定义持久化队列
                }
                string json = msg.ToJson();
                string exName = string.IsNullOrEmpty(msg.QueueName) ? msg.ExChange : "";//没有队列名，才能发送交换机。
                channel.BasicPublish(exName, msg.QueueName ?? "", null, body: Encoding.UTF8.GetBytes(json));
                channel.Dispose();
                return true;
            }
            catch (Exception err)
            {
                Log.Write(err, "MQ.Rabbit");
                return false;
            }
        }
        public override bool PublishBatch(List<MQMsg> msgList)
        {
            if (msgList == null || msgList.Count == 0)
            {
                return false;
            }
            if (!IsConnectOK) { return false; }
            try
            {
                //net 版本没有批量功能
                using (var channel = DefaultConnection.CreateModel())
                {
                    foreach (var msg in msgList)
                    {
                        if (string.IsNullOrEmpty(msg.QueueName) && string.IsNullOrEmpty(msg.ExChange))
                        {
                            continue;
                        }

                        if (!string.IsNullOrEmpty(msg.QueueName))
                        {
                            channel.QueueDeclare(msg.QueueName, true, false, false, null);//定义持久化队列
                        }
                        string json = msg.ToJson();
                        string exName = string.IsNullOrEmpty(msg.QueueName) ? msg.ExChange : "";//没有队列名，才能发送交换机。
                        channel.BasicPublish(exName, msg.QueueName ?? "", null, body: Encoding.UTF8.GetBytes(json));
                    }
                }
                return true;
            }
            catch (Exception err)
            {
                Log.Write(err, "MQ.Rabbit");
                return false;
            }
        }

        List<string> listenOKList = new List<string>();
        public override bool Listen(string queueName, OnReceivedDelegate onReceivedDelegate, string bindExName)
        {
            if (string.IsNullOrEmpty(queueName) || onReceivedDelegate == null)
            {
                return false;
            }
            if (!IsConnectOK)
            {
                if (!listenFailDic.ContainsKey(queueName))
                {
                    listenFailDic.Add(queueName, new ListenPara() { onReceivedDelegate = onReceivedDelegate, BindExName = bindExName });
                }
                return false;
            }
            try
            {
                if (!listenOKList.Contains(queueName))
                {
                    listenOKList.Add(queueName);
                    var channel = DefaultConnection.CreateModel();

                    channel.QueueDeclare(queueName, true, false, false, null);//定义持久化队列
                    if (!string.IsNullOrEmpty(bindExName))
                    {
                        //定义交换机
                        channel.ExchangeDeclare(bindExName, "fanout");
                        //绑定交换机
                        channel.QueueBind(queueName, bindExName, "");
                    }
                    var consumer = new EventingBasicConsumer(channel);
                    consumer.Received += (model, ea) =>
                    {
                        string json = Encoding.UTF8.GetString(ea.Body);
                        MQMsg msg = MQMsg.Create(json);

                        //反转队列名称和监听key
                        msg.QueueName = msg.CallBackName;
                        msg.CallBackName = queueName;

                        string subKey = msg.TaskKey;
                        msg.TaskKey = msg.CallBackKey;
                        msg.CallBackKey = subKey;

                        onReceivedDelegate(msg);
                    };
                    channel.BasicConsume(queueName, true, consumer);
                }
                return true;
            }
            catch (Exception err)
            {
                listenOKList.Remove(queueName);
                listenFailDic.Add(queueName, new ListenPara() { onReceivedDelegate = onReceivedDelegate, BindExName = bindExName });
                Log.Write(err, "MQ.Rabbit");
                return false;
            }

        }
    }
}
