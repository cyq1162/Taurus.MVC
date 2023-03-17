using CYQ.Data;
using CYQ.Data.Tool;


namespace Taurus.MicroService
{
    /// <summary>
    /// 定义安全路径，防止存档数据被直接访问。
    /// </summary>
    internal class IO
    {
        public static void Write(string path, string text)
        {
            return;
            path = AppConfig.WebRootPath + "/App_Data/" + path;
            IOHelper.Write(path, text);
        }

        public static string Read(string path)
        {
            return "";
            path = AppConfig.WebRootPath + "/App_Data/" + path;
            return IOHelper.ReadAllText(path);
        }
        public static void Delete(string path)
        {
            return;
            path = AppConfig.WebRootPath + "/App_Data/" + path;
            IOHelper.Delete(path);
        }
    }
}
