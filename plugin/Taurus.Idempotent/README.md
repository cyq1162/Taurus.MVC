# Taurus.Idempotent is a idempotent lock for .net or .net core.
<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus.Idempotent 幂等性锁，使用 .Net Core 示例：
<h4>1、以 Nuget 中引入运行包：Taurus.Idempotent</h4>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240127204301089-832373500.png" alt="" loading="lazy" /></p>
<p></p>
<h4>2、进行编码：</h4>
<p>1、引入名称空间：</p>
<pre><code>using Taurus.Plugin.Idempotent;</code></pre>
<p>3、配置相关项：</p>
<pre><code>
1、Database 锁配置：
   IdempotentConfig.Conn = "server=.;database=mslog;uid=sa;pwd=123456";//由数据库链接决定启用什么链接
   IdempotentConfig.TableName = "taurus_idempotent";
2、Redis 锁配置：
  IdempotentConfig.RedisServers = "127.0.0.1:6379";
3、MemCache 锁配置：
  IdempotentConfig.MemCacheServers = "192.168.100.111:11211";
 

</code></pre>
<p>2、根据需要获得对应锁类型：</p>
<pre><code>
var dsLock = Idempotent.File;// Get File Lock
var dsLock = Idempotent.Database;// Get DataBase Lock
var dsLock = Idempotent.Redis;// Get Redis Lock
var dsLock = Idempotent.MemCache;// Get MemCache Lock    
</code></pre>

<p>3、进行锁、并释放锁：</p>
<pre><code>
 string key = "myLock";
 if (dsLock.Lock(key, 30000))
 {
    Console.Write(" -  OK - " + );
 } 
</code></pre>
<p></p>
<p>更详细使用见：/demo 运行示例。</p>
