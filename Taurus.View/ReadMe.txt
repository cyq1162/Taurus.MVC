1：官网示例 ：http://taurus.cyqdata.com
2：开源地址 ：https://github.com/cyq1162/Taurus.MVC
3：CYQ.Data 开源地址: https://github.com/cyq1162/cyqdata

使用注意事项：
1：配置文件说明
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

更新日志：
1：升级CYQ.Data 版本。
2：Controller取消ajaxResult属性，增加Write方法用于输出数据。
V2.1.1.1
1：增加Token验证属性：在DefautController中可以定义并实现Token验证：（2016-11-16）（public static bool CheckToken(IController controller, string methodName){}）
2：Controller增强Write方法 (2016-11-30)
3：Controller增加GetEntity<T>()方法 (2016-11-30)
V2.2.0.0
1：Controller增加GetJson()方法 (2016-12-07)
2：增加跨域支持(2016-12-07)
3：增强GetEntity<T>()方法(2016-12-07)

V2.2.2.2
1：升级CYQ.Data(2017-02-04)
2：增加Session支持(2017-02-04)

V2.2.2.5 (2017-02-28)
1：升级CYQ.Data
2：优化3种情况：Session、跨域、编码 可能在某些情况引发异常或乱码


V2.2.2.6 (2017-03-27)
1：每个Controller都可以优先处理CheckToken，若不存在，则才处理DefautController中的CheckToken全局方法

V2.2.2.7 (2017-04-01)
1：小细节优化，对于没有引用（itemref)的移除节点（原来只是去除属性）,通过此小细节（这样在master.html中就可以事先放置多个引用）(2017-04-01)
2：修正LogicBase的参数缺少的问题。(2017-04-17)
3：DefautController增加全局BeforeInvoke方法，用于拦截做全局处理。(2017-04-17)
public static bool BeforeInvoke(IController controller, string methodName)
4：增加HttpGet和HttpPost特性(2017-04-17)

V2.2.2.8 (2017-04-18)
1：Query方法增加重载方法（方便取得Para中的值）(2017-04-18)
2：增加DefaultUrl配置项，设置默认起始访问路径。(2017-04-29)

V2.2.3.1(2017-05-15)
1：增加CheckFormat方法【支持参数为空或正则验证】

V2.2.3.3(2017-06-16)
1：增加方法参数的支持(兼容常规webapi的使用方法)
2：CYQ.Data同时升级到V5.7.7.4

V2.2.3.4(2017-07-05,2017-10-22)
1 :增加跨域参数
2：修正Query<T>(aaa,defaultValue)的默认取的取值顺序问题。
3：增加EndInvode事件和BenginInvode的事件执行顺序调整。
4：CYQ.Data同时升级到V5.7.8.3

V2.2.3.5(2017-04-19)
1：支持Controller分布在不同的dll中（Taurus.Controllers配置允许多个，逗号分隔）。
2：支持Controller二次继承（A：B   B：Taurus.Core.Controller）

V2.2.3.9(2019-03-14)
1：支持NetCore下的的部署（路径和大小写调整）
2、CYQ.Data同时升级到V5.7.9.4

V2.3(2019-03-21)
1、增加了CMS功能的标签替换功能。
2、增加参数验证属性（Require），验证是否必填写和正则格式。
3、增强了参数的类型转换。
4、增加WebAPI文档生成功能。
5、CYQ.Data同时升级到V5.7.9.7

V2.3.1(2019-03-21)
1、增强CMS标签替换功能（CYQ.Data里升级）。
2、支持WebAPI文档在路由模式2的显示。
3、内部优化。
4、CYQ.Data升级到V5.8(支持分布式缓存的高可用及性能，可以动态添加或减少节点而不影响程序。)

V2.3.3.1(2019-07-30)
1、CYQ.Data升级到V5.8.3.5
2、增强WebAPI文档生成功能
3、增强了参数的类型转换
4、增加[Ack]属性（机制和Token一样）