<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus.DTC 微服务分布式事务框架，使用.Net Core 示例：

<h4>基础说明：</h4>
<p>1、框架分为 Client（客户端，即调用端）和 Server（服务端，即服务提供方）。</p>
<p>2、项目需要选择数据存储类型（数据库或分布式缓存）和数据传输类型（消息队列）。</p>

<h4>数据存储：</h4>
<p>可选择数据库（MSSQL、MySql、Oracle、PostgreSql 等 CYQ.Data 所支持的10多种数据库之一）</p>
<p>MSSQL配置示例如下：</p>
<pre><code>{
  "ConnectionStrings": {
    "DTC.Server.Conn": "server=.;database=MSLog;uid=sa;pwd=123456"
  }
}</code></pre>
<p>也可选择分布式缓存存储，配置示例如下（二者选其一即可）：</p>
<pre><code>{
  "AppSettings": {
  "Redis.Servers":"127.0.0.1:6379 ,192.168.1.111:6379-withpassword",
  "MemCache.Servers":"127.0.0.1:11211" 
  }
}</code></pre>
<h4>消息队列：</h4>

<p>目前消息队列支持 RabbitMQ 或者 Kafka（配置其中一方即可）：</p>
<pre><code>{
  "AppSettings": {
  "DTC.Server.Rabbit":"127.0.0.1;guest;guest;/",//ip;username;password;virtualpath;
  "DTC.Server.Kafka":"127.0.0.1:9092" 
  }
}</code></pre>
<p>以上配置为Server端，客户端更改 Server 为 Client 即可。</p>


# Server 端 使用示例：
<p>1、Nuget 搜索 Taurus.DTC 引入工程项目中。</p>
<p>2、Program 或 Startup 添加服务使用引入：</p>
<pre><code>  services.AddTaurusDtc(); // 服务添加
  app.UseTaurusDtc(StartType.Server); //服务使用，启用服务端
</code></pre>
<p>3、appsettings.json 配置基本属性：</p>
<pre><code>  {
  "ConnectionStrings": {
    "DTC.Server.Conn": "host=localhost;port=3306;database=cyqdata;uid=root;pwd=123456;Convert Zero Datetime=True;"
  },
  "AppSettings": {
    "DTC.Server.Rabbit": "127.0.0.1;guest;guest;/" //IP;UserName;Password;VirtualPaath
}</code></pre>
<p>4、选择数据库对应的依赖组件，如MySql，可以：</p>
<pre><code>Nuget 上可以搜索 MySql.Data 、或者 CYQ.Data.MySql (会自动引入MySql.Data)  都可， 引入项目即可。
</code></pre>
<p>5、代码编写，可以参考源码中提供的示例代码，如下：</p>
<pre><code>
     public class ServerController : Taurus.Mvc.Controller
    {

        /// <summary>
        /// provide a Create api , and it provide a transation , call https://localhost:5001/server/create
        /// </summary>
        [HttpPost]
        [Require("name")]
        public void Create(string name)
        {
            //do something insert
            int createID = 123456;
            //here will receive a header:X-Request-ID 
            if (DTC.Server.Subscribe(createID.ToString(), "OnCreate")) // 事务相关信息保存，以便后续回调处理提交或回滚
            {
                Console.WriteLine("call : DTC.Server.Subscribe call.");
            }
            Write(createID, true);
        }


        [DTCServerSubscribe("OnCreate")] //订阅回调，处理提交或回滚
        private static bool AnyMethodNameForOnCreateCallBack(DTCServerSubscribePara para)
        {
            para.CallBackContent = "what message you need?";
            Console.WriteLine("call back :" + para.ExeType + " , content :" + para.Content);
            if (para.ExeType == ExeType.Commit) { return true; }
            if (para.ExeType == ExeType.RollBack)
            {
                string createID = para.Content;
                //return DeleteByID(createID);
                return true;
            }
            return false;
        }

        [DTCServerSubscribe("ToDoTask")] // 订阅任务
        private static bool DoSomeTask(DTCServerSubscribePara para)
        {
            Console.WriteLine("call :" + para.ExeType + " , content :" + para.Content);
            para.CallBackContent = "I do ok.";
            return true;
        }

    }
</code></pre>

# Client 端 使用示例：
<p>1、Nuget 搜索 Taurus.DTC 引入工程项目中。</p>
<p>2、Program 或 Startup 添加服务使用引入：</p>
<pre><code>  services.AddTaurusDtc(); // 服务添加
  app.UseTaurusDtc(StartType.Client); //服务使用，启用服务端
</code></pre>
<p>3、appsettings.json 配置基本属性：</p>
<pre><code>  {
  "ConnectionStrings": {
    "DTC.Client.Conn": "host=localhost;port=3306;database=cyqdata;uid=root;pwd=123456;Convert Zero Datetime=True;"
  },
  "AppSettings": {
    "DTC.Client.Rabbit": "127.0.0.1;guest;guest;/" //IP;UserName;Password;VirtualPaath
}</code></pre>
<p>4、选择数据库对应的依赖组件，如MySql，可以：</p>
<pre><code>Nuget 上可以搜索 MySql.Data 、或者 CYQ.Data.MySql (会自动引入MySql.Data)  都可， 引入项目即可。
</code></pre>
<p>5、代码编写，可以参考源码中提供的示例代码，如下：</p>
<pre><code>
    public class ClientController : Taurus.Mvc.Controller
    {
        [HttpGet]
        public void Transation()
        {
            //do something
            RpcTask task = Rpc.StartPostAsync("https://localhost:5001/server/create", Encoding.UTF8.GetBytes("name=hello world"));
            if (task.Result.IsSuccess)
            {
                if (JsonHelper.IsSuccess(task.Result.ResultText))
                {
                    if (DTC.Client.CommitAsync(1, "OnOK"))
                    {
                        Console.WriteLine("call : DTC.Client.CommitAsync.");
                    }
                    Write("Commit OK.", true);
                    return;
                }
            }
            if (DTC.Client.RollBackAsync(1, "OnFail"))
            {
                Console.WriteLine("call : DTC.Client.RollBackAsync call.");
            }
            Write("RollBack ing....", false);
        }


        [DTCClientCallBack("OnFail")]
        [DTCClientCallBack("OnOK")]
        [DTCClientCallBack("OnDoOK")]
        private void OnCallBack(DTCClientCallBackPara para)
        {
            Console.WriteLine("call back : " + para.ExeType + " - " + para.CallBackKey + " - " + para.CallBackContent);
        }


        /// <summary>
        /// to publish a new task , start https://localhost:5000/client/publishtask
        /// </summary>
        [HttpGet]
        public void PublishTask()
        {
            if (DTC.Client.PublishTaskAsync("I give you some info.", "ToDoTask", "OnDoOK"))
            {
                Console.WriteLine("call : DTC.Client.PublishTaskAsync.");
            }
            Write("Publish Task OK.", true);
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
