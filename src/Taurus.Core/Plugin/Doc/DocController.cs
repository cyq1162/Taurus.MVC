using System;
using System.Collections.Generic;
using System.IO;
using Taurus.Mvc;
using CYQ.Data;
using CYQ.Data.Xml;
using System.Xml;
using System.Reflection;
using CYQ.Data.Table;
using System.Data;
using Taurus.Mvc.Attr;
using Taurus.Plugin.MicroService;
using Taurus.Mvc.Reflect;

namespace Taurus.Plugin.Doc
{
    /// <summary>
    /// API文档生成及自动化测试接口
    /// </summary>
    internal partial class DocController : Controller
    {
        public DocController()
        {
            Init();
        }
        protected override string HtmlFolderName
        {
            get
            {
                return DocConfig.HtmlFolderName;
            }
        }

        public override void Default()
        {
            BindController();
            BindAction();
        }
        public void Detail()
        {
            BindController();
            BindDetail();
        }

        #region 绑定方法：BindController、BindDetail
       
        private void BindController()
        {
            ControllerTable.Bind(View);
        }
        private void BindAction()
        {
            string name = Query<string>("c");
            if (string.IsNullOrEmpty(name) && ControllerTable.Rows.Count > 0)
            {
                name = ControllerTable.Rows[0].Get<string>("CName");
            }
            if (View != null)
            {
                View.LoadData(ControllerTable.FindRow("CName='" + name + "'"), "");
                MDataTable dt = ActionTable.Select("CName='" + name + "'");
                string filter = Query<string>("f");
                if (!string.IsNullOrEmpty(filter))
                {
                    string where = string.Empty;
                    foreach (string item in filter.Split('|'))
                    {
                        if (where == string.Empty)
                        {
                            where = "ADesc like '%" + item + "%'";
                        }
                        else
                        {
                            where += " or ADesc like '%" + item + "%'";
                        }

                    }
                    dt = dt.Select(where);
                }
                if (dt != null)
                {
                    MCellStruct mc = new MCellStruct("Num", SqlDbType.NVarChar);
                    dt.Columns.Insert(0, mc);
                    int rowCount = dt.Rows.Count;
                    if (rowCount > 0)
                    {
                        int padLeft = rowCount.ToString().Length;
                        for (int i = 0; i < rowCount; i++)
                        {
                            string num = (i + 1).ToString();
                            dt.Rows[i].Set(0, num.PadLeft(padLeft, '0'));
                        }
                    }
                }
                dt.Bind(View);
            }
        }
        private void BindDetail()
        {
            Dictionary<string, string> dicReturn = new Dictionary<string, string>();
            //读取参数说明：
            XmlNode node = GetDescriptionNode(GetXml(), Query<string>("c") + "." + Query<string>("a"), "M:");
            if (node != null)
            {
                if (node.Name == "summary")
                {
                    node = node.ParentNode;
                }
                MDataTable dt = new MDataTable("Para");
                dt.Columns.Add("name,desc,required,type,value");
                XmlNodeList list = node.ChildNodes;
                if (node.ChildNodes.Count > 0 && (node.ChildNodes[0].InnerXml.Contains("<param") || node.ChildNodes[0].InnerXml.Contains("<returns")))
                {
                    list = node.ChildNodes[0].ChildNodes;
                }
                foreach (XmlNode item in list)
                {
                    switch (item.Name.ToLower())
                    {
                        case "returns":
                            if (!dicReturn.ContainsKey("returns"))
                            {
                                dicReturn.Add("returns", node.LastChild.InnerText.Trim());
                            }
                            break;
                        case "param":
                            string name = GetAttrValue(item, "name", "").ToLower();
                            string value = GetAttrValue(item, "value", Query<string>(name));
                            string type = GetAttrValue(item, "type", "string");
                            if (string.IsNullOrEmpty(type))
                            {

                            }
                            dt.NewRow(true).Set(0, name)
                                .Set(1, item.InnerText.Replace("\n", "<br />"))
                                .Set(2, GetAttrValue(item, "required", "false"))
                                .Set(3, type)
                                .Set(4, value);
                            break;
                    }
                }
                string[] attrs = Query<string>("p", "").Split(' ', '[', ']');
                foreach (string attr in attrs)
                {
                    if (!string.IsNullOrEmpty(attr))
                    {
                        string name = attr.ToLower();

                        if (name == "get" || name == "post" || name == "head" || name == "put" || name == "delete")
                        {
                            View.Set("httpType", name.ToUpper());
                            continue;
                        }
                        string value = Query<string>(attr);
                        if (name == "microservice")
                        {
                            value = MsConfig.Server.RcKey;
                        }

                        dt.NewRow(true, 0).Set(0, name)
                                .Set(1, name)
                                .Set(2, true)
                                .Set(3, "header")
                                .Set(4, value);
                    }

                }
                string[] paras = DocConfig.DefaultParas.Split(',');
                if (paras.Length > 0)
                {
                    foreach (string para in paras)
                    {
                        if (!string.IsNullOrEmpty(para))
                        {
                            string name = para.ToLower();
                            if (dt.FindRow("name='" + name + "'") == null)
                            {
                                dt.NewRow(true, 0).Set(0, name)
                                       .Set(1, name)
                                       .Set(2, true)
                                       .Set(3, "header")
                                       .Set(4, Query<string>(para));
                            }
                        }
                    }
                }
                if (dicReturn.Count > 0)
                {
                    View.LoadData(dicReturn);
                }
                if (dt.Rows.Count > 0)
                {
                    dt.Bind(View);
                }
            }
        }
        private string GetAttrValue(XmlNode node, string attrName)
        {
            return GetAttrValue(node, attrName, "");
        }
        private string GetAttrValue(XmlNode node, string attrName, string defaultValue)
        {
            if (node.Attributes[attrName] != null)
            {
                return node.Attributes[attrName].Value;
            }
            return defaultValue;
        }
       
