How to Use:
1£ºAdd Reference cyq.data.dll Taurus.Core.dll
2£ºweb.config add

<!-- I need to Route Url -->
<modules>
      <add name="Taurus.Core" type="Taurus.Core.UrlRewrite,Taurus.Core" />
</modules>

<!-- I need to Kown where's the Controllers -->
<appSettings>
	<add key="Taurus.Controllers" value="Your.Controllers" />
</appSettings>

3£ºLearn more £ºhttp://taurus.cyqdata.com
4£ºDownload source £ºhttps://github.com/cyq1162/Taurus.MVC.git