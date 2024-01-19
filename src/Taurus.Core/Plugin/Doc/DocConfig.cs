using CYQ.Data;
using System.Web;
using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Doc
{
    /// <summary>
    /// 为WebAPI文档自动化测试设置全局图片默认值|追加参数(其它参数设置用当前Controller控制器的SetQuery方法设置)
    /// </summary>
    public static class DocConfig
    {

        /// <summary>
        /// 配置是否启用WebAPI文档自动生成功能 
        /// 如 Doc.IsEnable ：true， 默认值：纯网关：false，其它：true
        /// </summary>
        public static bool IsEnable
        {
            get
            {
                return AppConfig.GetAppBool("Doc.IsEnable", !MsConfig.IsGateway);
            }
            set
            {
                AppConfig.SetApp("Doc.IsEnable", value.ToString());
            }
        }
        /// <summary>
        /// 配置Mvc的WebAPI文档访问路径
        /// 如 Doc.Path ： "doc"， 默认值：doc
        /// </summary>
        public static string Path
        {
            get
            {
                return AppConfig.GetApp("Doc.Path", "/doc");
            }
            set
            {
                AppConfig.SetApp("Doc.Path", value);
            }
        }
        /// <summary>
        /// 配置Doc的html加载文件夹名称
        /// 如 Doc.HtmlFolderName ： "Doc"， 默认值：Doc
        /// </summary>
        internal static string HtmlFolderName
        {
            get
            {
                return AppConfig.GetApp("Doc.HtmlFolderName", "Doc").Trim('/');
            }
        }
        /// <summary>
        /// 只读：从默认图片转换成的：HttpPostedFile
        /// 需要配置：DefaultImg 图片相对路径
        /// </summary>
        public static HttpPostedFile DefaultImgHttpPostedFile
        {
            get
            {
                if (!string.IsNullOrEmpty(DefaultImg))
                {
                    return HttpPostedFileExtend.Create(DefaultImg);
                }
                return null;
            }
        }

        /// <summary>
        /// 配置Doc默认文档自动提交的图片（配置相对路径），
        /// 如：Doc.DefaultImg ："/App_Data/xxx.jpg"
        /// </summary>
        public static string DefaultImg
        {
            get
            {
                return AppConfig.GetApp("Doc.DefaultImg", "");
            }
            set
            {
                AppConfig.SetApp("Doc.DefaultImg", value);
            }
        }
        /// <summary>
        /// 配置Doc默认追加的（多个以逗号分隔）参数（一般用于配置全局的请求头，如ack，token等），
        /// 如：Doc.DefaultParas ："ack,token"
        /// </summary>
        public static string DefaultParas
        {
            get
            {
                return AppConfig.GetApp("Doc.DefaultParas", "");
            }
            set
            {
                AppConfig.SetApp("Doc.DefaultParas", value);
            }
        }
    }
}
