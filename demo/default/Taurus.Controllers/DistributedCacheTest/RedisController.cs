using System;
using CYQ.Data;
using CYQ.Data.Cache;

namespace Taurus.Controllers
{
    /// <summary>
    /// Redis 并发 测试
    /// </summary>
    public class RedisController : Taurus.Mvc.Controller
    {
        public RedisController()
        {
            //对应配置："Redis.Servers": "127.0.0.1:6379"
            AppConfig.Redis.Servers = "127.0.0.1:6379";
        }
        /// <summary>
        /// 读取一个值
        /// </summary>
        public void Get()
        {
            DistributedCache cache = DistributedCache.Redis;
            string value = cache.Get<string>("a");
            Write(value, value != null);
        }
        /// <summary>
        /// 写入一个值
        /// </summary>
        public void Set()
        {
            DistributedCache cache = DistributedCache.Redis;
            bool result = cache.Set("a", DateTime.Now.Ticks);
           
            Write(cache.WorkInfo, result);
        }
    }
}
