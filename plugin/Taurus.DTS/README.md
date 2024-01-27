# Taurus.DTS is Distributed task scheduler

<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus.DTS 微服务分布式任务框架，使用.Net Core 示例：

<h4>基础说明：</h4>
<p>1、框架分为 Client（客户端，即任务发起端）和 Server（服务端，即方法订阅方）。</p>
<p>2、框架支持即时任务、延时任务、Cron表达式任务定时任务、广播任务，四种方式。</p>
<p>3、项目需要配置的参数：1、数据库（可选）；2、MQ（必选）。</p>

<h4>数据存储：</h4>
<p>可选择数据库（MSSQL、MySql、Oracle、PostgreSql 等 CYQ.Data 所支持的10多种数据库之一）</p>
<p>MSSQL配置示例如下：</p>
<pre><code>{
  "ConnectionStrings": {
    "DTS.Server.Conn": "server=.;database=MSLog;uid=sa;pwd=123456"
  }
}</code></pre>
<h4>消息队列：</h4>

<p>目前消息队列支持 RabbitMQ 或者 Kafka（配置其中一方即可）：</p>
<pre><code>{
  "AppSettings": {
  "DTS.Server.Rabbit":"127.0.0.1;guest;guest;/",//ip;username;password;virtualpath;
  "DTS.Server.Kafka":"127.0.0.1:9092" 
  }
}</code></pre>
<p>以上配置为Server端，客户端更改 Server 为 Client 即可。</p>


# Server 端 使用示例：
<p>1、Nuget 搜索 Taurus.DTS 引入工程项目中。</p>
<p>2、如果是ASP.Net Core 程序：Program 或 Startup 添加服务使用引入：</p>
<pre><code>  services.AddTaurusDts(); // 服务添加
  app.UseTaurusDts(TaskStartType.Server); //服务使用，启用服务端
