using System;
using System.Collections.Generic;
using System.IO.Compression;
using System.IO;
using System.Text;
using Taurus.Plugin.Admin;
using System.Threading;
using CYQ.Data;
using Taurus.Plugin.Doc;
using Ionic.Zip;

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
                using (ZipFile zipFile = ZipFile.Read(zipFileName))
                {
                    foreach (ZipEntry entry in zipFile)
                    {
                        if (!entry.IsDirectory)
                        {
                            entry.Extract(directoryPath, ExtractExistingFileAction.OverwriteSilently);
                        }
                    }
                }
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
