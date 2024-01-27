# Taurus.Gateway is a microservice gateway for asp.net or asp.net core

<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus Gateway microservices gateway, using Net Core Example:
<h4>1、Pulling images from Docker to run：</h4>
<p>Download the image. The current version released on January 23, 2024 is 3.3.0.1：</p>
<pre><code>docker pull registry.cn-hangzhou.aliyuncs.com/taurus-netcore/taurus.gateway:3.3.0.1</code></pre>
<p>After downloading, check the list of downloaded images：</p>
<pre><code>docker images</code></pre>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123174039876-329377314.png" alt="" loading="lazy" /></p>
<p>To run the container, the gateway needs to be externally mapped using ports 80 and 443. [The default listening ports for packaging and running applications in the image are: 8080 and 443]：</p>
<p>The following command line replaces [ImageID] with the downloaded image ID</p>
<pre><code>docker run -d -p 80:8080 -p 443:443 --name=gateway --security-opt seccomp=unconfined [ImageID]</code></pre>
<p>After startup, check if the container is running properly by using the following command:</p>
<pre><code>docker ps -a </code></pre>

<h4>2、Run as an engineering file: Enter the src directory and run the engineering file：</h4>
<p>The default runtime environment is the latest. net core 8 version, and the program mainly relies on Taurus MVC's Nuget package.</p>
<p>You can create project files and reference Taurus as needed The Nuget package for MVC is sufficient, which supports almost all versions of. net and. net core.</p>

<h4>3、When the program starts normally, first access it through HTTP:</h4>
<p>The default homepage is displayed as 404, and there are no pages to display. You need to manually enter the following website address to enter the background:/admin/login</p>
<p>Default account: admin. If the password is blank, simply click login to enter the backend.</p>
<p><img src="https://img2023.cnblogs.com/blog/17408/202306/17408-20230606151806482-635586726.png" alt="" loading="lazy" class="medium-zoom-image"></p>

<h4>4、Configure the registry URL to obtain configuration information:</h4>
<p>The configuration item is: MicroService Server RcUrl, interface configuration starts with:</p>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123173032033-311040766.png" alt="" loading="lazy" /></p>
<p></p>

<h4>After successful configuration, you can view the obtained configuration information:</h4>
<p><img src="https://img2023.cnblogs.com/blog/17408/202306/17408-20230606152004338-2079864972.png" alt="" loading="lazy" class="medium-zoom-image"></p>
<p></p>

<h4>5、Configure the certificate to enable HTTPS SSL access.</h4>
<p>/admin/config keystrel In the configuration item, directly upload the certificate file:</p>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123180941315-2011240626.png" alt="" loading="lazy" /></p>
<p>zip It can contain one or more certificate contents as follows, such as xxx.zip containing:</p>
<pre><code>
Certificate file：www.samples.com.pfx
Password file:www.samples.com.txt
</code></pre>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123181158459-2018499637.png" alt="" loading="lazy" /></p>
<p>After successful upload, the program needs to be restarted for the first time to start HTTPS listening.</p>
<h4>6、Taurus.RegistryCenter: Microservice Registration Center (Supporting Series)</h4>
<p>Gateway usage address:https://github.com/cyq1162/Taurus.RegistryCenter</p>
<h4>7、Taurus Microservice client (supporting series)</h4>
<p>见：/demo Run an example.</p>
<p></p>
<h4>7、For more tutorial addresses, please refer to the author's blog</h4>
<p>https://www.cnblogs.com/cyq1162/category/2205668.html</p>
