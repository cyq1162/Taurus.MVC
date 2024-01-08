using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Plugin.DistributedTransaction
{

    internal class MQRabbit : MQ
    {
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
        }


        private IConnection _Connection;
        public IConnection DefaultConnection
        {
            get
            {
                if (_Connection == null)
                {
                    _Connection = factory.CreateConnection();
                }
                if (!_Connection.IsOpen)
                {
                    _Connection.Close();
                    _Connection = factory.CreateConnection();
                }
                return _Connection;
            }
        }

        public override bool Publish(MQMsg msg)
        {
            try
            {
                if (msg == null || (string.IsNullOrEmpty(msg.QueueName) && string.IsNullOrEmpty(msg.ExChange)))
                {
                    return false;
                }
                using (var channel = DefaultConnection.CreateModel())
                {
                    if (!string.IsNullOrEmpty(msg.QueueName))
                    {
                        channel.QueueDeclare(msg.QueueName, true, false, false, null);//定义持久化队列
                    }
                    string json = msg.ToJson();
                    string exName = string.IsNullOrEmpty(msg.QueueName) ? msg.ExChange : "";//没有队列名，才能发送交换机。
                    channel.BasicPublish(exName, msg.QueueName ?? "", null, body: Encoding.UTF8.GetBytes(json));
                }
                return true;
            }
            catch (Exception err)
            {
                CYQ.Data.Log.Write(err, "RabbitMQ");
                return false;
            }
        }
        public override bool PublishBatch(List<MQMsg> msgList)
        {
            if (msgList == null || msgList.Count == 0) { return false; }
            try
            {

                using (var channel = DefaultConnection.CreateModel())
                {
                    var pub = channel.CreateBasicPublishBatch();

                    List<string> queueNames = new List<string>();
                    foreach (MQMsg msg in msgList)
                    {
                        if (string.IsNullOrEmpty(msg.QueueName) && string.IsNullOrEmpty(msg.ExChange))
                        {
                            continue;
                        }
                        if (!string.IsNullOrEmpty(msg.QueueName) && !queueNames.Contains(msg.QueueName))
                        {
                            queueNames.Add(msg.QueueName);
                            channel.QueueDeclare(msg.QueueName, true, false, false, null);//定义持久化队列
                        }
                        string json = msg.ToJson();
                        string exName = string.IsNullOrEmpty(msg.QueueName) ? msg.ExChange : "";//没有队列名，才能发送交换机。
                        pub.Add(exName, msg.QueueName ?? "", false, null, body: Encoding.UTF8.GetBytes(json));
                    }
                    pub.Publish();

                }
                return true;
            }
            catch (Exception err)
            {
                CYQ.Data.Log.Write(err, "RabbitMQ");
                return false;
            }
        }

        List<string> listenQueueList = new List<string>();
        public override bool Listen(string queueName, OnReceivedDelegate onReceivedDelegate, string bindExName)
        {
            if (string.IsNullOrEmpty(queueName) || onReceivedDelegate == null)
            {
                return false;
            }

            try
            {
                if (!listenQueueList.Contains(queueName))
                {
                    listenQueueList.Add(queueName);
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
                        string json = Encoding.UTF8.GetString(ea.Body.ToArray());
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
                listenQueueList.Remove(queueName);
                CYQ.Data.Log.Write(err, "RabbitMQ");
                return false;
            }

        }
    }
}
