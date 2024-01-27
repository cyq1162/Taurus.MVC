# Taurus.RegistryCenter is a microservice registry center for asp.net or asp.net core.
<h3>【 <a href='./README.md'>中文</a> | <a href='./README_en.md'>English</a>】</h3>
<hr />
# Taurus.RegistryCenter Micro service registration center, using Net Core Example：
<h4>1、To pull an image from Docker and run it:</h4>
<p>Download the image. The current version released on January 23, 2024 is 3.3.0.1:</p>
<pre><code>docker pull registry.cn-hangzhou.aliyuncs.com/taurus-netcore/taurus.registrycenter:3.3.0.1</code></pre>
<p>After downloading, check the list of downloaded images:</p>
<pre><code>docker images</code></pre>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123174039876-329377314.png" alt="" loading="lazy" /></p>
<p>Run the container, use port 8080 for internal network mapping, and do not enable port 443. [The default listening ports for application packaging and running in the image are: 8080 and 443]:</p>
<p>The following command line replaces [ImageID] with the downloaded image ID</p>
<pre><code>docker run -d -p 8080:8080  --name=rc --security-opt seccomp=unconfined [ImageID]</code></pre>
<p>After startup, check if the container is running properly by using the following command:</p>
<pre><code>docker ps -a </code></pre>

<h4>2、Run as engineering file: Enter the src directory and run the engineering file:</h4>
<p>The default runtime environment is the latest. net core 8 version, and the program mainly relies on Taurus The Nuget package for MVC.</p>
<p>You can also create engineering files according to the conditions and reference Taurus The Nuget package for MVC is sufficient, which supports almost all versions of. net and. net core.</p>

<h4>3、When the program starts normally:</h4>
<p>The default homepage is displayed as 404, and there are no pages to display. You need to manually enter the following website address to enter the background:/admin/login</p>
<p>Default account: admin. If the password is blank, simply click login to enter the backend.</p>
<p><img src="https://img2023.cnblogs.com/blog/17408/202306/17408-20230606151806482-635586726.png" alt="" loading="lazy" class="medium-zoom-image"></p>

<h4>4、Other instructions:</h4>
<p>After the program starts, the registration center runs successfully without any additional configuration. Wait for the microservices gateway, registration center (from), or client link to complete.</p>
<p>The program's own access URL needs to ensure that other microservices can access it.</p>
<p>You can view the clients on the connection in/admin/index in the future.</p>
<p><img src="https://img2023.cnblogs.com/blog/17408/202306/17408-20230606152004338-2079864972.png" alt="" loading="lazy" class="medium-zoom-image"></p>

<h4>5、Registration center (from) enable (optional):</h4>
<p>To ensure high availability of the registration center, you can choose to start the registration center simultaneously (from).</p>
<p>The steps to start the registration center (from) are the same as starting the registration center. Only after starting, you need to enter the management background and configure a link to the registration center</p>
<p>The configuration item is: MicroService Server RcUrl, interface configuration starts with:</p>
<p><img src="https://img2024.cnblogs.com/blog/17408/202401/17408-20240123173032033-311040766.png" alt="" loading="lazy" /></p>
<p></p>
<h4>6、Taurus.Gateway: Microservices Gateway (Supporting Series)</h4>
<p>Gateway usage address:https://github.com/cyq1162/Taurus.Gateway</p>
<h4>7、Taurus Microservice client (supporting series)</h4>
<p>见：/demo Run an example.</p>
<p></p>
<h4>7、For more tutorial addresses, please refer to the author's blog</h4>
<p>https://www.cnblogs.com/cyq1162/category/2205668.html</p>
