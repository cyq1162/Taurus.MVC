using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CYQ.Data;
using CYQ.Data.Cache;
using CYQ.Data.Lock;
namespace Taurus.Controllers.Test
{
    /// <summary>
    /// 分布式锁测试
    /// </summary>
    internal class DLController:Taurus.Mvc.Controller
    {
        public DLController()
        {
            AppConfig.Redis.Servers = "127.0.0.1:6379";
        }
        public void Test()
        {
            var redisLock = DistributedLock.Redis;
            string lockKey = "myRedisLockKey";
            var isLockOK = false;
            try
            {
                isLockOK = redisLock.Lock(lockKey, 1000);
                if (isLockOK)
                {
                    Write("Lock OK!");
                }
            }
            finally
            {
                if(isLockOK)
                {
                    redisLock.UnLock(lockKey);
                }
            }
        }
    }
}
