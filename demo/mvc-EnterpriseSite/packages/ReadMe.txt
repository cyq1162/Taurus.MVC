How to Use:
1��Add Reference cyq.data.dll Taurus.Core.dll
2��web.config add

<!-- I need to Route Url -->
<modules>
      <add name="Taurus.Core" type="Taurus.Core.UrlRewrite,Taurus.Core" />
</modules>

<!-- I need to Kown where's the Controllers -->
<appSettings>
	<add key="Taurus.Controllers" value="Your.Controllers" />
</appSettings>

3��Learn more ��http://taurus.cyqdata.com
4��Download source ��https://github.com/cyq1162/Taurus.MVC.git