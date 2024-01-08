using Confluent.Kafka;
using Confluent.Kafka.Admin;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Taurus.Plugin.DistributedTransaction
{

    internal class MQKafka : MQ
    {
        public override MQType MQType
        {
            get
            {
                return MQType.Kafka;
            }
        }
        bool isClient = false;
        string servers = string.Empty;
        public MQKafka(string mqConn, bool isClient)
        {
            servers = mqConn;
            this.isClient = isClient;
        }

        public override bool Publish(MQMsg msg)
        {
            try
            {
                if (msg == null || (string.IsNullOrEmpty(msg.QueueName) && string.IsNullOrEmpty(msg.ExChange)))
                {
                    return false;
                }
                var config = new ProducerConfig
                {
                    BootstrapServers = servers,
                    Acks = 0 //保持性能，不需要等待确认，即可发送下一条信息，允许数据丢失。
                };
                string json = msg.ToJson();
                var data = new Message<string, string> { Key = null, Value = json };

                using (var producer = new ProducerBuilder<string, string>(config).Build())
                {
                    if (string.IsNullOrEmpty(msg.QueueName))
                    {
                        List<string> topics = GetTopics(msg.ExChange);
                        if (topics == null || topics.Count == 0) { return false; }
                        foreach (var topic in topics)
                        {
                            producer.ProduceAsync(topic, data);
                        }
                    }
                    else
                    {
                        producer.ProduceAsync(msg.QueueName, data);
                    }
                    producer.Flush();
                }
                return true;
            }
            catch (Exception err)
            {
                CYQ.Data.Log.Write(err, "MQ.Kafka");
                return false;
            }
        }
        public override bool PublishBatch(List<MQMsg> msgList)
        {
            if (msgList == null || msgList.Count == 0) { return false; }
            try
            {
                var config = new ProducerConfig
                {
                    BootstrapServers = servers,
                    Acks = 0 //保持性能，不需要等待确认，即可发送下一条信息，允许数据丢失。
                };
                using (var producer = new ProducerBuilder<string, string>(config).Build())
                {
                    foreach (var msg in msgList)
                    {
                        string json = msg.ToJson();
                        var data = new Message<string, string> { Key = null, Value = json };

                        if (string.IsNullOrEmpty(msg.QueueName))
                        {
                            List<string> topics = GetTopics(msg.ExChange);
                            if (topics == null || topics.Count == 0) { return false; }
                            foreach (var topic in topics)
                            {
                                producer.Produce(topic, data);
                            }
                        }
                        else
                        {
                            producer.Produce(msg.QueueName, data);
                        }
                    }
                    producer.Flush();
                }
                return true;
            }
            catch (Exception err)
            {
                CYQ.Data.Log.Write(err, "MQ.Kafka");
                return false;
            }
        }
        public override bool Listen(string topic, OnReceivedDelegate onReceivedDelegate, string exName)
        {
            if (string.IsNullOrEmpty(topic) || onReceivedDelegate == null)
            {
                return false;
            }
            try
            {
                if (CreateTopicIfNoExists(topic))
                {
                    ListenPara para = new ListenPara() { Topic = topic, OnReceivedDelegate = onReceivedDelegate };
                    ThreadPool.QueueUserWorkItem(new WaitCallback(ListenThread), para);
                    return true;
                }
            }
            catch (Exception err)
            {
                CYQ.Data.Log.Write(err, "MQ.Kafka");
            }
            return false;
        }

        private void OnReceive(object topicObj)
        {
            ListenPara para = topicObj as ListenPara;
            MQMsg msg = MQMsg.Create(para.Message);

            //反转队列名称和监听key
            msg.QueueName = msg.CallBackName;
            msg.CallBackName = para.Topic;

            string subKey = msg.TaskKey;
            msg.TaskKey = msg.CallBackKey;
            msg.CallBackKey = subKey;
            para.OnReceivedDelegate(msg);
        }
        private void ListenThread(object topicObj)
        {
            ListenPara para = topicObj as ListenPara;
            try
            {
                var config = new ConsumerConfig
                {
                    BootstrapServers = servers,
                    GroupId = para.Topic,
                    EnableAutoCommit = true,
                    AutoOffsetReset = AutoOffsetReset.Earliest
                };
                using (var consumer = new ConsumerBuilder<Ignore, string>(config).Build())
                {
                    consumer.Subscribe(para.Topic);
                    
                        while (true)
                        {
                            try
                            {
                                var consumeResult = consumer.Consume();
                                para.Message = consumeResult.Value;
                                ThreadPool.QueueUserWorkItem(new WaitCallback(OnReceive), para);
                            }
                            catch (Exception err)
                            {
                                CYQ.Data.Log.Write(err, "MQ.Kafka");
                            }
                        }
                   
                }

            }
            catch (Exception err)
            {
                CYQ.Data.Log.Write(err, "MQ.Kafka");
            }

        }

        /// <summary>
        /// 根据交换机名称获取所有主题名称
        /// </summary>
        private List<string> GetTopics(string exName)
        {
            var config = new AdminClientConfig
            {
                BootstrapServers = servers // Kafka 服务器地址和端口
            };
            List<string> topicList = new List<string>();
            using (var adminClient = new AdminClientBuilder(config).Build())
            {
                var metadata = adminClient.GetMetadata(TimeSpan.FromSeconds(10));

                foreach (var topic in metadata.Topics)
                {
                    if (topic.Topic.EndsWith(exName))
                    {
                        topicList.Add(topic.Topic);
                    }
                }
            }
            return topicList;
        }
        private bool CreateTopicIfNoExists(string topic)
        {
            try
            {
                var config = new AdminClientConfig
                {
                    BootstrapServers = servers // Kafka 服务器地址和端口
                };
                using (var adminClient = new AdminClientBuilder(config).Build())
                {
                    var metadata = adminClient.GetMetadata(topic, TimeSpan.FromSeconds(10));
                    if (metadata.Topics[0].Error.IsError)
                    {
                        // 创建主题
                        var topicSpecification = new TopicSpecification
                        {
                            Name = topic,
                            NumPartitions = 1,
                            ReplicationFactor = 1
                        };
                        adminClient.CreateTopicsAsync(new[] { topicSpecification }).GetAwaiter().GetResult();
                    }
                }
                return true;
            }
            catch (Exception err)
            {
                CYQ.Data.Log.Write(err, "MQ.Kafka");
                return false;
            }

        }

        public class ListenPara
        {
            public string Topic { get; set; }
            public string Message { get; set; }
            public OnReceivedDelegate OnReceivedDelegate { get; set; }
        }
    }


}
