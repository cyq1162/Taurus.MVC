using CYQ.Data.Tool;
using CYQ.Data.Table;
using System.Collections.Generic;
using Taurus.Plugin.MicroService;
using Taurus.Plugin.Limit;
using System.Xml;
using System;
using CYQ.Data.Xml;
using Taurus.Mvc;

namespace Taurus.Plugin.Admin
{

    /// <summary>
    /// 设置 - 按钮事件：账号、IP黑名单、手工添加微服务客户端
    /// </summary>
    internal partial class AdminController
    {
        #region 页面呈现

        /// <summary>
        /// 配置 - 主界面
        /// </summary>
        public void Setting() { }

        public void SettingOfAccount()
        {
            if (!IsHttpPost)
            {
                string[] items = AppDataIO.Read(AdminConst.AccountPath).Split(',');
                string name = items.Length == 2 ? items[0] : "user";
                View.KeyValue.Add("UserName", name);
            }
        }

        public void SettingOfMenu()
        {
            if (!IsHttpPost)
            {
                string menuList = AppDataIO.Read(AdminConst.MenuPath);
                View.KeyValue.Add("MenuList", menuList);
            }
        }

        /// <summary>
        /// 添加 - 主机 - 同步【to all ends】
        /// </summary>
        public void SettingOfHostSync()
        {
            if (!IsHttpPost)
            {
                if (MsConfig.IsRegistryCenterOfMaster)
                {
                    string hostList = AppDataIO.Read(AdminConst.HostSyncPath);
                    View.KeyValue.Add("HostList", hostList);
                }
                else
                {
                    View.Set("btnAddHostSync", SetType.Disabled, "true");
                    View.Set("hostList", "# Current type is not registry center of master.");
                    View.Set("hostList", SetType.Disabled, "true");
                }
            }
        }

        /// <summary>
        /// 添加 - IP黑名 - 同步【to all servers】
        /// </summary>
        public void SettingOfIPSync()
        {
            if (!IsHttpPost)
            {
                if (LimitConfig.IP.IsEnable)
                {
                    string ipList = AppDataIO.Read(AdminConst.IPSyncPath);
                    View.KeyValue.Add("IPList", ipList);
                }
                else
                {
                    View.Set("btnAddIPSync", SetType.Disabled, "true");
                    View.Set("ipList", "# Current Limit.IP.IsEnable = false");
                    View.Set("ipList", SetType.Disabled, "true");
                }

            }
        }

        /// <summary>
        /// 添加 - 配置 - 同步【to all clients】
        /// </summary>
        public void SettingOfConfigSync()
        {
            if (!IsHttpPost)
            {
                string configList = AppDataIO.Read(AdminConst.ConfigSyncPath);
                View.Set("isDurable", SetType.Checked, configList.StartsWith("#durable\n").ToString());
                if (MsConfig.IsRegistryCenterOfMaster)
                {
                    View.KeyValue.Add("configList", configList.Replace("#durable\n", ""));
                }
                else
                {
                    View.Set("btnAddConfigSync", SetType.Disabled, "true");
                    View.Set("configList", SetType.Disabled, "true");
                    View.Set("isDurable", SetType.Disabled, "true");
                    string tip = "# Current type is not registry center of master.\n";
                    if (MsConfig.IsClient)
                    {
                        tip += "# Current MicroService.Client.IsAllowSyncConfig = " + MsConfig.Client.IsAllowSyncConfig.ToString().ToLower() + "\n";
                    }
                    View.Set("configList", tip + configList);
                }

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
                AppDataIO.Write(AdminConst.AccountPath, uid + "," + pwd);
                View.Set("msg", "Save success.");
            }

        }

        /// <summary>
        /// 删除管理员2账号
        /// </summary>
        public void btnDeleteAccount()
        {
            AppDataIO.Delete(AdminConst.AccountPath);
            View.Set("msg", "Delete success.");
        }

        /// <summary>
        /// 添加 - 自定义菜单
        /// </summary>
        public void BtnAddMenu()
        {
            string menuList = Query<string>("menuList");
            AppDataIO.Write(AdminConst.MenuPath, menuList);
            View.KeyValue.Add("MenuList", menuList);
            View.Set("msg", "Save success.");
        }


        /// <summary>
        /// 添加 - 同步 - 注册主机
        /// </summary>
        public void BtnAddHostSync()
        {
            if (!MsConfig.IsRegistryCenterOfMaster)
            {
                View.Set("msg", "Setting only for registry center of master.");
                return;
            }

            string hostList = Query<string>("hostList");
            Server.RegistryCenter.AddHostByAdmin(hostList);
            AppDataIO.Write(AdminConst.HostSyncPath, hostList);
            View.KeyValue.Add("HostList", hostList);
            View.Set("msg", "Save success.");
        }

        /// <summary>
        /// 添加 - 同步 - 黑名单
        /// </summary>
        public void BtnAddIPSync()
        {
            string ipList = Query<string>("ipList");
            Server.SyncIPTime = DateTime.Now;
            IPLimit.ResetIPList(ipList, true);
            if (!MsConfig.IsRegistryCenterOfMaster)
            {
                //手工保存后，重启服务前不再与注册同心保持同步。
                MsConfig.Server.IsAllowSyncIP = false;
            }
            View.KeyValue.Add("IPList", ipList);
            View.Set("msg", "Save success.");
        }


        /// <summary>
        /// 添加 - 同步 - 同步配置
        /// </summary>
        public void BtnAddConfigSync()
        {
            if (!MsConfig.IsRegistryCenterOfMaster)
            {
                View.Set("msg", "Setting only for registry center of master.");
                return;
            }
            bool isDurable = Query<bool>("isDurable");
            string configList = Query<string>("configList");

            Server.SyncConfigTime = DateTime.Now;
            AppDataIO.Write(AdminConst.ConfigSyncPath, (isDurable ? "#durable\n" : "") + configList);
            View.KeyValue.Add("ConfigList", configList);
            View.Set("isDurable", SetType.Checked, isDurable.ToString());
            View.Set("msg", "Save success.");
        }
        #endregion
    }
}
