1������ʾ�� ��http://taurus.cyqdata.com
2����Դ��ַ ��https://github.com/cyq1162/Taurus.MVC
3��CYQ.Data ��Դ��ַ: https://github.com/cyq1162/cyqdata

ʹ��ע�����
1�������ļ�˵��
------------------------------------------------------------------------------
<configuration>
  <appSettings>
    <!--����Ҫ�ĳɣ����������ڵ���Ŀ������dll���ƣ���������׺��-->
    <add key="Taurus.Controllers" value="��Ŀ������dll����" />
    <!--ָ������ĺ�׺��Ĭ���޺�׺��������.shtml��-->
    <add key="Taurus.Suffix" value=""/>
	 <!--�Ƿ������������Ĭ��true-->
    <add key="Taurus.IsAllowCORS" value="true"/>
    <!--·��ģʽ��ֵΪ0,1��2��[Ĭ��Ϊ1]
      ֵΪ0��ƥ��{Action}/{Para}
      ֵΪ1��ƥ��{Controller}/{Action}/{Para}
      ֵΪ2��ƥ��{Module}/{Controller}/{Action}/{Para}-->
    <add key="Taurus.RouteMode" value="1"/>
    <!--ָ��ҳ����ʼ����·��-->
    <add key="Taurus.DefaultUrl" value="home/index"/>
	 <!--�Ƿ���������API�ĵ�������·��Ϊ��/doc/default,��Ҫ����Դ����/Views/DocĿ¼�µ��ļ�-->
    <add key="Taurus.IsStartDoc" value="true"/>
	<!--�Ƿ�����Ĭ�ϵ�Token���ƣ������õ�ӳ���ֶΣ�TableName,UserName,Password(����������д�������ѡ��,
	FullName,Status,PasswordExpireTime,Email,Mobile,RoleID,TokenExpireTime(���������Сʱ��
	���ú󣬿���ʹ��AuthHelper��Ĺ��ܽ���ע�ᣬ��½�ȹ��ܣ�Ĭ�ϻ�ȡtoken�ĵ�ַ�ڣ�/auth/gettoken?uid=xxx&pwd=xxx 
	-->
    <add key="Taurus.Auth" value="{TableName:Users,TokenExpireTime:24}"/>
  </appSettings>
  <system.web>
    <httpModules>
      <!--Taurus IISӦ�ó���أ�����ģʽ�������У����������ã���֮��ע�͵����У�-->
			<add name="Taurus.Core" type="Taurus.Core.UrlRewrite,Taurus.Core" />
    </httpModules>
  </system.web>
  <system.webServer>
    <modules>
      <!--Taurus IISӦ�ó���أ�����ģʽ�������У����������ã���֮��ע�͵����У�-->
      <add name="Taurus.Core" type="Taurus.Core.UrlRewrite,Taurus.Core" />
    </modules>
  </system.webServer>
</configuration>

A����Ŀ��Ҫ�ж�Ӧ��Controller��Ŀ��Ĭ�ϵ����õ���Ŀ���ƣ�Taurus.Controllers
B����������ģʽע�͵�Taurus.Core���õ�����һ����

2��DefaultController ȫ�ֿ����¼���
------------------------------------------------------------------------------
 /// <summary>
        /// ���ڵ�½ǰ������Ϸ�����֤�����[Ack]����
        /// </summary>
        public static bool CheckAck(IController controller, string methodName)
        {
            //��Ҫ�Լ�ʵ��Ack��֤
            return controller.CheckFormat("ack Can't be Empty", "ack");

        }

        /// <summary>
        /// ������Ҫ��½��������֤�����[Token]����
        /// </summary>
        public static bool CheckToken(IController controller, string methodName)
        {
            //��Ҫ�Լ�ʵ�֣�����ͨ������Taurus.Auth�����Դ�����֤���Դ���ע�͵��˷������ɣ���
            return controller.CheckFormat("token Can't be Empty", "token");
        }
        /// <summary>
        /// ȫ�֡�·��ӳ�䡿
        /// </summary>
        public static string RouteMapInvoke(HttpRequest request)
        {
            if (request.Url.LocalPath.StartsWith("/api/") && RouteConfig.RouteMode == 2)
            {
                return "/test" + request.RawUrl;
            }
            return string.Empty;
        }
        /// <summary>
        /// ȫ�֡�����ִ��ǰ���ء�
        /// </summary>
        public static bool BeforeInvoke(IController controller, string methodName)
        {
            //MAction action = new MAction("Test1", "server=.;database=demo;uid=sa;pwd=123456");

            //action.BeginTransation();
            //action.Set("name", "google");
            //if (action.Insert())
            //{
            //    throw new Exception("aa");
            //}

            //if (controller.IsHttpPost)
            //{
            //    //����ȫ�ִ���
            //    controller.Write(methodName + " NoACK");
            //}

            return true;
        }
        /// <summary>
        /// ȫ�֡�����ִ�к�ҵ��
        /// </summary>
        public static void EndInvoke(IController controller, string methodName)
        {

        }
