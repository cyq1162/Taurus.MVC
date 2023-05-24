using CYQ.Data;
using CYQ.Data.Tool;
using Taurus.Mvc;

namespace Taurus.MicroService
{
    /// <summary>
    /// 定义安全路径，防止存档数据被直接访问（读写App_Data目录下文件）。
    /// </summary>
    internal class IO
    {
        /// <summary>
        /// 获取绝对完整路径
        /// </summary>
        /// <param name="path">相对路径，以"/" 开头</param>
        /// <returns></returns>
        public static string Path(string path)
        {
            return AppConfig.WebRootPath + "App_Data" + path;
        }
        /// <summary>
        /// 写入内容
        /// </summary>
        /// <param name="path">相对路径，以"/" 开头</param>
        public static void Write(string path, string text)
        {
            path = AppConfig.WebRootPath + "App_Data" + path;
            IOHelper.Write(path, text);
        }
        /// <summary>
        /// 读取内容
        /// </summary>
        /// <param name="path">相对路径，以"/" 开头</param>
        /// <returns></returns>
        public static string Read(string path)
        {
            path = AppConfig.WebRootPath + "App_Data" + path;
            Log.WriteLogToTxt("Read path :" + path);
            return IOHelper.ReadAllText(path);
        }
        /// <summary>
        /// 删除文件
        /// </summary>
        /// <param name="path">相对路径，以"/" 开头</param>
        public static void Delete(string path)
        {
            path = AppConfig.WebRootPath + "App_Data" + path;
            IOHelper.Delete(path);
        }
    }
}
