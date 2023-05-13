using System;
using CYQ.Data.Xml;
using CYQ.Data;
using CYQ.Data.Tool;
using System.IO;
using System.Xml;
namespace Taurus.Mvc
{
    /// <summary>
    /// 视图引擎
    /// </summary>
    public static class ViewEngine
    {
        private static string _ViewPath = string.Empty;
        internal static string ViewsPath
        {
            get
            {
                if (string.IsNullOrEmpty(_ViewPath))
                {
                    _ViewPath = AppConfig.WebRootPath + MvcConfig.Views;
                    if (!Directory.Exists(_ViewPath))
                    {
                        _ViewPath = AppConfig.WebRootPath + MvcConfig.Views.ToLower();//兼容Linux 文件夹小写
                    }
                }
                return _ViewPath;
            }

        }
        private static string _SharedPath = string.Empty;
        internal static string SharedPath
        {
            get
            {
                if (string.IsNullOrEmpty(_SharedPath))
                {
                    _SharedPath = ViewsPath + "/Shared";
                    if (!Directory.Exists(_SharedPath))
                    {
                        _SharedPath = ViewsPath + "/shared"; ;//兼容Linux 文件夹小写
                    }
                }
                return _SharedPath;
            }

        }
        /// <summary>
        /// 创建视图对象
        /// </summary>
        public static XHtmlAction Create(string controlName, string actionName)
        {
            string cName = controlName.Replace(ReflectConst.Controller, "");
            if (Directory.Exists(ViewsPath))
            {
                string[] folders = Directory.GetDirectories(ViewsPath, "*", SearchOption.TopDirectoryOnly);
                foreach (string folder in folders)
                {
                    string foName = Path.GetFileNameWithoutExtension(folder);
                    if (string.Equals(cName, foName, StringComparison.OrdinalIgnoreCase))
                    {
                        string[] files = Directory.GetFiles(folder, "*.html", SearchOption.TopDirectoryOnly);
                        foreach (string file in files)
                        {
                            string fiName = Path.GetFileNameWithoutExtension(file);
                            if (string.Equals(actionName, fiName, StringComparison.OrdinalIgnoreCase))
                            {
                                return Create(file);
                            }
                        }
                        break;
                    }
                }
            }
            return null;
            //string folder = ViewsPath + "/" + cName;

            //if (!Directory.Exists(folder))
            //{
            //    folder = ViewsPath + "/" + cName.ToLower();
            //}
            //string filePath = folder + "/" + actionName + ".html";
            //if (!File.Exists(filePath))
            //{
            //    filePath = folder + "/" + actionName.ToLower() + ".html";
            //    if (!File.Exists(filePath))
            //    {
            //        return null;
            //    }
            //}
            //return Create(filePath);
        }
        /// <summary>
        /// 创建视图对象
        /// </summary>
        /// <param name="path">相对路径，如：/abc/cyq/a.html</param>
        public static XHtmlAction Create(string fullPath)
        {
            XHtmlAction view = new XHtmlAction(true, false);
            if (view.Load(fullPath, XmlCacheLevel.Hour, true))
            {
                // System.Web.HttpContext.Current.Response.Write("load ok");
                //处理Shared目录下的节点替换。
                ReplaceItemRef(view, view.GetList("*", "itemref"), false, 0);
            }
            return view;
        }
        private static void ReplaceItemRef(XHtmlAction view, XmlNodeList list, bool isBreak, int loopCount)
        {

            //处理Shared目录下的节点替换。
            if (list != null && list.Count > 0)
            {
                if (loopCount > 50)
                {
                    throw new Exception("Reference loop : " + list[0].InnerXml);
                }
                string itemref = "itemref";
                for (int i = 0; i < list.Count; i++)
                {
                    string itemValue = list[i].Attributes[itemref].Value;
                    if (!string.IsNullOrEmpty(itemValue))
                    {
                        bool isOK = false;
                        string[] items = itemValue.Split('.');
                        if (items.Length == 1)// 只一个节点，从当前节点寻找。
                        {
                            XmlNode xNode = view.Get(items[0]);
                            if (xNode != null)
                            {
                                view.ReplaceNode(xNode, list[i]);
                                view.Remove(xNode);//从自己拿节点的，需要移除
                                isOK = true;
                            }
                        }
                        else
                        {
                            XHtmlAction sharedView = GetSharedView(items[0], view.FileName);//找到masterView
                            if (sharedView != null)
                            {
                                XmlNode xNode = sharedView.Get(items[1]);//找到被替换的节点


                                if (xNode != null)
                                {
                                    view.InsertAfter(xNode, list[i]);//先插入节点。
                                    XmlNodeList childNodeList = view.GetList("*", itemref, list[i].NextSibling);//检测内部是否有引用指向外部。
                                    if (childNodeList != null && childNodeList.Count > 0)
                                    {
                                        loopCount++;
                                        ReplaceItemRef(view, childNodeList, true, loopCount);//下次跳出，避免死循环。
                                    }
                                    view.Remove(list[i]);//移除节点
                                    isOK = true;
                                }
                            }

                        }
                        if (!isOK)
                        {
                            view.Remove(list[i]);//移除没有引用的节点
                            // view.RemoveAttr(list[i], itemref);
                        }
                    }
                }
                loopCount++;
                if (!isBreak)//避免死循环。
                {
                    ReplaceItemRef(view, view.GetList("*", itemref), isBreak, loopCount);
                }
            }
        }

        #region 处理Shared模板View
        /// <summary>
        /// 获取Shared文件View
        /// </summary>
        /// <param name="htmlName"></param>
        /// <returns></returns>
        private static XHtmlAction GetSharedView(string htmlName, string htmlPath)
        {
            string path = SharedPath + "/" + htmlName + ".html";
            if (!File.Exists(path))
            {
                path = null;
                string[] files = Directory.GetFiles(Path.GetDirectoryName(htmlPath), htmlName + ".html", SearchOption.AllDirectories);
                if (files != null && files.Length > 0)
                {
                    path = files[0];
                }
                else
                {
                    files = Directory.GetFiles(ViewsPath, htmlName + ".html", SearchOption.AllDirectories);
                    if (files != null && files.Length > 0)
                    {
                        path = files[0];
                    }
                }
                if (path == null)
                {
                    return null;
                }
            }
            XHtmlAction sharedView = new XHtmlAction(true, true);
            if (sharedView.Load(path, XmlCacheLevel.Day, true))
            {
                return sharedView;
            }

            return null;
        }
        #endregion
    }
}
