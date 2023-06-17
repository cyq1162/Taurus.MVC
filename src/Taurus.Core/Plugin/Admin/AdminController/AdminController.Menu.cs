using CYQ.Data.Tool;
using CYQ.Data.Table;
using System.Collections.Generic;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using System.Xml;
using System;

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
            string menuList = IO.Read(AdminConst.MenuPath);
            if (!string.IsNullOrEmpty(menuList))
            {
                menuTable = new MDataTable();
                menuTable.Columns.Add("MenuName,HostName,HostUrl");
                MDataTable dt = new MDataTable();
                dt.Columns.Add("MenuName");
                List<string> menus = new List<string>();
                string[] items = menuList.Split('\n');
                foreach (string item in items)
                {
                    string[] names = item.Split(',');
                    if (names.Length > 2)
                    {
                        menuTable.NewRow(true).Sets(0, names[0].Trim(), names[1].Trim(), names[2].Trim());
                    }
                    string name = names[0].Trim();
                    if (!menus.Contains(name.ToLower()))
                    {
                        menus.Add(name.ToLower());
                        dt.NewRow(true).Set(0, name);
                    }
                }
                View.OnForeach += View_OnForeach_Menu;
                dt.Bind(View, "menuList");
            }
        }

        private string View_OnForeach_Menu(string text, MDictionary<string, string> values, int rowIndex)
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
