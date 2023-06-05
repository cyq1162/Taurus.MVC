using CYQ.Data.Tool;
using CYQ.Data.Table;
using System.Collections.Generic;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using System.Xml;

namespace Taurus.Plugin.Admin
{

    /// <summary>
    /// 设置 - 按钮事件：账号、IP黑名单、手工添加微服务客户端
    /// </summary>
    internal partial class AdminController
    {
        #region 页面呈现

        public void Setting() { }

        private MDataTable menuTable;
        public void Menu()
        {
            string menuList = IO.Read(AdminConst.MenuAddPath);
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


        public void SettingOfAccount()
        {
            if (!IsHttpPost)
            {
                string[] items = IO.Read(AdminConst.AccountPath).Split(',');
                string name = items.Length == 2 ? items[0] : "user";
                View.KeyValue.Set("UserName", name);
            }
        }


        public void SettingOfHostAdd()
        {
            if (!IsHttpPost)
            {
                if (MsConfig.IsRegCenterOfMaster)
                {
                    string hostList = IO.Read(AdminConst.HostAddPath);
                    View.KeyValue.Add("HostList", hostList);
                }
                else
                {
                    View.Set("btnAddHost", CYQ.Data.Xml.SetType.Disabled, "true");
                    View.Set("hostList", CYQ.Data.Xml.SetType.Disabled, "true");
                }
            }
        }

        public void SettingOfMenuAdd()
        {
            if (!IsHttpPost)
            {
                string menuList = IO.Read(AdminConst.MenuAddPath);
                View.KeyValue.Add("MenuList", menuList);
            }
        }
        #endregion

        #region 页面点击事件

        /// <summary>
        /// 添加管理员2账号
        /// </summary>
        public void BtnSaveAccount()
        {
            string uid = Query<string>("uid");
            string pwd = Query<string>("pwd");
            string pwdAgain = Query<string>("pwdAgain");
            if (string.IsNullOrEmpty(uid) || string.IsNullOrEmpty(pwd))
            {
                View.Set("msg", "User or passowrd can't be empty.");
            }
            else if (pwd != pwdAgain)
            {
                View.Set("msg", "Password must be same.");
            }
            if (uid.Contains(",") || pwd.Contains(","))
            {
                View.Set("msg", "User or password can't contain ','.");
            }
            else
            {
                IO.Write(AdminConst.AccountPath, uid + "," + pwd);
                View.Set("msg", "Save success.");
            }

        }

        /// <summary>
        /// 删除管理员2账号
        /// </summary>
        public void btnDeleteAccount()
        {
            IO.Delete(AdminConst.AccountPath);
            View.Set("msg", "Delete success.");
        }

        /// <summary>
        /// 添加注册主机
        /// </summary>
        public void BtnAddHost()
        {
            if (!MsConfig.IsRegCenterOfMaster)
            {
                View.Set("msg", "Setting only for register center of master.");
                return;
            }

            string hostList = Query<string>("hostList");
            Server.RegCenter.AddHostByAdmin(hostList);
            IO.Write(AdminConst.HostAddPath, hostList);
            View.KeyValue.Add("HostList", hostList);
            View.Set("msg", "Save success.");
        }
        /// <summary>
        /// 添加IP黑名单
        /// </summary>
        public void SettingOfIpBlackname()
        {
            if (!IsHttpPost)
            {
                string ipList = IO.Read(AdminConst.IPBlacknamePath);
                View.KeyValue.Add("IPList", ipList);
            }
        }
        /// <summary>
        /// 添加黑名单
        /// </summary>
        public void BtnAddIPBlackname()
        {
            string ipList = Query<string>("ipList");
            IPLimit.ResetIPList(ipList);
            LimitConfig.IP.IsSync = false;//手工保存后，重启服务前不再与注册同心保持同步。
            View.KeyValue.Add("IPList", ipList);
            View.Set("msg", "Save success.");
        }

        /// <summary>
        /// 添加自定义菜单
        /// </summary>
        public void BtnAddMenu()
        {
            string menuList = Query<string>("menuList");
            IO.Write(AdminConst.MenuAddPath, menuList);
            View.KeyValue.Add("MenuList", menuList);
            View.Set("msg", "Save success.");
        }


        #endregion
    }
}
