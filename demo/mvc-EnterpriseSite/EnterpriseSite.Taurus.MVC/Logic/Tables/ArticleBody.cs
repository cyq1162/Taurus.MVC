using System;
using System.Data;
using System.Configuration;
using System.Web;
using System.IO;
using System.Text;

namespace Taurus.Logic
{
    public class ArticleBody
    {
        public static void Set(int id, string body)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/db/body/" + id + ".body";
            File.WriteAllText(path, body, Encoding.Default);
        }
        public static string Get(int id)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + "/App_Data/db/body/" + id + ".body";
            if (File.Exists(path))
            {
                return File.ReadAllText(path,Encoding.Default);
            }
            return string.Empty;
        }
    }
}
