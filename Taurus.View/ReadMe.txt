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
3：Controller增强Write方法 (2016-11-30)
4：Controller增加GetEntity<T>()方法 (2016-11-30)