1：官网示例 ：http://taurus.cyqdata.com
2：开源地址 ：https://github.com/cyq1162/Taurus.MVC
3：CYQ.Data 开源地址: https://github.com/cyq1162/cyqdata

使用注意事项：
1：配置文件说明
------------------------------------------------------------------------------
<configuration>
  <appSettings>
    <!--这里要改成：控制器所在的项目编绎后的dll名称（不包括后缀）-->
    <add key="Taurus.Controllers" value="项目编绎后的dll名称" />
    <!--指定处理的后缀（默认无后缀，可配置.shtml）-->
    <add key="Taurus.Suffix" value=""/>
	 <!--是否允许跨域请求，默认true-->
    <add key="Taurus.IsAllowCORS" value="true"/>
    <!--路由模式【值为0,1或2】[默认为1]
      值为0：匹配{Action}/{Para}
      值为1：匹配{Controller}/{Action}/{Para}
      值为2：匹配{Module}/{Controller}/{Action}/{Para}-->
    <add key="Taurus.RouteMode" value="1"/>
    <!--指定页面起始访问路径-->
    <add key="Taurus.DefaultUrl" value="home/index"/>
	 <!--是否启动创建API文档，访问路径为：/doc/default,需要保留源码里/Views/Doc目录下的文件-->
    <add key="Taurus.IsStartDoc" value="true"/>
	<!--是否启动默认的Token机制，可配置的映射字段：TableName,UserName,Password(这三个必填写，后面可选）,
	FullName,Status,PasswordExpireTime,Email,Mobile,RoleID,TokenExpireTime(这个是配置小时）
	启用后，可以使用AuthHelper里的功能进行注册，登陆等功能，默认获取token的地址在：/auth/gettoken?uid=xxx&pwd=xxx 
	-->
    <add key="Taurus.Auth" value="{TableName:Users,TokenExpireTime:24}"/>
  </appSettings>
  <system.web>
    <httpModules>
      <!--Taurus IIS应用程序池：经典模式（下运行，开启此配置，反之，注释掉此行）-->
			<add name="Taurus.Core" type="Taurus.Core.UrlRewrite,Taurus.Core" />
    </httpModules>
  </system.web>
  <system.webServer>
    <modules>
      <!--Taurus IIS应用程序池：集成模式（下运行，开启此配置，反之，注释掉此行）-->
      <add name="Taurus.Core" type="Taurus.Core.UrlRewrite,Taurus.Core" />
    </modules>
  </system.webServer>
</configuration>

A：项目需要有对应的Controller项目，默认的配置的项目名称：Taurus.Controllers
B：根据运行模式注释掉Taurus.Core配置的其中一个。

2：DefaultController 全局可用事件：
------------------------------------------------------------------------------
 /// <summary>
        /// 用于登陆前的请求合法性验证，配合[Ack]属性
        /// </summary>
        public static bool CheckAck(IController controller, string methodName)
        {
            //需要自己实现Ack验证
            return controller.CheckFormat("ack Can't be Empty", "ack");

        }

        /// <summary>
        /// 用于需要登陆后的身份验证，配合[Token]属性
        /// </summary>
        public static bool CheckToken(IController controller, string methodName)
        {
            //需要自己实现，或者通过配置Taurus.Auth启动自带的验证（自带的注释掉此方法即可）。
            return controller.CheckFormat("token Can't be Empty", "token");
        }
        /// <summary>
        /// 全局【路由映射】
        /// </summary>
        public static string RouteMapInvoke(HttpRequest request)
        {
            if (request.Url.LocalPath.StartsWith("/api/") && RouteConfig.RouteMode == 2)
            {
                return "/test" + request.RawUrl;
            }
            return string.Empty;
        }
        /// <summary>
        /// 全局【方法执行前拦截】
        /// </summary>
        public static bool BeforeInvoke(IController controller, string methodName)
        {
            //MAction action = new MAction("Test1", "server=.;database=demo;uid=sa;pwd=123456");

            //action.BeginTransation();
            //action.Set("name", "google");
            //if (action.Insert())
            //{
            //    throw new Exception("aa");
            //}

            //if (controller.IsHttpPost)
            //{
            //    //拦截全局处理
            //    controller.Write(methodName + " NoACK");
            //}

            return true;
        }
        /// <summary>
        /// 全局【方法执行后业务】
        /// </summary>
        public static void EndInvoke(IController controller, string methodName)
        {

        }
