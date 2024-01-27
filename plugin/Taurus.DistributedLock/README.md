# Taurus.DistributedLock is a distributed lock for .net or .net core.
<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus.DistributedLock 分布式锁，使用 .Net Core 示例：
<h4>1、以 Nuget 中引入运行包：Taurus.DistributedLock</h4>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240127202232791-2003546912.png" alt="" loading="lazy" /></p>

<h4>2、进行编码：</h4>
<p>1、引入名称空间：</p>
<pre><code>using Taurus.Plugin.DistributedLock;</code></pre>
<p>3、配置相关项：</p>
<pre><code>
1、Database 锁配置：
   DLockConfig.Conn = "server=.;database=mslog;uid=sa;pwd=123456";//由数据库链接决定启用什么链接
   DLockConfig.TableName = "taurus_lock";
2、Redis 锁配置：
  DLockConfig.RedisServers = "127.0.0.1:6379";
3、MemCache 锁配置：
  DLockConfig.MemCacheServers = "192.168.100.111:11211";
 

</code></pre>
<p>2、根据需要获得对应锁类型：</p>
<pre><code>
var dsLock = DLock.File;// Get File Lock
var dsLock = DLock.Local;// Get Local Lock
var dsLock = DLock.Database;// Get DataBase Lock
var dsLock = DLock.Redis;// Get Redis Lock
var dsLock = DLock.MemCache;// Get MemCache Lock    
</code></pre>

<p>3、进行锁、并释放锁：</p>
<pre><code>
 string key = "myLock";
 bool isOK = false;
 try
 {
     isOK = dsLock.Lock(key, 30000);
     if (isOK)
     {
         Console.Write(" -  OK - " + );
     }
 }
 finally
 {
     if (isOK)
     {
         dsLock.UnLock(key);
     }
 }  
</code></pre>
<p></p>
<p>更详细使用见：/demo 运行示例。</p>
