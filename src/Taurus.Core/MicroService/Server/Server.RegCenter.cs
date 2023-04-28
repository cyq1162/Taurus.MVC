﻿using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using Taurus.Plugin.Admin;

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
            static RegCenter()
            {
                if (hostListByAdmin.Count == 0)
                {
                    string hostList = IO.Read(AdminConst.HostAddPath);
                    if (!string.IsNullOrEmpty(hostList))
                    {
                        AddHostByAdmin(hostList);
                    }
                }
            }
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
                string[] items = name.Split('|');//允许模块域名带优先级版本号，和是否虚拟属性
                name = items[0];
                int ver = 0;
                bool vir = false;
                if (items.Length > 1)
                {
                    int.TryParse(items[1], out ver);
                }
                if (items.Length > 2)
                {
                    vir = items[2] == "1" || items[2].ToLower() == "true";
                }
                var kvTable = HostList;
                if (!kvTable.ContainsKey(name))
                {
                    //首次添加
                    Server.IsChange = true;
                    List<HostInfo> list = new List<HostInfo>();
                    HostInfo info = new HostInfo();
                    info.Host = host;
                    info.RegTime = DateTime.Now;
                    info.Version = ver;
                    info.IsVirtual = vir;
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
                            hostInfo.Version = ver;
                            hostInfo.IsVirtual = vir;
                            hostInfo.RegTime = DateTime.Now;//更新时间。
                            return;
                        }
                    }
                    Server.IsChange = true;
                    HostInfo info = new HostInfo();
                    info.Host = host;
                    info.Version = ver;
                    info.IsVirtual = vir;
                    info.RegTime = DateTime.Now;
                    list.Add(info);
                }
            }


            #region Admin管理后台：手工添加Host

            private static Dictionary<string, string> hostListByAdmin = new Dictionary<string, string>();
            /// <summary>
            /// Admin管理后台：手工添加Host
            /// </summary>
            /// <param name="hostList">手工添加HostList</param>
            internal static void AddHostByAdmin(string hostList)
            {
                Dictionary<string, string> hostDic = new Dictionary<string, string>();
                if (!string.IsNullOrEmpty(hostList))
                {
                    string[] rows = hostList.Split('\n');
                    foreach (string row in rows)
                    {
                        string[] kv = row.Split('#');
                        if (kv.Length == 2)
                        {
                            string host = kv[1];
                            string[] names = kv[0].Split(',');
                            foreach (string name in names)
                            {
                                if (string.IsNullOrEmpty(name))
                                {
                                    continue;
                                }
                                if (hostDic.ContainsKey(name))
                                {
                                    hostDic[name] = hostDic[name] + "," + host;
                                }
                                else
                                {
                                    hostDic.Add(name, host);
                                }
                            }
                        }
                    }

                }
                hostListByAdmin = hostDic;
            }
            /// <summary>
            /// 加载所有手工添加主机信息
            /// </summary>
            internal static void LoadHostByAdmin()
            {
                var hostList = hostListByAdmin;//获取引用
                foreach (var item in hostList)
                {
                    string[] hosts = item.Value.Split(',');
                    foreach (string host in hosts)
                    {
                        AddHost(item.Key, host);
                    }
                }
            }

            #endregion
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
