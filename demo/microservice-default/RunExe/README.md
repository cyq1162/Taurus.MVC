# Taurus microservice run demo
<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus.MVC 微服务框架运行示例。


# 运行测试案例1：【注册中心 + 客户端 = 负载均衡】

<p>1、运行注册中心：Taurus.RegistryCenter.exe （单开，启动时监听端口：8000）</p>
<p>2、运行WebAPI：Taurus.Client.Web.exe （多开，启动时监听随机端口）</p>
<p>打开浏览器，访问：http://localhost:8000/api/hello （多次刷新可以看到请求被负载均衡了）</p>
<p>第1次请求</p>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123210621017-517353383.png" alt="" loading="lazy" /></p>
<p>第2次请求</p>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123210740896-428229390.png" alt="" loading="lazy" /></p>
<p></p>

# 运行测试案例2：【注册中心 + 客户端 + 网关 = 负载均衡】
<p>在运行测试案例1的情况下，继续运行：</p>
<p>3、运行网关：Taurus.Gateway.exe （允许多开，这里启动1次，启动监听端口：9000）</p>
<p>打开浏览器，直接访问网关：http://localhost:9000/api/hello （多次刷新可以看到请求被负载均衡了）</p>
<p>效果如同测试1，只是将访问对象从注册中心，转移到网关。</p>
<p>增加网关的好处，后续可以实现多网关，以dns解析多IP的方式，实现网关的负载均衡。</p>


# 运行测试案例3：【注册中心 + 客户端 + 网关 + 注册中心（从） = 负载均衡】
<p>在运行测试案例2的情况下，继续运行：</p>
<p>4、运行注册中心（从）：Taurus.RegistryCenter.Slave.exe （单开，启动时监听端口：7000）
<p>打开浏览器，访问：http://localhost:7000/api/hello （多次刷新可以看到请求被负载均衡了）<p>
<p>注册中心（从）的作用是：当主注册中心宕机后，实现故障转移。</p>
<p>通过以下操作，可以观察效果</p>
<p>A、关闭（主）注册中心  - 观察请求都转移到（从），同时不影响网关的访问。</p>
<p>B、重新启动（主）注册中心 - 观察请求都恢复转移到（主），同时不影响网关的访问。</p>