</code></pre>
<p>3、appsettings.json 配置基本属性：</p>
<pre><code>  {
  "ConnectionStrings": {
    "DTS.Server.Conn": "host=localhost;port=3306;database=cyqdata;uid=root;pwd=123456;Convert Zero Datetime=True;"
  },
  "AppSettings": {
    "DTS.Server.Rabbit": "127.0.0.1;guest;guest;/" //IP;UserName;Password;VirtualPaath
}</code></pre>
<p>4、选择数据库对应的依赖组件，如MySql，可以：</p>
<pre><code>Nuget 上可以搜索 MySql.Data 、或者 CYQ.Data.MySql (会自动引入MySql.Data)  都可， 引入项目即可。
</code></pre>
<p>5、代码编写，可以参考源码中提供的示例代码，如下为控制台示例代码：</p>
<pre><code>
using System;
using Taurus.Plugin.DistributedTask;

namespace Console_App_Server
{

     internal class Program
     {
     
         static void Main(string[] args)
        {

            DTSConfig.Server.Rabbit = "127.0.0.1;guest;guest;/";
            //DTSConfig.Server.Kafka = "127.0.0.1:9092;";
            //DTSConfig.Server.Conn = DTSConfig.Client.Conn;

            DTSConfig.ProjectName = "ConsoleApp5";

            DTS.Server.Start();//start client and server

            Console.WriteLine("---------------------------------------");

            Console.ReadLine();
        }


    }


    /// <summary>
    /// 服务端 server class need to public
    /// </summary>
    public class Server
    {
        [DTSSubscribe("DoInstantTask")]
        public static bool A(DTSSubscribePara para)
        {
            para.CallBackContent = "show you a.";
            return true;
        }

        [DTSSubscribe("DoDelayTask")]
        private static bool B(DTSSubscribePara para)
        {
            para.CallBackContent = "show you b.";
            return true;
        }
        [DTSSubscribe("DoCronTask")]
        private static bool C(DTSSubscribePara para)
        {
            para.CallBackContent = "show you c.";
            return true;
        }
        /// <summary>
        /// 定时任务
        /// </summary>
        [DTSSubscribe("DoBroadastTask")]
        private static bool TimerTask(DTSSubscribePara para)
        {
            para.CallBackContent = "show you d.";
            return true;
        }
    }
}

</code></pre>

# Client 端 使用示例：
<p>1、Nuget 搜索 Taurus.DTS 引入工程项目中。</p>
<p>2、如果是ASP.Net Core 程序：Program 或 Startup 添加服务使用引入：</p>
<pre><code>  services.AddTaurusDts(); // 服务添加
  app.UseTaurusDts(StartType.Client); //服务使用，启用服务端
</code></pre>
<p>3、appsettings.json 配置基本属性：</p>
<pre><code>  {
  "ConnectionStrings": {
    "DTS.Client.Conn": "host=localhost;port=3306;database=cyqdata;uid=root;pwd=123456;Convert Zero Datetime=True;"
  },
  "AppSettings": {
    "DTS.Client.Rabbit": "127.0.0.1;guest;guest;/" //IP;UserName;Password;VirtualPaath
}</code></pre>
<p>4、选择数据库对应的依赖组件，如MySql，可以：</p>
<pre><code>Nuget 上可以搜索 MySql.Data 、或者 CYQ.Data.MySql (会自动引入MySql.Data)  都可， 引入项目即可。
</code></pre>
<p>5、代码编写，可以参考源码中提供的示例代码，如下为控制台示例代码：</p>
<pre><code>
using System;
using System.Threading;
using Taurus.Plugin.DistributedTask;

namespace Console_App_Client
{

      internal class Program
      {
      
        static void Main(string[] args)
        {
        
            DTSConfig.Client.IsPrintTraceLog = false;
            //AppConfig.Redis.Servers = "127.0.0.1:6379";

            DTSConfig.Client.Rabbit = "127.0.0.1;guest;guest;/";
            //DTSConfig.Client.Kafka = "127.0.0.1:9092;";
            DTSConfig.Client.Conn = "server=.;database=mslog;uid=sa;pwd=123456";

            DTSConfig.ProjectName = "ConsoleApp5";

            DTS.Client.Start();//start client and server
            
            Console.WriteLine("---------------------------------------");
            Console.WriteLine("1-InstantTask、2-DelayTask（1Minutes）、3-CronTask、4-DeleteCronTask、5-BroadastTask");
            Console.WriteLine("Input ：1、2、3、4、5，Press Enter.");
            while (true)
            {
                string line = Console.ReadLine();
                try
                {
                    Client.Run(int.Parse(line));
                }
                catch(Exception err)
                {
                    Console.WriteLine(err.Message);
                }
                
            }

        }
    }

    /// <summary>
    /// 客户端 client class need to public if has callback method.
    /// </summary>
    public class Client
    {
        public static void Run(int i)
        {

            if (i == 2)
            {
                //发布一个延时1分钟的任务
                DTS.Client.Delay.PublishAsync(1, "i publish a delay task.", "DoDelayTask", "DelayCallBack");
                Console.WriteLine("Wait for 1 minute...");
            }
            else if (i == 3)
            {
                //发布一个秒在30时的循环任务。
                DTS.Client.Cron.PublishAsync("10,30,50 * * * * ?", "i publish a timer task with cron express.", "DoCronTask", "CronCallBack");
                Console.WriteLine("Wait for execute task when second is 10,30,50...");
            }
            else if (i == 4)
            {
                //发布一个秒在30时的循环任务。
                DTS.Client.Cron.DeleteAsync("DoCronTask", null, "CronCallBack");
            }
            else if (i == 5)
            {
                //发布一个广播任务
                DTS.Client.Broadast.PublishAsync("i publish a task for all server.", "DoBroadastTask", "BroadastCallBack");
            }
            else
            {
                for (int k = 0; k < 1; k++)
                {
                    //发布一个即时任务
                    DTS.Client.Instant.PublishAsync("i publish a task instantly.", "DoInstantTask", "InstantCallBack");
                    Console.WriteLine(k);
                }
                
            }
        }

        [DTSCallBack("InstantCallBack")]
        [DTSCallBack("DelayCallBack")]
        [DTSCallBack("CronCallBack")]
        [DTSCallBack("BroadastCallBack")]
        private static void OnCallBack(DTSCallBackPara para)
        {
            Console.WriteLine("Client callback : " + para.TaskType + " - " + para.CallBackKey + " - " + para.CallBackContent);
        }
    }
}

</code></pre>

# 各种数据库链接语句大全
<pre><code>
###--------------------------------------------------------###

   Txt::  Txt Path=E:\
   Xml::  Xml Path=E:\
Access::  Provider=Microsoft.Jet.OLEDB.4.0; Data Source=E:\cyqdata.mdb
Sqlite::  Data Source=E:\cyqdata.db;failifmissing=false;
 MySql::  host=localhost;port=3306;database=cyqdata;uid=root;pwd=123456;Convert Zero Datetime=True;
 Mssql::  server=.;database=cyqdata;uid=sa;pwd=123456;provider=mssql; 
Sybase::  data source=127.0.0.1;port=5000;database=cyqdata;uid=sa;pwd=123456;provider=sybase; 
Postgre:  server=localhost;uid=sa;pwd=123456;database=cyqdata;provider=pg; 
    DB2:  Database=SAMPLE;User ID=administrator;Server=127.0.0.1;password=1234560;provider=db2; 
FireBird  user id=SYSDBA;password=123456;database=d:\\test.dbf;server type=Default;data source=127.0.0.1;port number=3050;provider=firebird;
Dameng::  user id=SYSDBA;password=123456789;data source=127.0.0.1;schema=test;provider=dameng;
KingBaseES server=127.0.0.1;User Id=system;Password=123456;Database=test;Port=54321;schema=public;provider=kingbasees;
Oracle ODP.NET::
Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=127.0.0.1)(PORT = 1521)))(CONNECT_DATA =(SID = orcl)));User ID=sa;password=123456

由于各种数据库链接语句基本一致，除了特定写法外，可以通过链接补充：provider=mssql、provider=mysql、provider=db2、provider=postgre等来区分。
###--------------------------------------------------------###
</code></pre>

