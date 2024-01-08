
using CYQ.Data.Tool;
using System.Collections.Generic;

namespace Taurus.Plugin.DistributedTransaction
{
    internal enum KeyType
    {
        ClientRabbit = 1,
        ServerRabbit = 2,

        ClientKafka = 11,
        ServerKafka = 12,

    }
    internal abstract partial class MQ
    {
        public delegate void OnReceivedDelegate(MQMsg msg);

        private static MDictionary<string, MQ> instanceDic = new MDictionary<string, MQ>();
        private static MQ GetInstance(KeyType keyType, string conn)
        {
            int type = (int)(keyType);
            string key = type + conn;
            if (instanceDic.ContainsKey(key))
            {
                return instanceDic[key];
            }
            if (type < 10)
            {
                var rabbit = new MQRabbit(conn);
                instanceDic.Add(key, rabbit);
            }
            else
            {
                var kafka = new MQKafka(conn, keyType == KeyType.ClientKafka);
                instanceDic.Add(key, kafka);
            }
            return instanceDic[key];

        }

        public static MQ Client
        {
            get
            {
                if (!string.IsNullOrEmpty(DTCConfig.Client.Rabbit))
                {
                    return GetInstance(KeyType.ClientRabbit, DTCConfig.Client.Rabbit);
                }
                else if (!string.IsNullOrEmpty(DTCConfig.Client.Kafka))
                {
                    return GetInstance(KeyType.ClientKafka, DTCConfig.Client.Kafka);
                }
                return Empty.Instance;
            }
        }
        public static MQ Server
        {
            get
            {
                if (!string.IsNullOrEmpty(DTCConfig.Server.Rabbit))
                {
                    return GetInstance(KeyType.ServerRabbit, DTCConfig.Server.Rabbit);
                }
                else if (!string.IsNullOrEmpty(DTCConfig.Server.Kafka))
                {
                    return GetInstance(KeyType.ServerKafka, DTCConfig.Server.Kafka);
                }
                return Empty.Instance;
            }
        }
        public abstract MQType MQType { get; }
        public abstract bool Publish(MQMsg msg);
        public abstract bool PublishBatch(List<MQMsg> msgList);
        public abstract bool Listen(string queueNameOrGroupName, OnReceivedDelegate onReceivedDelegate, string bindExNameOrTopicName);
    }

    internal class Empty : MQ
    {
        public static readonly Empty Instance = new Empty();

        public override MQType MQType
        {
            get
            {
                return MQType.Empty;
            }
        }

        public override bool Listen(string queueName, OnReceivedDelegate onReceivedDelegate, string bindExName)
        {
            return false;
        }

        public override bool Publish(MQMsg msg)
        {
            return false;
        }

        public override bool PublishBatch(List<MQMsg> msgList)
        {
            return false;
        }
    }

    internal enum MQType
    {
        Empty,
        Rabbit,
        Kafka
    }
}
