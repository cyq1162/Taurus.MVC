using CYQ.Data.Table;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;

namespace Taurus.MicroService
{
    /// <summary>
    /// 网关或注册中心端编码
    /// </summary>
    internal partial class Server
    {
        /// <summary>
        /// 注册中心 - 专用
        /// </summary>
        public class RegCenter
        {
            private static string _HostListJson = null;

            private static bool _IsReadFromFile = false;
            /// <summary>
            /// 注册中心 - 返回的表数据Json
            /// </summary>
            internal static string HostListJson
            {
                get
                {
                    if (_HostListJson == null)
                    {
                        _IsReadFromFile = true;
                        _HostListJson = IO.Read(MsConst.ServerRegCenterJsonPath);
                    }
                    return _HostListJson;
                }
                set
                {
                    _IsReadFromFile = false;
                    _HostListJson = value;

                    if (string.IsNullOrEmpty(value))
                    {
                        IO.Delete(MsConst.ServerRegCenterJsonPath);
                    }
                    else
                    {
                        IO.Write(MsConst.ServerRegCenterJsonPath, _HostListJson);
                    }
                }
            }
            private static MDictionary<string, List<HostInfo>> _HostList;
            /// <summary>
            /// 作为注册中心 - 存档的微服务列表
            /// </summary>
            public static MDictionary<string, List<HostInfo>> HostList
            {
                get
                {
                    if (_HostList == null)
                    {
                        _HostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(HostListJson);
                        #region 从Json文件恢复数据
                        if (_IsReadFromFile && _HostList != null)
                        {
                            //恢复时间，避免被清除。
                            foreach (var item in _HostList)
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
                        #endregion
                    }
                    return _HostList;
                }
                set
                {
                    _HostList = value;
                }
            }

            /// <summary>
            /// 为Server端添加Host
            /// </summary>
            public static void AddHost(string name, string host)
            {
                if (string.IsNullOrEmpty(host)) { return; }
                var kvTable = HostList;
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
        }

        //internal static MDataTable GetHostTable()
        //{
        //   // MDictionary<string, List<HostInfo>> hostList = JsonHelper.ToEntity<MDictionary<string, List<HostInfo>>>(_HostListJson);
        //    return CreateTable(Gateway.HostList);
        //}

        //internal static MDataTable CreateTable(MDictionary<string, List<HostInfo>> hostList)
        //{
        //    MDataTable _MsTable = new MDataTable();
        //    _MsTable.TableName = MsConfig.MsTableName;
        //    _MsTable.Conn = MsConfig.MsConn;
        //    _MsTable.Columns.Add("MsID", System.Data.SqlDbType.Int, true);
        //    _MsTable.Columns.Add("MsName", System.Data.SqlDbType.NVarChar, false, false, 50);
        //    _MsTable.Columns.Add("Host", System.Data.SqlDbType.NVarChar, false, false, 250);
        //    _MsTable.Columns.Add("Version", System.Data.SqlDbType.Int);
        //    _MsTable.Columns.Add("LastActiveTime", System.Data.SqlDbType.DateTime);
        //    _MsTable.Columns.Add("CreateTime", System.Data.SqlDbType.DateTime, false, false, 0, false, CYQ.Data.SQL.SqlValue.GetDate);

        //    if (hostList != null && hostList.Count > 0 && !string.IsNullOrEmpty(MsConfig.MsConn))
        //    {
        //        foreach (KeyValuePair<string, List<HostInfo>> item in hostList)
        //        {
        //            foreach (HostInfo host in item.Value)
        //            {
        //                _MsTable.NewRow(true).Sets(1, item.Key, host.Host, host.Version, host.RegTime);
        //            }
        //        }
        //    }
        //    return _MsTable;

        //}
    }
}
