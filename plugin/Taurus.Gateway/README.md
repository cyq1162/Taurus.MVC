# Taurus.Gateway is a microservice gateway for asp.net or asp.net core

<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus.Gateway 微服务网关，使用 .Net Core 示例：
<h4>1、以 Docker 中拉取镜像运行：</h4>
<p>下载镜像，当前【2024-01-23 发布的版本是3.3.0.1】：</p>
<pre><code>docker pull registry.cn-hangzhou.aliyuncs.com/taurus-netcore/taurus.gateway:3.3.0.1</code></pre>
<p>下载完成后，查看已下载的镜像列表：</p>
<pre><code>docker images</code></pre>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123174039876-329377314.png" alt="" loading="lazy" /></p>
<p>运行容器，网关需要对外，映射使用80和443端口【默认镜像中的应用打包运行的监听端口为：8080 和 443】：</p>
<p>下面命令行将[ImageID]换成下载的镜像ID</p>
<pre><code>docker run -d -p 80:8080 -p 443:443 --name=gateway --security-opt seccomp=unconfined [ImageID]</code></pre>
<p>启动后，通过以下命令查看容器是否正常运行：</p>
<pre><code>docker ps -a </code></pre>

<h4>2、以工程文件运行：进入 src 目录下，运行工程文件：</h4>
<p>默认运行环境是当前最新的 .net core 8 版本，程序主要依赖 Taurus.Mvc 的 Nuget 包。</p>
<p>可以根据需要自行创建工程文件，引用 Taurus.Mvc 的 Nuget 包即可，该包支持.net 和 .net core 几乎所有版本。</p>

<h4>3、程序启动正常时，先以 http 访问：</h4>
<p>默认主页显示为：404，没有可显示页面，需要手动输入以下网址进入后台：/admin/login</p>
<p>默认账号：admin，密码空，直接点登陆即可进入后台。</p>
<p><img src="https://img2023.cnblogs.com/blog/17408/202306/17408-20230606151806482-635586726.png" alt="" loading="lazy" class="medium-zoom-image"></p>

<h4>4、配置注册中心Url，以获取配置信息：</h4>
<p>配置项为：MicroService.Server.RcUrl，界面配置始下：</p>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123173032033-311040766.png" alt="" loading="lazy" /></p>
<p></p>

<h4>配置成功后，可以查看获取到的配置信息：</h4>
<p><img src="https://img2023.cnblogs.com/blog/17408/202306/17408-20230606152004338-2079864972.png" alt="" loading="lazy" class="medium-zoom-image"></p>
<p></p>

<h4>5、配置证书，以开启 https SSL 访问。</h4>
<p>在/admin/config keystrel 配置项中，直接上传证书文件：</p>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123180941315-2011240626.png" alt="" loading="lazy" /></p>
<p>zip 可包含一个或多个证书内容如下，如xxx.zip 包含：</p>
<pre><code>
证书文件：www.samples.com.pfx
密码文件：www.samples.com.txt
</code></pre>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123181158459-2018499637.png" alt="" loading="lazy" /></p>
<p>上传成功后，首次需要重启程序，以启动https监听。</p>
<h4>6、Taurus.RegistryCenter：微服务注册中心（配套系列）</h4>
<p>网关使用地址：https://github.com/cyq1162/Taurus.RegistryCenter</p>
<h4>7、Taurus 微服务客户端（配套系列）</h4>
<p>见：/demo 运行示例。</p>
<p></p>
<h4>7、更多教程地址，见作者博客</h4>
<p>https://www.cnblogs.com/cyq1162/category/2205668.html</p>
