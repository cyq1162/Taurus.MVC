using System;
using System.Text;
using CYQ.Data.Tool;
using Taurus.Mvc;
using System.Collections.Generic;
using Taurus.Mvc.Attr;
using Taurus.Plugin.Admin;
using Taurus.Plugin.Limit;
using CYQ.Data.Json;
using CYQ.Data;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 微服务 - 注册中心。
    /// </summary>
    internal partial class MicroServiceController : Controller
    {
        private void WriteLine(string msg)
        {
            if (AppConfig.IsDebugMode)
            {
                Console.WriteLine(msg);
            }
        }

        public override bool CheckMicroService(string msKey)
        {
            return base.CheckMicroService(msKey);
        }

        public override bool BeforeInvoke()
        {
            switch (MethodName)
            {
                case "stop"://client
                case "exit"://client
                    return true;
                default:
                    if (!MsConfig.IsRegistryCenter || !MsConfig.Server.IsEnable)
                    {
                        Write("Microservice (registry center) unavailable.", false);
                        return false;
                    }
                    //check ui login
                    return true;
            }
        }

        /// <summary>
        /// 注册中心 - 注册服务。
        /// </summary>
        /// <param name="name">服务名称，多个用逗号分隔，【可绑定域名】【模块追加版本号|号分隔。】</param>
        /// <param name="host">服务的可访问地址</param>
        /// <param name="version">服务的版本号【用于版本升级】</param>
        /// <param name="isVirtual">是否虚拟名称【名称路径不转发】</param>
        /// <param name="domain">绑定的域名</param>
        /// <param name="pid">进程ID</param>
        [HttpPost]
        [MicroService]
        [Require("name,host,domain")]
        public void Reg(string name, string host, string domain, int version, bool isVirtual, int pid)
        {
            host = host.Trim(' ', '/', '\r', '\n').ToLower();
            name = name.Trim() + "," + domain.Trim();
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : Receive From : {0} Port : {1} Name : {2}", host, Request.ServerVariables["REMOTE_PORT"], name));
            WriteLine("--------------------------------------");
            #region 注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (Server.IsLiveOfMasterRC)// && !MsConfig.IsRegCenterOfMaster
            {
                Write(JsonHelper.OutResult(true, "", "tick", Server.Tick, "host", MsConfig.Server.RcUrl, "host2", MvcConfig.RunUrl));
                return;
            }
            #endregion
            string hostIP = Request.UserHostAddress;
            var kvTable = Server.RegistryCenter.HostList;

            StringBuilder sb = new StringBuilder();

            #region 注册名字[版本号检测]
            if (name.Contains("."))//包含域名
            {
                bool hasModule = false;
                foreach (var item in name.Split(','))
                {
                    if (!item.Contains("."))
                    {
                        hasModule = true;
                        break;
                    }
                }
                if (!hasModule)
                {
                    name += ",*";//对于仅绑定域名的，追加通用模块。
                }
            }
            else
            {
                name += ",*.*";//未绑定域名的，添加通用域名模块。
            }

            string[] names = name.Trim(' ', '/', '\r', '\n').ToLower().Split(',');//允许一次注册多个模块。
            foreach (string item in names)
            {
                if (string.IsNullOrEmpty(item)) { continue; }
                string[] items = item.Split('|');//允许模块域名带优先级版本号，和是否虚拟属性
                int ver = version;
                bool vir = isVirtual;
                if (items.Length > 1)
                {
                    int.TryParse(items[1], out ver);
                }
                if (items.Length > 2)
                {
                    vir = items[2] == "1" || items[2] == "true";
                }
                string module = items[0];
                if (!kvTable.ContainsKey(module))
                {
                    //首次添加
                    Server.IsChange = true;
                    List<HostInfo> list = new List<HostInfo>();
                    HostInfo info = new HostInfo();
                    info.PID = pid;
                    info.HostIP = hostIP;
                    info.Host = host;
                    info.RegTime = DateTime.Now;
                    info.Version = ver;
                    info.IsVirtual = vir;
                    list.Add(info);
                    kvTable.Add(module, list);
                }
                else
                {
                    bool hasHost = false;
                    bool isRemove = false;
                    bool clearOne = false;
                    bool hasBiggerVersion = false;
                    List<HostInfo> list = kvTable[module];//ms,a.com
                    StringBuilder sb2 = new StringBuilder();
                    int count = list.Count;//1、先拿总数，不能在for中用list.Count
                    for (int i = 0; i < count; i++)
                    {
                        HostInfo info = list[i];//2、先拿总数，再循环，否则在并发下会拿到null值。
                        if (info.Version == -1)
                        {
                            if (info.Host == host)
                            {
                                isRemove = true;
                                sb2.Length = 0;//优先提示级别高
                                sb2.AppendFormat("【{0}】 wait to remove。", module);
                                break;
                            }
                            continue;
                        }
                        if (info.Host == host)
                        {
                            hasHost = true;
                            info.RegTime = DateTime.Now;//更新时间。
                            info.PID = pid;
                            info.HostIP = hostIP;
                        }
                        if (info.Version < ver)
                        {
                            if (!clearOne)
                            {
                                info.Version = -1;//标识为-1，由任务清除。
                                clearOne = true;//每次注册仅清除1个，用于平滑版本过渡版本升级。
                            }
                        }
                        else
                        {
                            if (info.Version > ver && !hasBiggerVersion)
                            {
                                hasBiggerVersion = true;
                                if (sb2.Length == 0)
                                {
                                    sb2.AppendFormat("Reg 【{0}】 fail:【Version : {1}<{2}】。", module, ver, info.Version);
                                }
                            }
                        }


                    }
                    if (hasHost)
                    {
                        if (isRemove)
                        {
                            sb.Append(sb2);
                        }
                    }
                    else if (hasBiggerVersion)//新添旧版本
                    {
                        sb.Append(sb2);//提示已有新版本。
                    }
                    else //新版本添加
                    {
                        Server.IsChange = true;
                        HostInfo info = new HostInfo();
                        info.PID = pid;
                        info.HostIP = hostIP;
                        info.Host = host;
                        info.RegTime = DateTime.Now;
                        info.Version = ver;
                        info.IsVirtual = vir;
                        list.Add(info);
                    }
                }
            }

            #endregion
            if (Server.Tick == 0)
            {
                Server.Tick = DateTime.Now.Ticks;
            }
            string result = JsonHelper.OutResult(sb.Length == 0, sb.ToString(), "tick", Server.Tick, "host2", Server.Host2, "configtick", Server.SyncConfigTime.Ticks);
            Write(result);
        }

        /// <summary>
        /// 注册中心 - 获取服务列表。
        /// </summary>
        /// <param name="tick">最后获取的时间Tick，首次请求可传0</param>
        /// <param name="isGateway">是否网关请求</param>
        /// <param name="pid">进程ID</param>
        [HttpGet]
        [MicroService]
        public void GetList(long tick, bool isGateway, int pid)
        {
            string host2 = Server.IsLiveOfMasterRC ? MvcConfig.RunUrl : Server.Host2;
            string host = Server.IsLiveOfMasterRC ? MsConfig.Server.RcUrl : "";//注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (host == MvcConfig.RunUrl)//主机即是自己。
            {
                host = string.Empty;
            }
            if (isGateway && Request.UrlReferrer != null)
            {
                Server.RegistryCenter.AddHost("Gateway", Request.UrlReferrer.OriginalString, pid, Request.UserHostAddress);
            }
            if (tick == Server.Tick || Server.RegistryCenter.HostList == null)
            {
                string result = JsonHelper.OutResult(true, "", "tick", Server.Tick, "host2", host2, "host", host, "iptick", Server.SyncIPTime.Ticks);
                Write(result);
            }
            else
            {
                string result = JsonHelper.OutResult(true, Server.RegistryCenter.HostListJson, "tick", Server.Tick, "host2", host2, "host", host, "iptick", Server.SyncIPTime.Ticks);
                Write(result);
            }

            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : GetList : From :" + Request.UrlReferrer + " Port : " + Request.ServerVariables["REMOTE_PORT"]);
            WriteLine("--------------------------------------");
        }

        /// <summary>
        /// 注册中心 - 设置【从】的备用地址。
        /// </summary>
        /// <param name="host">地址</param>
        /// <param name="pid">进程ID</param>
        [HttpPost]
        [MicroService]
        [Require("host")]
        public void Reg2(string host, int pid)
        {
            Server.RegistryCenter.AddHost("RegistryCenterOfSlave", host, pid, Request.UserHostAddress);
            Server.Host2 = host;
            string result = JsonHelper.OutResult(true, "", "tick", Server.Tick, "iptick", Server.SyncIPTime.Ticks);
            Write(result);

            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Reg Host2 :" + host + " Port : " + Request.ServerVariables["REMOTE_PORT"]);
            WriteLine("--------------------------------------");
        }

        /// <summary>
        /// 注册中心 - 同步数据【备用=》主机】。
        /// </summary>
        /// <param name="json">数据</param>
        /// <param name="tick">标识</param>
        [HttpPost]
        [MicroService]
        [Require("json")]
        public void SyncList(string json, long tick)
        {
            if (tick > Server.Tick)
            {
                Server.Tick = tick;
                Server.RegistryCenter.HostListJson = json;
                Server.RegistryCenter.HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
            }
            Write("", true);

            
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.API.Call.SyncList : Tick :" + tick);
            WriteLine("--------------------------------------");
        }

    }

    /// <summary>
    /// 扩展【和 Admin 插件协同】 - 同步 IP、配置
    /// </summary>
    internal partial class MicroServiceController
    {
        /// <summary>
        /// 注册中心 - 获取同步IP黑名单列表。
        /// </summary>
        [HttpGet]
        [MicroService]
        public void GetIPSyncList()
        {
            string ipList = IO.Read(AdminConst.IPSyncPath);
            Write(JsonHelper.OutResult(true, ipList));

            
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : GetIPList From :" + Request.UrlReferrer);
            WriteLine("--------------------------------------");
        }

        /// <summary>
        /// 注册中心 - 获取同步配置列表。
        /// </summary>
        [HttpGet]
        [MicroService]
        public void GetConfigSyncList()
        {
            string configList = IO.Read(AdminConst.ConfigSyncPath);
            Write(JsonHelper.OutResult(true, configList));

            
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : GetConfigList From :" + Request.UrlReferrer);
            WriteLine("--------------------------------------");
        }
    }
}
