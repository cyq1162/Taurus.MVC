1：官网示例 ：http://taurus.cyqdata.com
2：开源地址 ：https://github.com/cyq1162/Taurus.MVC
3：CYQ.Data 开源地址: https://github.com/cyq1162/cyqdata

使用注意事项：
1：配置文件说明
<configuration>
  <appSettings>
    <!--指定控制器所在的项目（Dll）名称-->
    <add key="Taurus.Controllers" value="Taurus.Controllers" />
    <!--指定处理的后缀（默认无后缀，可配置.shtml）-->
    <add key="Taurus.Suffix" value=""/>
	 <!--是否允许跨域请求，默认true-->
    <add key="IsAllowCORS" value="true"/>
    <!--路由模式【值为0,1或2】[默认为1]
      值为0：匹配{Action}/{Para}
      值为1：匹配{Controller}/{Action}/{Para}
      值为2：匹配{Module}/{Controller}/{Action}/{Para}-->
    <add key="RouteMode" value="1"/>
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