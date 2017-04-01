using System;
using System.Collections.Generic;
using System.Text;
using CYQ.Data.Xml;
using CYQ.Data;
using System.IO;
using System.Xml;
namespace Taurus.Core
{
    /// <summary>
    /// 视图引擎
    /// </summary>
    public static class ViewEngine
    {
        /// <summary>
        /// 创建视图对象
        /// </summary>
        public static XHtmlAction Create(string controlName, string actionName)
        {
            string path = AppConfig.GetApp("Views", "Views") + "/"
                          + controlName.Replace(InvokeLogic.Controller, "")
                          + "/" + actionName + ".html";
            return Create(path);
        }
        /// <summary>
        /// 创建视图对象
        /// </summary>
        /// <param name="path">相对路径，如：/abc/cyq/a.html</param>
        public static XHtmlAction Create(string path)
        {
            path = AppDomain.CurrentDomain.BaseDirectory + path.TrimStart('/').Replace("/", "\\");
            if (File.Exists(path))
            {
                XHtmlAction view = new XHtmlAction(true, false);

                if (view.Load(path, XmlCacheLevel.Hour, true))
                {
                    //处理Shared目录下的节点替换。
                    ReplaceItemRef(view, view.GetList("*", "itemref"), false, 0);
                }
                return view;
            }
            return null;
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
                            XHtmlAction sharedView = GetSharedView(items[0]);//找到masterView
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
        static Dictionary<string, XHtmlAction> sharedViews = new Dictionary<string, XHtmlAction>();
        /// <summary>
        /// 获取Shared文件View
        /// </summary>
        /// <param name="htmlName"></param>
        /// <returns></returns>
        private static XHtmlAction GetSharedView(string htmlName)
        {
            string path = AppDomain.CurrentDomain.BaseDirectory + AppConfig.GetApp("Views", "Views") + "\\shared\\" + htmlName + ".html";
            if (!File.Exists(path))
            {
                return null;
            }
            XHtmlAction sharedView = null;
            string key = path.GetHashCode().ToString();
            if (sharedViews.ContainsKey(key))
            {
                sharedView = sharedViews[key];
                if (sharedView.IsXHtmlChanged)
                {
                    sharedViews.Remove(key);
                    sharedView = null;
                }
                else
                {
                    return sharedView;
                }
            }
            sharedView = new XHtmlAction(true, true);
            if (sharedView.Load(path, XmlCacheLevel.Day, true))
            {
                sharedViews.Add(key, sharedView);
                return sharedView;
            }
            return null;
        }
        #endregion
    }
}
