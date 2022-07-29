using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
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
        public void Reg(string name, string host, int version)
        {
            if (MicroService.Config.ServerName.ToLower() != MicroService.Const.RegCenter)
            {
                return;//仅服务类型为注册中心，才允许接收注册。
            }
            #region 注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (MicroService.Server.IsLive && MicroService.Config.ServerHost != MicroService.Config.ClientHost)
            {
                Write(JsonHelper.OutResult(true, "", "tick", MicroService.Server.Tick, "host", MicroService.Config.ServerHost));
                return;
            }
            #endregion
            if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(host))
            {
                Write("Name and Host can't be empty", false);
                return;
            }
            MDataTable MSTable = MicroService.Server.Table;
            bool isChange = false;

            #region 注册名字
            string[] names = name.ToLower().Split(',');//允许一次注册多个模块。
            foreach (string module in names)
            {
                MDataTable table = MSTable.FindAll("name='" + module + "'");
                if (table == null || table.Rows.Count == 0)
                {
                    //首次添加
                    isChange = true;
                    MSTable.NewRow(true).Sets(0, module, host, version, DateTime.Now);
                }
                else
                {
                    //移除低版本号的服务。
                    MDataRowCollection rows = table.FindAll("version<" + version);
                    if (rows != null && rows.Count > 0)
                    {
                        foreach (var item in rows)
                        {
                            table.Rows.Remove(item);
                            MSTable.Rows.Remove(item);
                            isChange = true;
                        }
                    }
                    //新版本添加
                    MDataRow row = table.FindRow("host='" + host + "'");
                    if (row == null)
                    {
                        isChange = true;
                        MSTable.NewRow(true).Sets(0, module, host, version, DateTime.Now);
                    }
                    else
                    {
                        row.Set("time", DateTime.Now);//更新时间。
                    }
                }
            }
            #endregion
            if (isChange || MicroService.Server.Tick == 0)
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
            string host = MicroService.Server.IsLive ? MicroService.Config.ServerHost : "";//注册中心【从】检测到【主】恢复后，推送host，让后续的请求转回【主】
            if (host == MicroService.Config.ClientHost)//主机即是自己。
            {
                host = string.Empty;
            }
            if (MicroService.Server.Table.Rows.Count == 0 || tick == MicroService.Server.Tick)
            {
                string result = JsonHelper.OutResult(true, "", "tick", MicroService.Server.Tick, "host2", MicroService.Server.Host2, "host", host);
                Write(result);
            }
            else
            {
                string json = MicroService.Server.Table.ToJson(false, true);
                MicroService.IO.Write(MicroService.Const.ServerTablePath, json);
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
        public void Reg2(string host)
        {
            if (string.IsNullOrEmpty(host))
            {
                Write("Host can't be null", false);
                return;
            }

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
        public void SyncList(string json, long tick)
        {
            if (!string.IsNullOrEmpty(json) && tick > MicroService.Server.Tick)
            {
                MicroService.Server.Tick = tick;
                MicroService.Server._Table = MDataTable.CreateFrom(json);
            }
            Write("", true);
        }
    }
}
