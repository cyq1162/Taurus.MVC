# Taurus microservice run demo
<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus Example of microservices framework operation.


# Run Test Case 1: 【Registry Center + Client = Load Balancing】

<p>1、Run Registry Center：Taurus.RegistryCenter.exe （listen on port during startup : 8000）</p>
<p>2、Run WebAPI：Taurus.Client.Web.exe （Open multiple, listen to random ports during startup）</p>
<p>Open the browser and access : http://localhost:8000/api/hello （Multiple refreshes show that the request has been load balanced）</p>
<p>1st request</p>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123210621017-517353383.png" alt="" loading="lazy" /></p>
<p>Second request</p>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123210740896-428229390.png" alt="" loading="lazy" /></p>
<p></p>

# Run Test Case 2 【Registry Center + Client + Gateway = Load Balancing】
<p>While running test case 1, continue to run:</p>
<p>3、Run gateway : Taurus.Gateway.exe （listen on port during startup : 9000）</p>
<p>Open the browser and access : http://localhost:9000/api/hello （Multiple refreshes show that the request has been load balanced）</p>
<p>The effect is similar to Test 1, which simply transfers the access object from the registry to the gateway.</p>
<p>The benefits of adding gateways can be achieved by implementing multiple gateways in the future, using DNS to resolve multiple IPs and achieve load balancing of gateways.</p>


# Run Test Case 3 【Registry Center + Client + Gateway + Registry Center Of Slave = Load Balancing】
<p>While running test case 2, continue to run:</p>
<p>4、Run registry center of slave：Taurus.RegistryCenter.Slave.exe （listen on port during startup : 7000）
<p>Open the browser and access : http://localhost:7000/api/hello （Multiple refreshes show that the request has been load balanced）<p>
<p>The role of a registry (slave) is to achieve a failure over when the main registry goes down.</p>
<p>By performing the following operations, the effect can be observed</p>
<p>A. Close (primary) registry - observe that all requests are transferred to (secondary) without affecting gateway access.</p>
<p>B. Restart the (primary) registry - observe that all requests are restored and transferred to (primary) without affecting gateway access.</p>

