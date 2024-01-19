using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using static Taurus.Plugin.Limit.LimitConfig;
using Taurus.Plugin.Admin;
using Taurus.Mvc.Properties;
using System.Threading;
using CYQ.Data;

namespace Taurus.Mvc
{
    public static partial class ViewEngine
    {
        private static void ZipTo(string directoryPath, byte[] zipBytes)
        {
            Directory.CreateDirectory(directoryPath);
            string zipFileName = directoryPath + "/htmlcssjs.zip";
            using (FileStream fs = File.Create(zipFileName))
            {
                fs.Write(zipBytes, 0, zipBytes.Length);
            }
            ZipFile.ExtractToDirectory(zipFileName, directoryPath, true);
            File.Delete(zipFileName);
        }

    }
}
