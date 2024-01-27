# Taurus.DistributedLock is a distributed lock for .net or .net core.
<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus.DistributedLock Use Net Core Example：
<h4>1、Reference Nuget package : Taurus.DistributedLock</h4>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240127202232791-2003546912.png" alt="" loading="lazy" /></p>

<h4>2、Coding ：</h4>
<p>1、Using NameSpace：</p>
<pre><code>using Taurus.Plugin.DistributedLock;</code></pre>
<p>3、Config Items：</p>
<pre><code>
1、Database Lock Config：
   DLockConfig.Conn = "server=.;database=mslog;uid=sa;pwd=123456";
   DLockConfig.TableName = "taurus_lock";
2、Redis Lock Config：
  DLockConfig.RedisServers = "127.0.0.1:6379";
3、MemCache Lock Config：
  DLockConfig.MemCacheServers = "192.168.100.111:11211";
 

</code></pre>
<p>2、Get Lock Type ：</p>
<pre><code>
var dsLock = DLock.File;// Get File Lock
var dsLock = DLock.Local;// Get Local Lock
var dsLock = DLock.Database;// Get DataBase Lock
var dsLock = DLock.Redis;// Get Redis Lock
var dsLock = DLock.MemCache;// Get MemCache Lock    
</code></pre>

<p>3、Lock and UnLock ：</p>
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
<p>more to see ：/demo </p>
