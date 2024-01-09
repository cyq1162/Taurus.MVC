# Taurus.MVC.MicroService.Demo
Taurus.MVC 微服务框架示例Demo。

RunExe 目录下是可运行程序（编绎在.NET6）

运行测试案例1：

1、运行注册中心：MicroService-RegCenter.exe （单开，监听8000端口）

2、运行WebAPI：MicroService-Web.exe （可重复运行多次）

打开浏览器，访问：http://localhost:8000 （多次刷新可以看到请求被负载均衡了）



运行测试案例2：

3、运行网关：MicroService-Gateway.exe （默认可以多开，这里运行1次就好，控制台可以查看监听的端口）

打开浏览器，访问：http://localhost:那个监听的端口号 （多次刷新可以看到请求被负载均衡了）


运行测试案例3：

4、运行注册中心（从）：MicroService-RegCenterOfSlave.exe （单开，默认7000端口）

打开浏览器，访问：http://localhost:7000 （发现它也是可兼网关功能的）

A、关闭（主）注册中心  - 观察请求都转移到（从），同时不影响网关的访问。

B、重新启动（主）注册中心 - 观察请求都恢复转移到（主），同时不影响网关的访问。

