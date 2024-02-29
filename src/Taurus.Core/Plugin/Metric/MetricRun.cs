using CYQ.Data;
using CYQ.Data.Json;
using CYQ.Data.Tool;
using System;
using System.IO;
using System.Threading;

namespace Taurus.Plugin.Metric
{
    /// <summary>
    /// 指标统计（统计各接口请求次数）
    /// </summary>
    internal class MetricRun
    {
        static MetricRun()
        {
            string folder = AppConst.WebRootPath + MetricConfig.DurablePath.TrimStart('/');
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }
            if (MetricConfig.IsDurable)
            {
                string file = folder + "/" + DateTime.Now.ToString("yyyyMMdd") + ".json";
                if (File.Exists(file))
                {
                    string json = IOHelper.ReadAllText(file);
                    if (!string.IsNullOrEmpty(json))
                    {
                        todayMetric = JsonHelper.ToEntity<MDictionary<string, long[]>>(json);
                    }
                }
            }
            ThreadBreak.AddGlobalThread(new System.Threading.ParameterizedThreadStart(DoMetricTask));
        }
        /// <summary>
        /// 循环任务
        /// </summary>
        /// <param name="para"></param>
        private static void DoMetricTask(object para)
        {
            while (true)
            {
                int interval = MetricConfig.DurableInterval;
                Thread.Sleep(interval * 1000);
                try
                {
                    if (MetricConfig.IsDurable && todayMetric.Count > 0)
                    {
                        string file = AppConst.WebRootPath + MetricConfig.DurablePath.TrimStart('/') + "/" + DateTime.Now.ToString("yyyyMMdd") + ".json";
                        string json = JsonHelper.ToJson(todayMetric);
                        IOHelper.Write(file, json);
                    }
                }
                catch (Exception err)
                {
                    Log.WriteLogToTxt(err, LogType.Taurus);
                }
            }
        }
        /// <summary>
        /// 存档 接口 访问 统计
        /// </summary>
        private static MDictionary<string, long[]> todayMetric = new MDictionary<string, long[]>(StringComparer.OrdinalIgnoreCase);
        private static int today = DateTime.Now.Day;

        public static MDictionary<string, long[]> TodayMetric
        {
            get
            {
                if (DateTime.Now.Day != today)
                {
                    todayMetric.Clear();
                    today = DateTime.Now.Day;
                }
                return todayMetric;
            }
        }
        /// <summary>
        /// 根据时间获取数据
        /// </summary>
        public static MDictionary<string, long[]> GetMetric(DateTime day)
        {
            if (day.DayOfYear == DateTime.Now.DayOfYear)
            {
                return todayMetric;
            }
            return GetMetric(day.ToString("yyyyMMdd"));
        }
        public static MDictionary<string, long[]> GetMetric(string dayString)
        {
            if (string.IsNullOrEmpty(dayString) || dayString == DateTime.Now.ToString("yyyyMMdd"))
            {
                return todayMetric;
            }
            string file = AppConst.WebRootPath + MetricConfig.DurablePath.TrimStart('/') + "/" + dayString + ".json";
            if (File.Exists(file))
            {
                string json = IOHelper.ReadAllText(file);
                if (!string.IsNullOrEmpty(json))
                {
                    return JsonHelper.ToEntity<MDictionary<string, long[]>>(json);
                }
            }
            return null;
        }
        /// <summary>
        /// 开始统计
        /// </summary>
        public static void Start(Uri uri)
        {
            string localPath = uri.LocalPath;
            if (string.IsNullOrEmpty(localPath)) { localPath = "/"; }
            var metric = TodayMetric;
            if (metric.ContainsKey(localPath))
            {
                var list = metric[localPath];
                list[24]++;
                list[DateTime.Now.Hour]++;
            }
            else
            {
                long[] longs = new long[25];
                longs[24]++;
                longs[DateTime.Now.Hour]++;
                metric.Add(localPath, longs);
            }

        }
    }
}
