using CYQ.Data.Tool;
using Taurus.Mvc;

namespace Taurus.MicroService
{
    /// <summary>
    /// 定义安全路径，防止存档数据被直接访问（读写App_Data目录下文件）。
    /// </summary>
    internal class IO
    {
        public static string Path(string path)
        {
            return MvcConst.AppDataFolderPath + path;
        }
        public static void Write(string path, string text)
        {
            path = MvcConst.AppDataFolderPath + path;
            IOHelper.Write(path, text);
        }

        public static string Read(string path)
        {
            path = MvcConst.AppDataFolderPath + path;
            return IOHelper.ReadAllText(path);
        }
        public static void Delete(string path)
        {
            path = MvcConst.AppDataFolderPath + path;
            IOHelper.Delete(path);
        }
    }
}
