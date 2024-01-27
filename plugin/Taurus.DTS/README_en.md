<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus DTC distributed transaction framework, using Net Core Example:：

<h4>Basic Description：</h4>
<p>1、The framework is divided into Client (client, i.e. calling end) and Server (server, i.e. service provider)。</p>
<p>2、The project requires selecting data storage type (database or distributed cache) and data transmission type (message queue)。</p>

<h4>Data Storage：</h4>
<p>Database selection (one of the more than 10 databases supported by CYQ. Data, such as MSSQL, MySql, Oracle, PostgreSQL, etc.)</p>
<p>The MSSQL configuration example is as follows：</p>
<pre><code>{
  "ConnectionStrings": {
    "DTC.Server.Conn": "server=.;database=MSLog;uid=sa;pwd=123456"
  }
}</code></pre>
<p>Distributed cache storage can also be chosen, and the configuration example is as follows (choose either one)：</p>
<pre><code>{
  "AppSettings": {
  "Redis.Servers":"127.0.0.1:6379 ,192.168.1.111:6379-withpassword",
  "MemCache.Servers":"127.0.0.1:11211" 
  }
}</code></pre>
<h4>Message queue：</h4>

<p>At present, the message queue supports RabbitMQ or Kafka (one can be configured)：</p>
<pre><code>{
  "AppSettings": {
  "DTC.Server.Rabbit":"127.0.0.1;guest;guest;/",//ip;username;password;virtualpath;
  "DTC.Server.Kafka":"127.0.0.1:9092" 
  }
}</code></pre>
<p>The above configuration is for the server side, and the client can change the server to the client side。</p>


# Server side usage examples：
<p>1、Nuget search Taurus Introducing DTC into engineering projects。</p>
<p>2、Program or Startup add service usage introduction:</p>
<pre><code>  services.AddTaurusDtc(); 
  app.UseTaurusDtc(StartType.Server); 
</code></pre>
<p>3、appsettings.json config：</p>
<pre><code>  {
  "ConnectionStrings": {
    "DTC.Server.Conn": "host=localhost;port=3306;database=cyqdata;uid=root;pwd=123456;Convert Zero Datetime=True;"
  },
  "AppSettings": {
    "DTC.Server.Rabbit": "127.0.0.1;guest;guest;/" //IP;UserName;Password;VirtualPaath
}</code></pre>
<p>4、Select the corresponding dependency components for the database, such as MySql, which can：</p>
<pre><code>You can search for MySql on Nuget Data, or CYQ Data MySql (which will automatically import MySql. Data) is available, just import the project.
</code></pre>
<p>5、Code writing can refer to the example code provided in the source code, as follows：</p>
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
            // Save transaction related information for subsequent callback processing, submission or rollback
            if (DTC.Server.Subscribe(createID.ToString(), "OnCreate")) 
            {
                Console.WriteLine("call : DTC.Server.Subscribe call.");
            }
            Write(createID, true);
        }


        [DTCServerSubscribe("OnCreate")] //Subscription callback, handling commit or rollback
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

        [DTCServerSubscribe("ToDoTask")] // Subscription Task
        private static bool DoSomeTask(DTCServerSubscribePara para)
        {
            Console.WriteLine("call :" + para.ExeType + " , content :" + para.Content);
            para.CallBackContent = "I do ok.";
            return true;
        }

    }
</code></pre>

# Client Example of End Use：
<p>1、Nuget search Taurus Introducing DTC into engineering projects。</p>
<p>2、Program or Startup adding service usage introduction：</p>
<pre><code>  services.AddTaurusDtc(); 
  app.UseTaurusDtc(StartType.Client); 
</code></pre>
<p>3、appsettings.json config：</p>
<pre><code>  {
  "ConnectionStrings": {
    "DTC.Client.Conn": "host=localhost;port=3306;database=cyqdata;uid=root;pwd=123456;Convert Zero Datetime=True;"
  },
  "AppSettings": {
    "DTC.Client.Rabbit": "127.0.0.1;guest;guest;/" //IP;UserName;Password;VirtualPaath
}</code></pre>
<p>4、Select the corresponding dependency components for the database, such as MySql, which can：</p>
<pre><code>You can search for MySql on Nuget Data, or CYQ Data MySql (which will automatically import MySql. Data) is available, just import the project.
</code></pre>
<p>5、Code writing can refer to the example code provided in the source code, as follows：</p>
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

# A comprehensive collection of various database linking statements
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

Due to the basic consistency of various database linking statements, except for specific writing methods, they can be supplemented through linking：provider=mssql、provider=mysql、provider=db2、provider=postgre。
###--------------------------------------------------------###
</code></pre>