        #endregion

    }
    internal partial class DocController
    {
        #region 对外提供调用Init方法
        static MDataTable ControllerTable, ActionTable;
        static readonly object o = new object();
        /// <summary>
        /// 初始化全局：ControllerTable、ActionTable
        /// </summary>
        internal void Init()
        {
            if (ControllerTable == null)
            {
                lock (o)
                {
                    if (ControllerTable == null)
                    {
                        InitController();
                        InitAction();
                    }
                }
            }
        }
        #endregion

        #region 全局：初始化


        List<XHtmlAction> actions = null;
        private List<XHtmlAction> GetXml()
        {
            if (actions == null)
            {
                actions = new List<XHtmlAction>();
                List<Assembly> assList = AssemblyCollector.ControllerAssemblyList;
                foreach (Assembly ass in assList)
                {
                    string dllName = ass.GetName().Name;
                    string xmlPath = string.Empty;
                    if (dllName.Contains(".dll"))
                    {
                        xmlPath = dllName.Replace(".dll", ".xml");
                    }
                    else
                    {
                        xmlPath = dllName + ".xml";
                    }
                    if (xmlPath[0] != '/' && !xmlPath.Contains(":")) //非 linux 或 window 完整路径
                    {
                        xmlPath = AppConst.AssemblyPath + xmlPath;
                    }
                    if (File.Exists(xmlPath))
                    {
                        XHtmlAction action = new XHtmlAction(false, true);
                        if (action.Load(xmlPath))
                        {
                            actions.Add(action);
                        }
                        else
                        {
                            action.Dispose();
                        }
                    }
                }
            }
            return actions;
        }
        private string GetDescription(List<XHtmlAction> actions, string name, string type)
        {
            XmlNode node = GetDescriptionNode(actions, name, type);
            if (node != null && node.ChildNodes.Count > 0)
            {
                return node.ChildNodes[0].InnerText.Trim();
            }
            return "";
        }
        private XmlNode GetDescriptionNode(List<XHtmlAction> actions, string name, string type)
        {
            if (actions.Count > 0)
            {
                foreach (XHtmlAction action in actions)
                {
                    XmlNode node = action.Get(type + name);
                    if (node != null && node.ChildNodes.Count > 0)
                    {
                        return node.ChildNodes[0];
                    }
                    else
                    {
                        XmlNodeList list = action.GetList("member", "name");
                        if (list != null)
                        {
                            foreach (XmlNode item in list)
                            {
                                if (item.Attributes["name"].Value.StartsWith(type + name.Split('(')[0] + "("))
                                {
                                    return item;
                                }
                            }
                        }
                    }
                }
            }
            return null;
        }


