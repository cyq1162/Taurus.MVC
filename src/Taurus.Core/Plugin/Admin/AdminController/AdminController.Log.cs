using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using CYQ.Data;
using System.IO;


namespace Taurus.Plugin.Admin
{

    /// <summary>
    /// 日志信息
    /// </summary>
    internal partial class AdminController
    {
        /// <summary>
        /// 错误日志
        /// </summary>
        public void Log()
        {
            View.KeyValue.Add("yyyyMM", DateTime.Now.ToString("yyyyMM"));
            string logPath = AppConfig.WebRootPath + AppConfig.Log.Path.Trim('/', '\\');
            if (Directory.Exists(logPath))
            {
                string key = Query<string>("k", DateTime.Now.ToString("*yyyyMM"));
                string[] files = Directory.GetFiles(logPath, key + "*.txt", SearchOption.TopDirectoryOnly);
                MDataTable dt = new MDataTable();
                dt.Columns.Add("FileName");
                foreach (string file in files)
                {
                    dt.NewRow(true).Set(0, Path.GetFileName(file));
                }
                dt.Rows.Sort("FileName desc");
                dt.Bind(View, "fileList");

            }
        }
        public void LogDetail()
        {
            string fileName = Query<string>("filename");
            if (!string.IsNullOrEmpty(fileName))
            {
                string logPath = AppConfig.WebRootPath + AppConfig.Log.Path.Trim('/', '\\');
                string[] files = Directory.GetFiles(logPath, fileName, SearchOption.TopDirectoryOnly);
                if (files != null && files.Length > 0)
                {
                    string logDetail = IOHelper.ReadAllText(files[0]);
                    logDetail = System.Web.HttpUtility.HtmlEncode(logDetail);
                    logDetail = logDetail.Replace("\n", "<br/>").Replace("&lt;hr /&gt;", "<hr />");
                    View.KeyValue.Add("detail", logDetail);
                }
            }
        }
        public void LogDelete()
        {
            string fileName = Query<string>("filename");
            if (!string.IsNullOrEmpty(fileName))
            {
                string logPath = AppConfig.WebRootPath + AppConfig.Log.Path.Trim('/', '\\') + "/" + fileName;
                if (File.Exists(logPath))
                {
                    IOHelper.Delete(logPath);
                    Write("Delete success.", true);
                    return;
                }
            }
            Write("Delete fail.", false);
        }
    }

}
