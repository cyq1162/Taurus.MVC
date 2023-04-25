using System;
using System.Collections.Generic;
using System.Text;
using CYQ.Data.Cache;
using Microsoft.Extensions.Hosting;

namespace Taurus.Controllers
{
    /// <summary>
    /// Redis 并发 测试
    /// </summary>
    public class RedisController : Taurus.Mvc.Controller
    {

        public void Get()
        {
            CacheManage cache = CacheManage.RedisInstance;
            string value = cache.Get<string>("a");
            Write(value, value != null);
        }
        public void Set()
        {
            CacheManage cache = CacheManage.RedisInstance;
            bool result = cache.Set("a", DateTime.Now.Ticks);
           
            Write(cache.WorkInfo, result);
        }
    }
}
