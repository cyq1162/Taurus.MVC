using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Plugin.Limit
{
    /// <summary>
    /// Ack类【负责Ack的产生、解密、还有限制重复使用】 - 该加解密方式简单，适合App端随手编写加密算法。
    /// </summary>
    public static partial class AckLimit
    {
        /// <summary>
        /// 创建一个Ack（通常用于测试，为客户端追加一个可访问的ack）
        /// </summary>
        /// <returns></returns>
        public static string CreateAck()
        {
            //1、Key+时间戳
            string rndKey = LimitConfig.Ack.Key + DateTime.Now.Ticks.ToString();
            //2、转字节
            byte[] bytes = Encoding.ASCII.GetBytes(rndKey);
            //3、反转字节
            Array.Reverse(bytes);
            //4、字节转Base64
            string base64Key = Convert.ToBase64String(bytes);
            //5、返回 组合后的内容
            return "#" + (char)(DateTime.Now.Second + 65) + base64Key.Replace("=", "#");
        }
        /// <summary>
        /// 对ack进行解码。
        /// </summary>
        /// <returns></returns>
        public static string Decode(string ack)
        {
            //#tsNjExNzIwODU0OTc2NzI4NzUxODM2Mg##
            if (ack.Length > 10 && ack.StartsWith("#"))//sr
            {
                try
                {
                    string code = ack.Substring(2, ack.Length - 2);
                    byte[] bytes = Convert.FromBase64String(code.Replace("#", "="));
                    Array.Reverse(bytes);
                    return Encoding.ASCII.GetString(bytes);
                }
                catch
                {
                    return string.Empty;
                }

            }
            return string.Empty;
        }

    }

    public static partial class AckLimit
    {
        /// <summary>
        /// 检测ack是否有效。
        /// </summary>
        /// <returns></returns>
        public static bool IsValid(string ack)
        {
            if (string.IsNullOrEmpty(ack))
            {
                return false;
            }
            if (LimitConfig.Ack.IsVerifyDecode && !Decode(ack).StartsWith(LimitConfig.Ack.Key))
            {
                return false;
            }
            if (LimitConfig.Ack.IsVerifyUsed && AckLimit.IsAckUsed(ack))
            {
                return false;
            }
            return true;
        }
        /// <summary>
        /// 以5分钟为间隔，存储前半5分钟
        /// </summary>
        static Dictionary<int, byte> kvSmall = new Dictionary<int, byte>(10000);
        /// <summary>
        /// 以5分钟为间隔，存储后半5分钟
        /// </summary>
        static Dictionary<int, byte> kvBig = new Dictionary<int, byte>(10000);

        /// <summary>
        /// ack 是否已使用
        /// </summary>
        /// <returns></returns>
        public static bool IsAckUsed(string ack)
        {
            int code = ack.GetHashCode();
            Dictionary<int, byte> kv = GetKeyValuePairs();
            if (kv.ContainsKey(code))
            {
                return true;
            }
            kv.Add(code, 1);
            return false;
        }

        /// <summary>
        /// 根据时间间隔获取字典。
        /// </summary>
        /// <returns></returns>
        private static Dictionary<int, byte> GetKeyValuePairs()
        {
            string mi = DateTime.Now.Minute.ToString();
            if (mi.Length == 2)
            {
                mi = mi[1].ToString();
            }
            if (int.Parse(mi) >= 5)
            {
                kvSmall.Clear();
                return kvBig;
            }
            else
            {
                kvBig.Clear();
                return kvSmall;
            }
        }
    }
}
