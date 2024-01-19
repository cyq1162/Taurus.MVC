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

        private static void ZipTo(string directoryPath, byte[] zipBytes)
        {
            Directory.CreateDirectory(directoryPath);
            string zipFileName = directoryPath + "/htmlcssjs.zip";
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
        }
    }
}
