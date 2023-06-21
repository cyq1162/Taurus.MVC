using System;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using System.Threading;
using System.Diagnostics;
using System.Reflection;
using System.Net;
using Taurus.Plugin.Metric;
using System.Collections.Generic;
using CYQ.Data.Xml;
using CYQ.Data.Cache;
using CYQ.Data.Tool;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 接口访问指标信息
    /// </summary>
    internal partial class AdminController
    {

        /// <summary>
        /// 接口访问指标
        /// </summary>
        public void Metric()
        {
            InitMenu();
            string type = Query<string>("t", "api").ToLower();
            if (type.StartsWith("api"))
            {
                Metric_API();
            }
            else if (type.StartsWith("redis"))
            {
                Metric_Redis();
            }
            else if (type.StartsWith("memcache"))
            {
                Metric_MemCache();
            }
        }

        private void InitMenu()
        {
            // API
            View.KeyValue.Set("Today", DateTime.Now.ToString("yyyyMMdd"));
            View.KeyValue.Set("Yesterday", DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
            //Redis
            if (string.IsNullOrEmpty(AppConfig.Redis.Servers))
            {
                View.Set("redis", SetType.ClearFlag, "1");
            }
            //MemCache
            if (string.IsNullOrEmpty(AppConfig.MemCache.Servers))
            {
                View.Set("memcache", SetType.ClearFlag, "1");
            }
        }

        private void Metric_API()
        {
            View.RemoveAttr("apiTable", "clearflag");
            string day = Query<string>("d", DateTime.Now.ToString("yyyyMMdd"));
            var metric = MetricRun.GetMetric(day);
            if (metric == null)
            {
                return;
            }
            List<string> list = metric.GetKeys();
            MDataTable dtTaurus = new MDataTable();
            dtTaurus.Columns.Add("LocalPath");
            for (int i = 0; i < 25; i++)
            {
                dtTaurus.Columns.Add("h" + i, System.Data.SqlDbType.BigInt);
            }
            long total = 0;
            foreach (string key in list)
            {
                var value = metric[key];
                total += value[24];
                var row = dtTaurus.NewRow(true).Set(0, key);
                for (int i = 0; i < value.Length; i++)
                {
                    row.Set(i + 1, value[i]);
                }
            }
            View.KeyValue.Set("KeyCount", list.Count.ToString());
            View.KeyValue.Set("Total", total.ToString());
            string sort = Query<string>("s", "LocalPath");
            bool isDesc = Query<bool>("desc");
            dtTaurus.Rows.Sort(sort + (isDesc ? " desc" : " asc"));
            dtTaurus.Bind(View, "apiView");
        }

        private void Metric_Redis()
        {
            View.RemoveAttr("redisTable", "clearflag");
            string type = Query<string>("t").ToLower();


            MDataTable bindTable = null;
            if (type.StartsWith("redis-status"))
            {
                bindTable = CacheManage.RedisInstance.CacheInfo;

                View.SetForeach(bindTable, "redisView", GetRowText(bindTable));
            }
            else if (type.StartsWith("redis-socket"))
            {
                bindTable = GetTable(CacheManage.RedisInstance.WorkInfo);
                if (bindTable != null)
                {
                    View.SetForeach(bindTable, "redisView", GetRowText(bindTable));
                }
            }
            if (bindTable != null)
            {
                MDataTable redisStatusHead = bindTable.Columns.ToTable();
                foreach (var item in redisStatusHead.Rows)
                {
                    item[0].Value = item[0].StringValue.Split('-')[0];
                }
                redisStatusHead.Bind(View, "redisHead");
            }
        }

        private void Metric_MemCache()
        {
            View.RemoveAttr("memcacheTable", "clearflag");

            string type = Query<string>("t").ToLower();


            MDataTable bindTable = null;
            if (type.StartsWith("memcache-status"))
            {
                bindTable = CacheManage.MemCacheInstance.CacheInfo;

                View.SetForeach(bindTable, "memcacheView", GetRowText(bindTable));
            }
            else if (type.StartsWith("memcache-socket"))
            {
                bindTable = GetTable(CacheManage.MemCacheInstance.WorkInfo);
                if (bindTable != null)
                {
                    View.SetForeach(bindTable, "memcacheView", GetRowText(bindTable));
                }
            }
            if (bindTable != null)
            {
                MDataTable headTable = bindTable.Columns.ToTable();
                foreach (var item in headTable.Rows)
                {
                    item[0].Value = item[0].StringValue.Split('-')[0];
                }
                headTable.Bind(View, "memcacheHead");
            }
        }

        private string GetRowText(MDataTable dt)
        {
            string rowText = "<tr><td align=\"left\" width=\"300px\">${0}</td>";
            for (int i = 1; i < dt.Columns.Count; i++)
            {
                rowText += "<td align=\"left\">${" + i + "}</td>";
            }
            rowText += "</tr>";
            return rowText;
        }


        private MDataTable GetTable(string workJson)
        {
            string serverJson = JsonHelper.GetValue<string>(workJson, "Servers");
            if (string.IsNullOrEmpty(serverJson)) { return null; }


            MDataTable cacheTable = new MDataTable();
            Dictionary<string, string> status = JsonHelper.Split(serverJson);
            if (status != null)
            {
                foreach (KeyValuePair<string, string> item in status)
                {
                    var value = JsonHelper.Split(item.Value);
                    if (value.Count > 0)
                    {
                        MDataTable dt = MDataTable.CreateFrom(value);
                        if (cacheTable.Columns.Count == 0)//第一次
                        {
                            cacheTable = dt;
                        }
                        else
                        {
                            cacheTable.JoinOnName = "Key";
                            cacheTable = cacheTable.Join(dt, "Value");
                        }
                        cacheTable.Columns["Value"].ColumnName = item.Key;
                    }
                }
            }
            cacheTable.TableName = "WorkTable";

            return cacheTable;
        }
    }
}
