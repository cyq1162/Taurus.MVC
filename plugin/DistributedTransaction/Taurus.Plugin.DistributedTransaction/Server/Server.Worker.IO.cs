using CYQ.Data;
using CYQ.Data.Cache;
using CYQ.Data.Json;
using CYQ.Data.Lock;
using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.IO;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTC
    {
        public static partial class Server
        {
            /// <summary>
            /// dtc 写数据库、写队列
            /// </summary>
            internal static partial class Worker
            {
                internal static class IO
                {
                    private static string GetKey(string key)
                    {
                        return "DTC.Server:" + key;
                    }

                    /// <summary>
                    /// 写入数据
                    /// </summary>
                    public static bool Write(Table table)
                    {
                        var disCache = DistributedCache.Instance;
                        string json = table.ToJson();
                        bool isOK = false;
                        if (disCache.CacheType == CacheType.Redis || disCache.CacheType == CacheType.MemCache)
                        {
                            isOK = disCache.Set(GetKey(table.MsgID), json, DTCConfig.Server.Worker.TimeoutKeepSecond / 60);//写入分布式缓存
                            SetTraceIDListWithDisLock(disCache, table.TraceID, table.MsgID, table.ExeType, true);//写入traceID => 多个msgID
                        }
                        if (!isOK)
                        {
                            string path = AppConfig.WebRootPath + "App_Data/dtc/server/";
                            if (table.ExeType == ExeType.Task.ToString())
                            {
                                path += table.MsgID + ".txt";
                            }
                            else
                            {
                                path += table.TraceID.Replace(':', '_') + "/" + table.MsgID + ".txt";
                            }
                            isOK = IOHelper.Write(path, json);
                        }
                        return isOK;


                    }

                    /// <summary>
                    /// 删除数据
                    /// </summary>
                    public static bool Delete(string traceID, string msgID, string exeType)
                    {
                        var disCache = DistributedCache.Instance;
                        bool isOK = false;
                        if (disCache.CacheType == CacheType.Redis || disCache.CacheType == CacheType.MemCache)
                        {
                            isOK = disCache.Remove(GetKey(msgID));//删除数据。
                            SetTraceIDListWithDisLock(disCache, traceID, msgID, exeType, false);//写入traceID => 多个msgID

                        }
                        if (!isOK)
                        {
                            if (exeType == ExeType.Task.ToString())
                            {
                                string path = AppConfig.WebRootPath + "App_Data/dtc/server/" + msgID + ".txt";
                                isOK = IOHelper.Delete(path);
                            }
                            else
                            {
                                string folder = AppConfig.WebRootPath + "App_Data/dtc/server/" + traceID.Replace(':', '_');
                                string path = folder + "/" + msgID + ".txt";
                                isOK = IOHelper.Delete(path);
                            }
                        }
                        return isOK;
                    }

                    /// <summary>
                    /// 是否存在数据
                    /// </summary>
                    public static bool Exists(string traceID, string msgID, string exeType)
                    {
                        string id = exeType == ExeType.Task.ToString() ? msgID : traceID;
                        var disCache = DistributedCache.Instance;
                        bool isExists = false;
                        if (disCache.CacheType == CacheType.Redis || disCache.CacheType == CacheType.MemCache)
                        {
                            isExists = disCache.Get(GetKey(id)) != null;

                        }
                        if (!isExists)
                        {
                            string path = AppConfig.WebRootPath + "App_Data/dtc/server/" + id.Replace(':', '_') + ".txt";
                            isExists = IOHelper.ExistsDirectory(path);
                        }
                        return isExists;
                    }
                    /// <summary>
                    /// 获取数据：仅commit、rollback用到
                    /// </summary>
                    public static List<Table> GetListByTraceID(string traceID, string exeType)
                    {
                        List<Table> tables = new List<Table>();

                        var disCache = DistributedCache.Instance;
                        if (disCache.CacheType == CacheType.Redis || disCache.CacheType == CacheType.MemCache)
                        {
                            var ids = disCache.Get<string>(GetKey(traceID));
                            if (!string.IsNullOrEmpty(ids))
                            {
                                foreach (string id in ids.Split(','))
                                {
                                    var json = disCache.Get<string>(GetKey(id));
                                    if (!string.IsNullOrEmpty(json))
                                    {
                                        var entity = JsonHelper.ToEntity<Table>(json);
                                        if (entity != null)
                                        {
                                            tables.Add(entity);
                                        }

                                    }
                                }
                            }
                        }
                        if (tables.Count == 0)
                        {
                            string folder = AppConfig.WebRootPath + "App_Data/dtc/server/" + traceID.Replace(':', '_');
                            string[] files = IOHelper.GetFiles(folder);
                            if (files != null && files.Length > 0)
                            {
                                foreach (string file in files)
                                {
                                    string json = IOHelper.ReadAllText(file, 0);
                                    if (!string.IsNullOrEmpty(json))
                                    {
                                        tables.Add(JsonHelper.ToEntity<Table>(json));
                                    }
                                }
                            }
                        }
                        return tables;
                    }

                    /// <summary>
                    /// 获取超时需要删除的数据，仅硬盘文件需要删除。
                    /// </summary>
                    public static void DeleteTimeoutTable()
                    {
                        var disCache = DistributedCache.Instance;
                        if (disCache.CacheType == CacheType.LocalCache)
                        {
                            try
                            {
                                string folder = AppConfig.WebRootPath + "App_Data/dtc/server/";
                                DirectoryInfo directoryInfo = new DirectoryInfo(folder);
                                if (directoryInfo.Exists)
                                {
                                    //System.IO.Directory.em
                                    FileInfo[] files = directoryInfo.GetFiles("*.txt", SearchOption.AllDirectories);
                                    if (files != null && files.Length > 0)
                                    {

                                        int timeoutSecond = DTCConfig.Server.Worker.TimeoutKeepSecond;
                                        foreach (FileInfo file in files)
                                        {
                                            if (file.LastWriteTime < DateTime.Now.AddSeconds(-timeoutSecond))
                                            {
                                                file.Delete();
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                Log.Write(err, "DTC.Server");
                            }
                        }
                    }

                    /// <summary>
                    /// 删除空目录文件夹【由提交和回滚产生的】
                    /// </summary>
                    public static void DeleteEmptyDirectory()
                    {
                        var disCache = DistributedCache.Instance;
                        if (disCache.CacheType == CacheType.LocalCache)
                        {
                            try
                            {
                                string folder = AppConfig.WebRootPath + "App_Data/dtc/server/";
                                if (System.IO.Directory.Exists(folder))
                                {
                                    //删除空文件夹
                                    string[] folders = Directory.GetDirectories(folder);
                                    if (folders != null && folders.Length > 0)
                                    {
                                        foreach (string path in folders)
                                        {
                                            if (Directory.GetFiles(path, "*", SearchOption.TopDirectoryOnly).Length == 0)
                                            {
                                                Directory.Delete(path, false);
                                            }
                                        }
                                    }
                                }
                            }
                            catch (Exception err)
                            {
                                Log.Write(err, "DTC.Server");
                            }
                        }
                    }
                    /// <summary>
                    /// 维护一个列表：TraceID=>{id1,id2,id3}
                    /// </summary>
                    private static void SetTraceIDListWithDisLock(DistributedCache disCache, string traceID, string msgID, string exeType, bool isWrite)
                    {
                        if (exeType == ExeType.Task.ToString()) { return; }
                        double defaultCacheTime = DTCConfig.Server.Worker.TimeoutKeepSecond / 60;
                        #region 更新列表
                        var disLock = DistributedLock.Instance;
                        bool isGetLock = false;
                        var traceKey = GetKey(traceID);
                        string lockKey = traceKey + ".lock";// GetKey("Lock." + traceID);// ;
                        try
                        {
                            isGetLock = disLock.Lock(lockKey, 5000);//锁定
                            if (isGetLock)
                            {
                                var ids = disCache.Get<string>(traceKey);
                                if (isWrite)
                                {
                                    if (ids == null)
                                    {
                                        disCache.Set(traceKey, msgID + ",", defaultCacheTime);

                                    }
                                    else
                                    {
                                        disCache.Set(traceKey, ids + msgID + ",", defaultCacheTime);
                                    }
                                }
                                else
                                {
                                    //remove
                                    if (!string.IsNullOrEmpty(ids))
                                    {
                                        ids = ids.Replace(msgID + ",", "");
                                    }
                                    if (string.IsNullOrEmpty(ids))
                                    {
                                        disCache.Remove(traceKey);
                                    }
                                    else
                                    {
                                        disCache.Set(traceKey, ids, defaultCacheTime);
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (isGetLock)
                            {
                                disLock.UnLock(lockKey);//释放锁。
                            }
                        }
                        #endregion

                    }
                }
            }

        }
    }
}
