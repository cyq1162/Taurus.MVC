# Taurus.Idempotent is a idempotent lock for .net or .net core.
<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus.Idempotent  .Net Core Example：
<h4>1、Reference Nuget package ：Taurus.Idempotent</h4>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240127204301089-832373500.png" alt="" loading="lazy" /></p>
<p></p>
<h4>2、Coding ：</h4>
<p>1、Using NameSpace ：</p>
<pre><code>using Taurus.Plugin.Idempotent;</code></pre>
<p>3、Config Items：</p>
<pre><code>
1、Database Lock Config：
   IdempotentConfig.Conn = "server=.;database=mslog;uid=sa;pwd=123456";
   IdempotentConfig.TableName = "taurus_idempotent";
2、Redis Lock Config ：
  IdempotentConfig.RedisServers = "127.0.0.1:6379";
3、MemCache Lock Config ：
  IdempotentConfig.MemCacheServers = "192.168.100.111:11211";
 

</code></pre>
<p>2、Get Lock Type ：</p>
<pre><code>
var dsLock = Idempotent.File;// Get File Lock
var dsLock = Idempotent.Database;// Get DataBase Lock
var dsLock = Idempotent.Redis;// Get Redis Lock
var dsLock = Idempotent.MemCache;// Get MemCache Lock    
</code></pre>

<p>3、Lock ：</p>
<pre><code>
 string key = "myLock";
 if (dsLock.Lock(key, 30000))
 {
    Console.Write(" -  OK - " + );
 } 
</code></pre>
<p></p>
<p>More to see ：/demo </p>
