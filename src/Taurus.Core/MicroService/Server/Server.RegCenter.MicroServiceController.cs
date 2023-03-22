using System;
using System.Text;
using CYQ.Data.Tool;
using Taurus.Mvc;
using System.Collections.Generic;
using Taurus.Mvc.Attr;

namespace Taurus.MicroService
{
    /// <summary>
    /// 微服务 - 注册中心。
    /// </summary>
    internal partial class MicroServiceController : Controller
    {

        private void WriteLine(string msg)
        {
#if DEBUG
            Console.WriteLine(msg);
#endif
        }

        public override bool CheckMicroService(string msKey)
        {
            return base.CheckMicroService(msKey);
        }

        public override bool BeforeInvoke()
        {
            switch (MethodName)
            {
                case "exit":
                    return true;
                default:
                    if (!MsConfig.IsRegCenter)
                    {
                        Write("Current Is Not Run As Register Center.", false);
                    }
                    return MsConfig.IsRegCenter;
            }
        }

        public override void EndInvoke()
        {
            if (string.IsNullOrEmpty(MsConfig.AppRunUrl))
            {
                Uri uri = Context.Request.Url;
                string urlAbs = uri.AbsoluteUri;
                string urlPath = uri.PathAndQuery;
                string host = urlAbs.Substring(0, urlAbs.Length - urlPath.Length);
                MsConfig.AppRunUrl = host;
            }
        }
        /// <summary>
        /// 注册中心 - 注册服务。
        /// </summary>
        /// <param name="name">服务名称，多个用逗号分隔，【可绑定域名】【模块追加版本号|号分隔。】</param>
        /// <param name="host">服务的可访问地址</param>
        /// <param name="version">服务的版本号【用于版本升级】</param>
        [HttpPost]
        [MicroService]
        [Require("name,host")]
        public void Reg(string name, string host, int version)
        {
            WriteLine(Environment.NewLine);
            WriteLine("--------------------------------------");
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : Reg Host From : {0} Name : {1}", host, name));

            #region 注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (Server.RegCenterIsLive && !MsConfig.IsRegCenterOfMaster)
            {
                Write(JsonHelper.OutResult(true, "", "tick", Server.Tick, "host", MsConfig.ServerRcUrl));
                return;
            }
            #endregion
            var kvTable = Server.RegCenter.HostList;

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
            string[] names = name.ToLower().Split(',');//允许一次注册多个模块。
            foreach (string item in names)
            {
                string[] items = item.Split('|');//允许模块域名带优先级版本号
                int ver = 0;
                if (items.Length < 2 || !int.TryParse(items[1], out ver))
                {
                    ver = version;
                }
                string module = items[0];
                if (!kvTable.ContainsKey(module))
                {
                    //首次添加
                    Server.IsChange = true;
                    List<HostInfo> list = new List<HostInfo>();
                    HostInfo info = new HostInfo();
                    info.Host = host;
                    info.RegTime = DateTime.Now;
                    info.Version = ver;
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
                        info.Host = host;
                        info.RegTime = DateTime.Now;
                        info.Version = ver;
                        list.Add(info);
                    }
                }
            }

            #endregion
            if (Server.Tick == 0)
            {
                Server.Tick = DateTime.Now.Ticks;
            }
            if (Server.Host2LastRegTime < DateTime.Now.AddSeconds(-15))//超过15秒，备份链接无效化。
            {
                Server.Host2 = String.Empty;
            }
            string result = JsonHelper.OutResult(sb.Length == 0, sb.ToString(), "tick", Server.Tick, "host2", Server.Host2);
            Write(result);
        }

        /// <summary>
        /// 注册中心 - 获取服务列表。
        /// </summary>
        /// <param name="tick">最后获取的时间Tick，首次请求可传0</param>
        /// <param name="isGateway">是否网关请求</param>
        [HttpGet]
        [MicroService]
        public void GetList(long tick, bool isGateway)
        {
            WriteLine(Environment.NewLine + "--------------------------------------");
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : GetList : From :" + Request.UrlReferrer);
            if (Server.Host2LastRegTime < DateTime.Now.AddSeconds(-15))//超过15秒，备份链接无效化。
            {
                Server.Host2 = String.Empty;
            }
            string host = Server.RegCenterIsLive ? MsConfig.ServerRcUrl : "";//注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (host == MsConfig.AppRunUrl)//主机即是自己。
            {
                host = string.Empty;
            }
            if (isGateway && Request.UrlReferrer != null)
            {
                Server.RegCenter.AddHost("Gateway", Request.UrlReferrer.OriginalString);
            }
            if (Server.RegCenter.HostList.Count == 0 || tick == Server.Tick)
            {
                string result = JsonHelper.OutResult(true, "", "tick", Server.Tick, "host2", Server.Host2, "host", host);
                Write(result);
            }
            else
            {
                string result = JsonHelper.OutResult(true, Server.RegCenter.HostListJson, "tick", Server.Tick, "host2", Server.Host2, "host", host);
                Write(result);
            }
        }

        /// <summary>
        /// 注册中心 - 设置【从】的备用地址。
        /// </summary>
        /// <param name="host">地址</param>
        [HttpPost]
        [MicroService]
        [Require("host")]
        public void Reg2(string host)
        {
            WriteLine(Environment.NewLine + "--------------------------------------");
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Reg Host2 From :" + host);
            Server.RegCenter.AddHost("RegCenterOfSlave", host);
            Server.Host2 = host;
            Server.Host2LastRegTime = DateTime.Now;
            string result = JsonHelper.OutResult(true, "", "tick", Server.Tick);
            Write(result);
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
            WriteLine(Environment.NewLine + "--------------------------------------");
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.API.Call.SyncList : Tick :" + tick);
            if (tick > Server.Tick)
            {
                Server.Tick = tick;
                Server.RegCenter.HostListJson = json;
                Server.RegCenter.HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(json);
            }
            Write("", true);
        }

    }
}
