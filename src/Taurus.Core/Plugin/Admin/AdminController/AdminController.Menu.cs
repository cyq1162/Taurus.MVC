using CYQ.Data.Tool;
using CYQ.Data.Table;
using System.Collections.Generic;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using System.Xml;
using System;
using Taurus.Mvc;

namespace Taurus.Plugin.Admin
{

    /// <summary>
    /// 设置 - 按钮事件：账号、IP黑名单、手工添加微服务客户端
    /// </summary>
    internal partial class AdminController
    {
        #region 页面呈现

        private MDataTable menuTable;
        /// <summary>
        /// Ext - Menu 展示
        /// </summary>
        public void Menu()
        {

            menuTable = new MDataTable();
            menuTable.Columns.Add("MenuName,HostName,HostUrl");
            MDataTable dtGroup = new MDataTable();
            dtGroup.Columns.Add("MenuName");

            List<string> groupNames = new List<string>();

            #region 加载自定义菜单 - 来自代码 - 临时内存
            var menus = AdminAPI.ExtMenu.menuList;
            if (menus.Count > 0)
            {
                List<string> keys = menus.GetKeys();
                foreach (string key in keys)
                {
                    string[] keyvalue = key.Split(',');
                    if (!groupNames.Contains(keyvalue[0].ToLower()))
                    {
                        groupNames.Add(keyvalue[0].ToLower());
                        dtGroup.NewRow(true).Set(0, keyvalue[0]);
                    }

                    menuTable.NewRow(true).Sets(0, keyvalue[0].Trim(), keyvalue[1].Trim(), menus[key].Trim());
                }
            }

            #endregion

            #region 加载自定义菜单 - 来自配置 - 持久化
            string menuText = AppDataIO.Read(AdminConst.MenuPath);
            if (!string.IsNullOrEmpty(menuText))
            {
                string[] items = menuText.Split('\n');
                foreach (string item in items)
                {
                    string[] names = item.Split(',');
                    if (names.Length > 2)
                    {
                        menuTable.NewRow(true).Sets(0, names[0].Trim(), names[1].Trim(), names[2].Trim());
                    }
                    string name = names[0].Trim();
                    if (!groupNames.Contains(name.ToLower()))
                    {
                        groupNames.Add(name.ToLower());
                        dtGroup.NewRow(true).Set(0, name);
                    }
                }
            }
            #endregion
            View.OnForeach += View_OnForeach_Menu;
            dtGroup.Bind(View, "menuList");
        }

        private string View_OnForeach_Menu(string text, Dictionary<string, string> values, int rowIndex)
        {
            string menu = values["MenuName"];
            if (!string.IsNullOrEmpty(menu))
            {
                //循环嵌套：1-获取子数据
                MDataTable dt = menuTable.FindAll("MenuName='" + menu + "'");
                if (dt != null && dt.Rows.Count > 0)
                {
                    //循环嵌套：2 - 转为节点
                    XmlNode xmlNode = View.CreateNode("div", text);
                    //循环嵌套：3 - 获取子节点，以便进行循环
                    XmlNode hostNode = View.Get("hostList", xmlNode);
                    if (hostNode != null)
                    {
                        //循环嵌套：4 - 子节点，循环绑定数据。
                        View.SetForeach(dt, hostNode, hostNode.InnerXml, null);
                        //循环嵌套：5 - 返回整个节点的内容。
                        return xmlNode.InnerXml;
                    }
                }
            }

            return text;
        }

        #endregion

    }
}