        private void InitController()
        {
            if (ControllerTable == null)
            {
                ControllerTable = new MDataTable("Controller");
                ControllerTable.Columns.Add("CName,CDesc,TokenFlag");
                ControllerTable.Columns.Add("Type", SqlDbType.Variant);

                //搜集参数
                Dictionary<string, TypeEntity> cType = ControllerCollector.GetControllers(2);
                foreach (KeyValuePair<string, TypeEntity> item in cType)
                {
                    try
                    {
                        Type type = item.Value.Type;
                        string fullName = type.FullName;
                        //过滤插件控制器，插件只有一级，不用过滤。
                        if (fullName.EndsWith(ReflectConst.GlobalController))
                        {
                            continue;
                        }
                        // || fullName.EndsWith(ReflectConst.DocController) || fullName.EndsWith(ReflectConst.LogController)
                        //else if (fullName.EndsWith(ReflectConst.MicroServiceController) && !MicroService.MsConfig.IsServer)
                        //{
                        //    continue;
                        //}
                        var xmlList = GetXml();
                        string desc = GetDescription(xmlList, type.FullName, "T:").Trim();
                        if (!string.IsNullOrEmpty(desc))
                        {
                            ControllerTable.NewRow(true)
                              .Set(0, type.FullName)
                              .Set(1, desc)
                              .Set(2, type.GetCustomAttributes(typeof(TokenAttribute), false).Length)
                              .Set(3, type);
                        }
                    }
                    catch (Exception err)
                    {


                    }
                }
            }
        }
        private void InitAction()
        {
            if (ActionTable == null)
            {
                ActionTable = new MDataTable("Action");
                ActionTable.Columns.Add("CName,AName,Attr,Url,ADesc");
                for (int i = 0; i < ControllerTable.Rows.Count; i++)
                {
                    MDataRow row = ControllerTable.Rows[i];

                    Type type = row.Get<Type>("Type");

                    MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
                    bool hasMehtod = false;
                    #region 处理
                    foreach (MethodInfo method in methods)
                    {
                        switch (method.Name)
                        {
                            case ReflectConst.BeforeInvoke:
                            case ReflectConst.EndInvoke:
                            case ReflectConst.CheckToken:
                            case ReflectConst.Default:
                                continue;
                        }
                        hasMehtod = true;
                        string attrText = "";
                        #region 属性处理

                        object[] attrs = method.GetCustomAttributes(true);
                        bool methodHasToken = false, methodHasAck = false, methodHasMicroService = false;
                        foreach (object attr in attrs)
                        {
                            string attrName = attr.GetType().Name;
                            if (attrName.StartsWith("Http"))
                            {
                                attrText += "[" + attrName.Replace("Attribute", "] ").Replace("Http", "").ToLower();
                            }
                            else if (attrName == ReflectConst.TokenAttribute)
                            {
                                methodHasToken = true;
                            }
                            else if (attrName == ReflectConst.AckAttribute)
                            {
                                methodHasAck = true;
                            }
                            else if (attrName == ReflectConst.MicroServiceAttribute)
                            {
                                methodHasMicroService = true;
                            }
                        }
                        if (string.IsNullOrEmpty(attrText))
                        {
                            attrText = "[get] ";
                        }
                        if (methodHasToken || row.Get<bool>("TokenFlag"))
                        {
                            attrText += "[token]";
                        }
                        if (methodHasAck)
                        {
                            attrText += "[ack]";
                        }
                        if (methodHasMicroService)
                        {
                            attrText += "[microservice]";
                        }
                        #endregion

                        string url = "";
                        #region Url
                        url = "/" + type.Name.Replace("Controller", "").ToLower() + "/" + method.Name.ToLower();
                        if (MvcConfig.RouteMode == 2)
                        {
                            string[] items = type.FullName.Split('.');
                            string module = items[items.Length - 2];
                            url = "/" + module.ToLower() + url;
                        }
                        #endregion
                        string desc = "";
                        #region 描述
                        string name = type.FullName + "." + method.Name;
                        ParameterInfo[] paras = method.GetParameters();
                        if (paras.Length > 0)
                        {
                            name += "(";
                            foreach (ParameterInfo para in paras)
                            {
                                name += para.ParameterType.FullName + ",";
                            }
                            name = name.TrimEnd(',') + ")";
                        }
                        desc = GetDescription(actions, name, "M:");
                        #endregion
                        if (!string.IsNullOrEmpty(desc))
                        {
                            ActionTable.NewRow(true)
                            .Set(0, row.Get<string>("CName"))
                            .Set(1, method.Name)
                            .Set(2, attrText)
                            .Set(3, url)
                            .Set(4, desc);
                        }
                    }
                    #endregion
                    if (!hasMehtod)
                    {
                        //remove 
                        ControllerTable.Rows.RemoveAt(i);
                        i--;
                        continue;
                    }
                }
            }
        }


        



        #endregion
    }

}
