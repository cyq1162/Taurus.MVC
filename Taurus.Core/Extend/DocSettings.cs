using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Core
{
    /// <summary>
    /// 为WebAPI文档自动化测试设置默认值。
    /// </summary>
    public static class DocSettings
    {
        internal const string DocDefaultImg = "Taurus.DocDefaultImg";
        internal const string DocDefaultParas = "Taurus.DocDefaultParas";
        /// <summary>
        /// 设置默认值
        /// </summary>
        /// <param name="name">参数名</param>
        /// <param name="value">参数值</param>
        public static void Set(string name, string value)
        {
            AppConfig.SetApp("Taurus.Default" + name, value);
        }
        /// <summary>
        /// 配置Doc默认文档自动提交的图片（配置相对路径）
        /// </summary>
        public static string DefaultImg
        {
            get
            {
                return AppConfig.GetApp(DocDefaultImg);
            }
            set
            {
                AppConfig.SetApp(DocDefaultImg, value);
            }
        }
        /// <summary>
        /// 配置Doc默认追加的（多个以逗号分隔）参数（一般用于配置全局的请求头）
        /// </summary>
        public static string DefaultParas
        {
            get
            {
                return AppConfig.GetApp(DocDefaultParas);
            }
            set
            {
                AppConfig.SetApp(DocDefaultParas, value);
            }
        }
    }
}
