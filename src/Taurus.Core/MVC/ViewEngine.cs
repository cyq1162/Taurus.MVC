using System;
using CYQ.Data.Xml;
using CYQ.Data;
using CYQ.Data.Tool;
using System.IO;
using System.Xml;
using Taurus.Mvc.Reflect;
using Taurus.Plugin.Admin;
using System.Runtime.Versioning;
using System.IO.Compression;
using Taurus.Plugin.Doc;
using System.Collections.Generic;
using System.Reflection;

namespace Taurus.Mvc
{
    /// <summary>
    /// 视图引擎
    /// </summary>
    public static partial class ViewEngine
    {
        private static string _ViewPath = string.Empty;
        internal static string ViewsPath
        {
            get
            {
                if (string.IsNullOrEmpty(_ViewPath))
                {
                    _ViewPath = AppConst.WebRootPath + MvcConfig.Views;
                    if (!Directory.Exists(_ViewPath))
                    {
                        _ViewPath = AppConst.WebRootPath + MvcConfig.Views.ToLower();//兼容Linux 文件夹小写
                    }
                }
                return _ViewPath;
            }
            set
            {
                //允许重置
                _ViewPath = value;
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

        private static Dictionary<string, string> keyPath = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        private static Dictionary<string, bool> apiPath = new Dictionary<string, bool>(StringComparer.OrdinalIgnoreCase);
        /// <summary>
        /// 创建视图对象
        /// </summary>
        public static XHtmlAction Create(string controlName, string actionName, bool isReadOnly)
        {
            string key = controlName + "-" + actionName;
            if (keyPath.ContainsKey(key))
            {
                return Create(keyPath[key], isReadOnly);
            }
            if (apiPath.ContainsKey(controlName))
            {
                return null;
            }
            if (Directory.Exists(ViewsPath))
            {
                string[] folders = Directory.GetDirectories(ViewsPath, "*", SearchOption.TopDirectoryOnly);
                bool hasFolder = false;
                foreach (string folder in folders)
                {
                    string foName = Path.GetFileNameWithoutExtension(folder);
                    if (string.Equals(controlName, foName, StringComparison.OrdinalIgnoreCase))
                    {
                        hasFolder = true;
                        string[] files = Directory.GetFiles(folder, "*.html", SearchOption.TopDirectoryOnly);
                        foreach (string file in files)
                        {
                            string fiName = Path.GetFileNameWithoutExtension(file);
                            if (string.Equals(actionName, fiName, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!keyPath.ContainsKey(fiName))
                                {
                                    try
                                    {
                                        keyPath.Add(key, file);
                                    }
                                    catch
                                    {

                                    }

                                }
                                return Create(file, isReadOnly);
                            }
                        }
                        break;
                    }
                }
                if (!hasFolder)
                {
                    if (!apiPath.ContainsKey(controlName))
                    {
                        try
                        {
                            apiPath.Add(controlName, true);
                        }
                        catch
                        {

                        }
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
        /// <param name="fullPath">相对路径，如：/abc/cyq/a.html</param>
        /// <param name="isReadOnly">页面内容是否只读</param>
        public static XHtmlAction Create(string fullPath, bool isReadOnly)
        {
            XHtmlAction view = new XHtmlAction(true, isReadOnly);
            if (view.Load(fullPath, XmlCacheLevel.Day, true))
            {
                // System.Web.HttpContext.Current.Response.Write("load ok");
                //处理Shared目录下的节点替换。
                if (!view.IsLoadFromCache)
                {
                    ReplaceItemRef(view, view.GetList("*", "itemref"), false, 0);
                    view.RefleshCache();
                }
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

    public static partial class ViewEngine
    {
        /// <summary>
        /// 解压 Admin、Doc Views 相关文件。
        /// </summary>
        private static void ZipViews()
        {
            try
            {
                string adminPath = ViewsPath + "/" + AdminConfig.HtmlFolderName;
                if (!Directory.Exists(adminPath))
                {
                    ZipTo(adminPath, Properties.Resources.admin);
                }
                string docPath = ViewsPath + "/" + DocConfig.HtmlFolderName;
                if (!Directory.Exists(docPath))
                {
                    ZipTo(docPath, Properties.Resources.doc);
                }

                string stylesPath = ViewsPath + "/styles";
                if (!Directory.Exists(stylesPath))
                {
                    ZipTo(stylesPath, Properties.Resources.styles);
                }
            }
            catch (Exception err)
            {
                Log.Write(err, LogType.Taurus);

            }

        }

        /// <summary>
        /// 预加载所有 Html 文件。
        /// </summary>
        internal static void InitViews()
        {
            if (!Directory.Exists(ViewsPath)) { return; }
            ZipViews();
            string[] folders = Directory.GetDirectories(ViewsPath, "*", SearchOption.TopDirectoryOnly);
            if (folders.Length == 0) { return; }
            var ctls = ControllerCollector.GetControllers(1);
            if (ctls == null || ctls.Count == 0) { return; }
            #region 加载 html 页面文件
            foreach (var ctl in ctls)
            {
                string cName = ctl.Key;
                foreach (string folder in folders)
                {
                    string foName = Path.GetFileNameWithoutExtension(folder);
                    if (string.Equals(cName, foName, StringComparison.OrdinalIgnoreCase))
                    {

                        string[] files = Directory.GetFiles(folder, "*.html", SearchOption.TopDirectoryOnly);
                        foreach (string file in files)
                        {
                            //只加载有对应方法的，无方法的可能是部分视图，不能单独加载，可能itemref会把自身内容给替换掉。
                            //预加载所有文件
                            string fiName = Path.GetFileNameWithoutExtension(file);
                            if (MethodCollector.GetMethod(ctl.Value.Type, fiName) != null)
                            {
                                Create(ctl.Key, fiName, false);//走这个方法，让其入缓存。
                            }
                        }
                        break;
                    }
                }
            }
            #endregion
        }
    }
}
