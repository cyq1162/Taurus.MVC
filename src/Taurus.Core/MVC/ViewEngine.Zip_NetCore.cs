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
        internal static bool ZipTo(string directoryPath, byte[] zipBytes)
        {
            try
            {
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                string zipFileName = directoryPath + "/" + DateTime.Now.Ticks + ".zip";
                using (FileStream fs = File.Create(zipFileName))
                {
                    fs.Write(zipBytes, 0, zipBytes.Length);
                }
                ZipFile.ExtractToDirectory(zipFileName, directoryPath, true);
                File.Delete(zipFileName);
                return true;
            }
            catch (Exception err)
            {
                Log.Write(err);
                return false;
            }

        }

    }
}
