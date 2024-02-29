using CYQ.Data;
using CYQ.Data.Tool;
using System.IO;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 定义安全路径，防止存档数据被直接访问（读写App_Data目录下文件）。
    /// </summary>
    internal class IO
    {
        /// <summary>
        /// 获取文件信息
        /// </summary>
        public static FileInfo Info(string path)
        {
            return new FileInfo(Path(path));
        }
        /// <summary>
        /// 检测文件是否存在
        /// </summary>
        public static bool Exists(string path)
        {
            return File.Exists(path);
        }
        /// <summary>
        /// 获取绝对完整路径
        /// </summary>
        /// <param name="path">相对路径，以"/" 开头</param>
        /// <returns></returns>
        public static string Path(string path)
        {
            return AppConst.WebRootPath + "App_Data" + path;
        }
        /// <summary>
        /// 写入内容
        /// </summary>
        /// <param name="path">相对路径，以"/" 开头</param>
        public static void Write(string path, string text)
        {
            path = AppConst.WebRootPath + "App_Data" + path;
            IOHelper.Write(path, text);
        }
        /// <summary>
        /// 读取内容
        /// </summary>
        /// <param name="path">相对路径，以"/" 开头</param>
        /// <returns></returns>
        public static string Read(string path)
        {
            path = AppConst.WebRootPath + "App_Data" + path;
            return IOHelper.ReadAllText(path);
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path">相对路径，以"/" 开头</param>
        public static void Delete(string path)
        {
            path = AppConst.WebRootPath + "App_Data" + path;
            IOHelper.Delete(path);
        }

    }
}
