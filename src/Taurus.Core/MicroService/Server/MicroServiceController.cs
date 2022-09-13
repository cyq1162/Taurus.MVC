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
    internal class MicroServiceController : Controller
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
        public override void EndInvoke()
        {
            if (string.IsNullOrEmpty(MicroService.MSConfig.AppRunUrl))
            {
                Uri uri = Context.Request.Url;
                string urlAbs = uri.AbsoluteUri;
                string urlPath = uri.PathAndQuery;
                string host = urlAbs.Substring(0, urlAbs.Length - urlPath.Length);
                MicroService.MSConfig.AppRunUrl = host;
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
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + string.Format(" : Server.API.Call.Reg : Host : {0} Name : {1}", host, name));
            if (!MicroService.MSConfig.IsRegCenter)
            {
                string tip = "MicroService.Reg : This is not RegCenter";
                MSLog.Write(tip, Convert.ToString(Request.UrlReferrer), "POST", MicroService.MSConfig.ServerName);
                Write(tip, false);
                return;//仅服务类型为注册中心，才允许接收注册。
            }
            #region 注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (MicroService.Server.RegCenterIsLive && !MicroService.MSConfig.IsRegCenterOfMaster)
            {
                Write(JsonHelper.OutResult(true, "", "tick", MicroService.Server.Tick, "host", MicroService.MSConfig.ServerRegUrl));
                return;
            }
            #endregion
            var kvTable = MicroService.Server.HostList;

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
                    MicroService.Server.IsChange = true;
                    List<MicroService.HostInfo> list = new List<MicroService.HostInfo>();
                    MicroService.HostInfo info = new MicroService.HostInfo();
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
                    List<MicroService.HostInfo> list = kvTable[module];//ms,a.com
                    StringBuilder sb2 = new StringBuilder();
                    for (int i = 0; i < list.Count; i++)
                    {
                        MicroService.HostInfo info = list[i];
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
                        MicroService.Server.IsChange = true;
                        MicroService.HostInfo info = new MicroService.HostInfo();
                        info.Host = host;
                        info.RegTime = DateTime.Now;
                        info.Version = ver;
                        list.Add(info);
                    }
                }
            }

            #endregion
            if (MicroService.Server.Tick == 0)
            {
                MicroService.Server.Tick = DateTime.Now.Ticks;
            }
            if (MicroService.Server.Host2LastRegTime < DateTime.Now.AddSeconds(-15))//超过15秒，备份链接无效化。
            {
                MicroService.Server.Host2 = String.Empty;
            }
            string result = JsonHelper.OutResult(sb.Length == 0, sb.ToString(), "tick", MicroService.Server.Tick, "host2", MicroService.Server.Host2);
            Write(result);

        }

        /// <summary>
        /// 注册中心 - 获取服务列表。
        /// </summary>
        /// <param name="tick">最后获取的时间Tick，首次请求可传0</param>
        [HttpGet]
        [MicroService]
        public void GetList(long tick)
        {
            WriteLine(Environment.NewLine + "--------------------------------------");
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.API.Call.GetList : From :" + Request.UrlReferrer);
            if (MicroService.Server.Host2LastRegTime < DateTime.Now.AddSeconds(-15))//超过15秒，备份链接无效化。
            {
                MicroService.Server.Host2 = String.Empty;
            }
            string host = MicroService.Server.RegCenterIsLive ? MicroService.MSConfig.ServerRegUrl : "";//注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (host == MicroService.MSConfig.AppRunUrl)//主机即是自己。
            {
                host = string.Empty;
            }
            if (MicroService.Server.HostList.Count == 0 || tick == MicroService.Server.Tick)
            {
                string result = JsonHelper.OutResult(true, "", "tick", MicroService.Server.Tick, "host2", MicroService.Server.Host2, "host", host);
                Write(result);
            }
            else
            {
                string json = MicroService.Server.HostListJson;
                string result = JsonHelper.OutResult(true, json, "tick", MicroService.Server.Tick, "host2", MicroService.Server.Host2, "host", host);
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
            WriteLine(DateTime.Now.ToString("HH:mm:ss") + " : Server.API.Call.Reg2 : Host :" + host);
            MicroService.Server.Host2 = host;
            MicroService.Server.Host2LastRegTime = DateTime.Now;
            string result = JsonHelper.OutResult(true, "", "tick", MicroService.Server.Tick);
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
            if (tick > MicroService.Server.Tick)
            {
                MicroService.Server.Tick = tick;
                MicroService.Server._HostList = JsonHelper.ToEntity<MDictionary<string, List<MicroService.HostInfo>>>(json);
            }
            Write("", true);
        }
    }
}
