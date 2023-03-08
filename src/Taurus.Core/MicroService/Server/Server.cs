using CYQ.Data.Table;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.MicroService
{
    /// <summary>
    /// 网关或注册中心端编码
    /// </summary>
    internal class Server
    {
        #region 对外开放接口或属性

        /// <summary>
        /// 注册中心备份或网关 - 检测注册中心是否安在。
        /// </summary>
        public static bool RegCenterIsLive = false;

        #endregion

        /// <summary>
        /// 作为注册中心时的最后更新标识.
        /// </summary>
        internal static long Tick = 0;
        /// <summary>
        /// 注册中心 - 数据是否发生改变
        /// </summary>
        internal static bool IsChange = false;
        internal static string _HostListJson = String.Empty;

        /// <summary>
        /// 注册中心 - 返回的表数据Json
        /// </summary>
        internal static string HostListJson
        {
            get
            {
                if (string.IsNullOrEmpty(_HostListJson) && IsChange && _HostList != null && _HostList.Count > 0)
                {
                    IsChange = false;
                    lock (MsConst.tableLockObj)
                    {
                        _HostListJson = JsonHelper.ToJson(HostList);
                    }
                    IO.Write(MsConst.ServerHostListJsonPath, _HostListJson);
                }
                return _HostListJson;
            }
            set
            {
                _HostListJson = value;
            }
        }
        internal static string _Host2 = null;
        /// <summary>
        /// 注册中心【存档】故障转移备用链接。
        /// </summary>
        internal static string Host2
        {
            get
            {
                if (_Host2 == null)
                {
                    _Host2 = IO.Read(MsConst.ServerHost2Path);//首次读取，以便于恢复。
                }
                return _Host2;
            }
            set
            {
                _Host2 = value;
            }
        }
        /// <summary>
        /// 备份链接最新注册时间
        /// </summary>
        internal static DateTime Host2LastRegTime = DateTime.MinValue;


        internal static MDictionary<string, List<HostInfo>> _HostList;
        /// <summary>
        /// 作为微服务主程序时，存档的微服务列表
        /// </summary>
        public static MDictionary<string, List<HostInfo>> HostList
        {
            get
            {
                if (_HostList == null)
                {
                    _HostListJson = IO.Read(MicroService.MsConst.ServerHostListJsonPath);
                    if (!string.IsNullOrEmpty(_HostListJson))//数据恢复。
                    {
                        #region 从Json文件恢复数据
                        MDictionary<string, List<HostInfo>> keys = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(_HostListJson);
                        if (keys != null && keys.Count > 0)
                        {
                            //恢复时间，避免被清除。
                            foreach (var item in keys)
                            {
                                if (item.Value != null)
                                {
                                    foreach (var ci in item.Value)
                                    {
                                        ci.RegTime = DateTime.Now;
                                    }
                                }
                            }
                        }
                        _HostList = keys;
                        #endregion
                    }
                    if (_HostList == null)
                    {
                        _HostList = new MDictionary<string, List<HostInfo>>(StringComparer.OrdinalIgnoreCase);
                    }
                }
                return _HostList;
            }
        }
        /// <summary>
        /// 获取模块所在的对应主机网址【若存在多个：每次获取都会循环下一个】。
        /// </summary>
        /// <param name="name">服务模块名称</param>
        /// <returns></returns>
        public static string GetHost(string name)
        {
            List<HostInfo> infoList = GetHostList(name);//微服务程序。
            if (infoList != null && infoList.Count > 0)
            {
                bool isRegCenter = MsConfig.IsRegCenterOfMaster;
                HostInfo firstInfo = infoList[0];
                for (int i = 0; i < infoList.Count; i++)
                {
                    int callIndex = firstInfo.CallIndex + i;
                    if (callIndex >= infoList.Count)
                    {
                        callIndex = 0;
                    }
                    HostInfo info = infoList[callIndex];

                    if (info.Version < 0 || (info.CallTime > DateTime.Now && infoList.Count > 0) || (isRegCenter && info.RegTime < DateTime.Now.AddSeconds(-10)))//正常5-10秒注册1次。
                    {
                        continue;//已经断开服务的。
                    }
                    firstInfo.CallIndex = callIndex + 1;//指向下一个。
                    return infoList[callIndex].Host;
                }
            }
            return string.Empty;
        }
        /// <summary>
        /// 获取模块的所有Host列表。
        /// </summary>
        /// <param name="name">服务模块名称</param>
        /// <returns></returns>
        public static List<HostInfo> GetHostList(string name)
        {
            if (!string.IsNullOrEmpty(name) && HostList != null)
            {
                List<HostInfo> list = new List<HostInfo>();
                if (HostList.ContainsKey(name))//微服务程序。
                {
                    list.AddRange(HostList[name]);
                }
                if (name.Contains("."))//域名
                {
                    if (list.Count == 0 && name.Split(".").Length > 2)//2级泛域名检测
                    {
                        string seName = "*" + name.Substring(name.IndexOf("."));
                        if (HostList.ContainsKey(seName))
                        {
                            list.AddRange(HostList[seName]);
                        }
                    }
                    if (name != "*.*" && HostList.ContainsKey("*.*"))
                    {
                        List<HostInfo> commList = HostList["*.*"];
                        if (commList.Count > 0)
                        {
                            if (list.Count == 0 || commList[0].Version >= list[0].Version)//版本号比较处理
                            {
                                list.AddRange(commList);//增加“*.*”模块的通用符号处理。
                            }
                        }
                    }
                }
                else //普通模块
                {
                    if (name != "*" && HostList.ContainsKey("*"))
                    {
                        List<HostInfo> commList = HostList["*"];
                        if (commList.Count > 0)
                        {
                            if (list.Count == 0 || commList[0].Version >= list[0].Version)//版本号比较处理
                            {
                                list.AddRange(commList);//增加“*”模块的通用符号处理。
                            }
                        }
                    }
                }
                return list;
            }
            return null;
        }

        /// <summary>
        /// 为Server端添加Host
        /// </summary>
        public static void AddHost(string name, string host)
        {
            if (string.IsNullOrEmpty(host)) { return; }
            var kvTable = Server.HostList;
            if (!kvTable.ContainsKey(name))
            {
                //首次添加
                Server.IsChange = true;
                List<HostInfo> list = new List<HostInfo>();
                HostInfo info = new HostInfo();
                info.Host = host;
                info.RegTime = DateTime.Now;
                list.Add(info);
                kvTable.Add(name, list);
            }
            else
            {
                List<HostInfo> list = kvTable[name];//ms,a.com
                for (int i = 0; i < list.Count; i++)
                {
                    HostInfo hostInfo = list[i];
                    if (hostInfo.Host == host)
                    {
                        hostInfo.RegTime = DateTime.Now;//更新时间。
                        return;
                    }
                }
                Server.IsChange = true;
                HostInfo info = new HostInfo();
                info.Host = host;
                info.RegTime = DateTime.Now;
                list.Add(info);
            }
        }

        internal static MDataTable GetHostTable()
        {
            MDictionary<string, List<HostInfo>> hostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(_HostListJson);
            return CreateTable(hostList);
        }

        internal static MDataTable CreateTable(MDictionary<string, List<HostInfo>> hostList)
        {
            MDataTable _MsTable = new MDataTable();
            _MsTable.TableName = MsConfig.MsTableName;
            _MsTable.Conn = MsConfig.MsConn;
            _MsTable.Columns.Add("MsID", System.Data.SqlDbType.Int, true);
            _MsTable.Columns.Add("MsName", System.Data.SqlDbType.NVarChar, false, false, 50);
            _MsTable.Columns.Add("Host", System.Data.SqlDbType.NVarChar, false, false, 250);
            _MsTable.Columns.Add("Version", System.Data.SqlDbType.Int);
            _MsTable.Columns.Add("LastActiveTime", System.Data.SqlDbType.DateTime);
            _MsTable.Columns.Add("CreateTime", System.Data.SqlDbType.DateTime, false, false, 0, false, CYQ.Data.SQL.SqlValue.GetDate);

            if (hostList != null && hostList.Count > 0 && !string.IsNullOrEmpty(MsConfig.MsConn))
            {
                foreach (KeyValuePair<string, List<HostInfo>> item in hostList)
                {
                    foreach (HostInfo host in item.Value)
                    {
                        _MsTable.NewRow(true).Sets(1, item.Key, host.Host, host.Version, host.RegTime);
                    }
                }
            }
            return _MsTable;

        }
    }
}
