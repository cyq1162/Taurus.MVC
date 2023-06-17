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
            View.KeyValue.Set("Today", DateTime.Now.ToString("yyyyMMdd"));
            View.KeyValue.Set("Yesterday", DateTime.Now.AddDays(-1).ToString("yyyyMMdd"));
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
            dtTaurus.Bind(View);
        }

    }
}
