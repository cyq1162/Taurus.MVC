using System;
using CYQ.Data.Tool;
using CYQ.Data.Table;
using Taurus.Mvc;
using CYQ.Data;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using Taurus.Plugin.Doc;
using System.Configuration;
using CYQ.Data.Cache;
using Taurus.Mvc.Reflect;
using System.Web;
using System.IO;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 应用配置信息
    /// </summary>
    internal partial class AdminController
    {
        /// <summary>
        /// 上传文件
        /// </summary>
        public void UploadSSL(HttpPostedFile file)
        {
            if (file == null || Path.GetExtension(file.FileName) != ".zip") { Write("you should uplad a zip file.", false); return; }
            string sslPath = AppConfig.WebRootPath + MvcConfig.Kestrel.SslPath.TrimStart(new char[] { '/', '\\' });
            byte[] bytes = new byte[file.InputStream.Length];
            file.InputStream.Read(bytes, 0, (int)file.InputStream.Length);
            if (ViewEngine.ZipTo(sslPath, bytes))
            {
                Write("upload success.", true);
            }
            else
            {
                Write("upload fail.", false);
            }
        }
    }
}
