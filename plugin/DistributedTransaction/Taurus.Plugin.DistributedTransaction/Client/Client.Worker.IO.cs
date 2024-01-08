using CYQ.Data;
using CYQ.Data.Tool;
using CYQ.Data.Cache;
using CYQ.Data.Lock;
using System.Diagnostics;
using System.Collections.Generic;
using CYQ.Data.Json;

namespace Taurus.Plugin.DistributedTransaction
{
    public static partial class DTC
    {
        public static partial class Client
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
                        return "DTC.Client:" + key;
                    }
                    /// <summary>
                    /// 写入数据
                    /// </summary>
                    public static bool Write(Table table)
                    {
                        var disCache = DistributedCache.Instance;
                        string id = table.ExeType == ExeType.Task.ToString() ? table.MsgID : table.TraceID;
                        string json = table.ToJson();
                        //写入Redis
                        bool isOK = false;
                        if (disCache.CacheType == CacheType.Redis || disCache.CacheType == CacheType.MemCache)
                        {
                            isOK = disCache.Set(GetKey(id), json, DTCConfig.Client.Worker.TimeoutKeepSecond / 60);//写入分布式缓存
                            SetIDListWithDisLock(disCache, id, table.ExeType, true);
                        }
                        if (!isOK)
                        {
                            string path = AppConfig.WebRootPath + "App_Data/dtc/client/" + table.ExeType.ToLower() + "/" + id.Replace(':', '_') + ".txt";
                            isOK = IOHelper.Write(path, json);
                        }
                        return isOK;

                    }
                    /// <summary>
                    /// 删除数据
                    /// </summary>
                    public static bool Delete(string traceID, string msgID, string exeType)
                    {
                        string id = exeType == ExeType.Task.ToString() ? msgID : traceID;
                        var disCache = DistributedCache.Instance;
                        bool isOK = false;
                        if (disCache.CacheType == CacheType.Redis || disCache.CacheType == CacheType.MemCache)
                        {
                            isOK = disCache.Remove(GetKey(id));//删除数据。
                            SetIDListWithDisLock(disCache, id, exeType, false);
                        }
                        if (!isOK)
                        {
                            string path = AppConfig.WebRootPath + "App_Data/dtc/client/" + exeType.ToLower() + "/" + id.Replace(':', '_') + ".txt";
                            isOK = IOHelper.Delete(path);
                        }
                        return isOK;
                    }
                    /// <summary>
                    /// 读取数据
                    /// </summary>
                    public static string Read(string traceID, string msgID, string exeType)
                    {
                        string id = exeType == ExeType.Task.ToString() ? msgID : traceID;
                        var disCache = DistributedCache.Instance;
                        string result = null;
                        if (disCache.CacheType == CacheType.Redis || disCache.CacheType == CacheType.MemCache)
                        {
                            result = disCache.Get<string>(GetKey(id));
                        }
                        if (string.IsNullOrEmpty(result))
                        {
                            string path = AppConfig.WebRootPath + "App_Data/dtc/client/" + exeType.ToLower() + "/" + id.Replace(':', '_') + ".txt";
                            result = IOHelper.ReadAllText(path);
                        }
                        return result;
                    }


                    /// <summary>
                    /// 获取需要扫描重发的数据。
                    /// </summary>
                    public static List<Table> GetScanTable()
                    {
                        List<Table> tables = new List<Table>();
                        var disCache = DistributedCache.Instance;

                        if (disCache.CacheType == CacheType.Redis || disCache.CacheType == CacheType.MemCache)
                        {
                            string taskID1 = disCache.Get<string>(GetKey(ExeType.Task.ToString()));
                            string taskID2 = disCache.Get<string>(GetKey(ExeType.Commit.ToString()));
                            string taskID3 = disCache.Get<string>(GetKey(ExeType.RollBack.ToString()));
                            var ids = taskID1 + taskID2 + taskID3;
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
                            string folder = AppConfig.WebRootPath + "App_Data/dtc/client/";
                            if (System.IO.Directory.Exists(folder))  
                            {
                                string[] files = IOHelper.GetFiles(folder, "*.txt", System.IO.SearchOption.AllDirectories);
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
                        }
                        return tables;
                    }

                    /// <summary>
                    /// 维护一个列表 DoTask=>{id1,id2,id3},Commit=>{id1,id2,id3},RollBack=>{id1,id2,id3}
                    /// </summary>
                    private static void SetIDListWithDisLock(DistributedCache disCache, string id, string exeType, bool isAdd)
                    {
                        #region 更新列表
                        double defaultCacheTime = DTCConfig.Client.Worker.TimeoutKeepSecond / 60;
                        var disLock = DistributedLock.Instance;
                        bool isGetLock = false;
                        string exeTypeKey = GetKey("ExeType:" + exeType);
                        string lockKey = exeTypeKey + ".lock";
                        try
                        {
                            isGetLock = disLock.Lock(lockKey, 5000);//锁定
                            if (isGetLock)
                            {
                                var ids = disCache.Get<string>(exeTypeKey);
                                if (isAdd)
                                {
                                    if (ids == null)
                                    {
                                        disCache.Set(exeTypeKey, id + ",", defaultCacheTime);

                                    }
                                    else
                                    {
                                        if (!ids.Contains(id))
                                        {
                                            disCache.Set(exeTypeKey, ids + id + ",", defaultCacheTime);
                                        }
                                    }
                                }
                                else
                                {
                                    //remove
                                    if (!string.IsNullOrEmpty(ids))
                                    {
                                        ids = ids.Replace(id + ",", "");
                                    }
                                    if (string.IsNullOrEmpty(ids))
                                    {
                                        disCache.Remove(exeTypeKey);
                                    }
                                    else
                                    {
                                        disCache.Set(exeTypeKey, ids, defaultCacheTime);
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
