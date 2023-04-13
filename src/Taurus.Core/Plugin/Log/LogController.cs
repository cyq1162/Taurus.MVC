using CYQ.Data;
using CYQ.Data.Table;
using CYQ.Data.Tool;
using System.IO;
using Taurus.Mvc;

namespace Taurus.Plugin.Log
{
    /// <summary>
    /// 日志文件查看
    /// </summary>
    internal class LogController : Controller
    {
        public override void Default()
        {
            BindList();
        }

        private void BindList()
        {
            string logPath = AppConfig.WebRootPath + AppConfig.Log.LogPath;
            if (Directory.Exists(logPath))
            {
                string[] files = Directory.GetFiles(logPath, "*.txt", SearchOption.TopDirectoryOnly);
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
        public void Detail()
        {
            string fileName = Query<string>("filename");
            if (!string.IsNullOrEmpty(fileName))
            {
                string logPath = AppConfig.WebRootPath + AppConfig.Log.LogPath;
                string[] files = Directory.GetFiles(logPath, fileName, SearchOption.TopDirectoryOnly);
                if (files != null && files.Length > 0)
                {
                    string logDetail = IOHelper.ReadAllText(files[0]);
                    View.KeyValue.Set("detail", logDetail.Replace("\n", "<br/>"));
                }
            }
        }
    }
}
