using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using System.Collections.Generic;

namespace Taurus.Core
{
    /// <summary>
    /// 微服务 - 主程序 - 注册中心。
    /// </summary>
    public partial class MicroServiceController : Controller
    {
        /// <summary>
        /// 授权检测。
        /// </summary>
        /// <returns></returns>
        public override bool CheckMicroService()
        {
            return base.CheckMicroService();
        }
        /// <summary>
        /// 服务注册接口 - 注册中心（ServerName配置为：RegCenter）类型才允许被触发。
        /// </summary>
        [HttpPost]
        [MicroService]
        [Require("name,host")]
        public void Reg(string name, string host, int version)
        {
            if (!MicroService.Server.IsRegCenter)
            {
                MicroService.LogWrite("MicroService.Reg : This is not RegCenter", Convert.ToString(Request.UrlReferrer), "POST", MicroService.Config.ServerName);
                return;//仅服务类型为注册中心，才允许接收注册。
            }
            #region 注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (MicroService.Server.RegCenterIsLive && !MicroService.Server.IsRegCenterOfMaster)
            {
                Write(JsonHelper.OutResult(true, "", "tick", MicroService.Server.Tick, "host", MicroService.Config.ServerRegUrl));
                return;
            }
            #endregion
            var kvTable = MicroService.Server.HostList;

            #region 注册名字
            string[] names = name.ToLower().Split(',');//允许一次注册多个模块。
            foreach (string module in names)
            {
                if (!kvTable.ContainsKey(module))
                {
                    //首次添加
                    MicroService.Server.IsChange = true;
                    List<MicroService.HostInfo> list = new List<MicroService.HostInfo>();
                    MicroService.HostInfo info = new MicroService.HostInfo();
                    info.Host = host;
                    info.RegTime = DateTime.Now;
                    info.Version = version;
                    list.Add(info);
                    kvTable.Add(module, list);
                }
                else
                {
                    bool hasHost = false;
                    List<MicroService.HostInfo> list = kvTable[module];
                    for (int i = 0; i < list.Count; i++)
                    {
                        MicroService.HostInfo info = list[i];
                        if (info.Version < version)
                        {
                            info.Version = -1;//标识为-1，由任务清除。
                        }
                        else if (info.Host == host)
                        {
                            hasHost = true;
                            info.RegTime = DateTime.Now;//更新时间。
                        }
                    }
                    if (!hasHost) //新版本添加
                    {
                        MicroService.Server.IsChange = true;
                        MicroService.HostInfo info = new MicroService.HostInfo();
                        info.Host = host;
                        info.RegTime = DateTime.Now;
                        info.Version = version;
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
            string result = JsonHelper.OutResult(true, "", "tick", MicroService.Server.Tick, "host2", MicroService.Server.Host2);
            Write(result);
        }

        /// <summary>
        /// 获取微服务列表
        /// </summary>
        [HttpGet]
        [MicroService]
        public void GetList(long tick)
        {
            if (MicroService.Server.Host2LastRegTime < DateTime.Now.AddSeconds(-15))//超过15秒，备份链接无效化。
            {
                MicroService.Server.Host2 = String.Empty;
            }
            string host = MicroService.Server.RegCenterIsLive ? MicroService.Config.ServerRegUrl : "";//注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (host == MicroService.Config.RunUrl)//主机即是自己。
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
        /// 服务注册接口 - 注册中心 - 设置备用地址
        /// </summary>
        /// <param name="host">地址</param>
        [HttpPost]
        [MicroService]
        [Require("host")]
        public void Reg2(string host)
        {
            MicroService.Server.Host2 = host;
            MicroService.Server.Host2LastRegTime = DateTime.Now;
            string result = JsonHelper.OutResult(true, "", "tick", MicroService.Server.Tick);
            Write(result);
        }

        /// <summary>
        /// 服务注册接口 - 注册中心 - 同步数据【备用=》主机】。
        /// </summary>
        /// <param name="json">数据</param>
        /// <param name="tick">标识</param>
        [HttpPost]
        [MicroService]
        [Require("json")]
        public void SyncList(string json, long tick)
        {
            if (tick > MicroService.Server.Tick)
            {
                MicroService.Server.Tick = tick;
                MicroService.Server._HostList = JsonHelper.ToEntity<MDictionary<string, List<MicroService.HostInfo>>>(json);
            }
            Write("", true);
        }
    }
}
