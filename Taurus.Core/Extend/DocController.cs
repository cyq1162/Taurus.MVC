using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using CYQ.Data;
using CYQ.Data.Xml;
using System.Xml;
using System.Reflection;
using CYQ.Data.Tool;
using CYQ.Data.Table;
namespace Taurus.Core
{
    /// <summary>
    /// API文档生成及自动化测试接口
    /// </summary>
    public partial class DocController : Controller
    {
        public override void Default()
        {
            LoadController();
            LoadAction();
        }
        public void Detail()
        {
            LoadController();
            LoadDetail();
        }

        /// <summary>
        /// 记录请求 ( 先不开 启)
        /// </summary>
        //public static void Record(IController controller, string methodName)
        //{
        //    if (controller.View == null)
        //    {
        //        string json = controller.APIResult;
        //        if (!string.IsNullOrEmpty(json) && json.Contains("success\":true"))
        //        {
        //            //遍历请求头
        //            StringBuilder rqHeader = new StringBuilder();
        //            rqHeader.AppendLine(controller.Request.HttpMethod + " " + controller.Request.RawUrl + "<hr/>");
        //            foreach (string key in controller.Request.Headers.AllKeys)
        //            {
        //                rqHeader.AppendLine(key + ":" + controller.Request.Headers[key]);
        //            }
        //            rqHeader.AppendLine("");
        //            rqHeader.AppendLine(JsonHelper.ToJson(controller.Request.Form));
        //            rqHeader.AppendLine("<hr />");
        //            rqHeader.AppendLine(json);

        //            controller.Write(rqHeader.ToString());
        //            //写入文件中。
        //        }

        //    }
        //}
    }
    public partial class DocController
    {
        List<XHtmlAction> actions = null;
        private List<XHtmlAction> GetXml()
        {
            if (actions == null)
            {
                actions = new List<XHtmlAction>();
                string[] dllNames = ("Taurus.Core," + InvokeLogic.DllNames).Split(',');
                foreach (string dll in dllNames)
                {
                    if (File.Exists(AppConfig.AssemblyPath + dll + ".XML"))
                    {
                        XHtmlAction action = new XHtmlAction(false, true);
                        if (action.Load(AppConfig.AssemblyPath + dll + ".XML"))
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
            if (node != null)
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
                    if (node != null)
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
                                if (item.Attributes["name"].Value.StartsWith(type + name + "("))
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

        string name, description;
        Type type;
        private void LoadController()
        {
            name = Query<string>("c");

            MDictionary<string, string> cDescrption = new MDictionary<string, string>();
            //搜集参数
            Dictionary<string, Type> cType = InvokeLogic.GetControllers();
            foreach (KeyValuePair<string, Type> item in cType)
            {
                switch (item.Value.Name)
                {
                    case InvokeLogic.Const.DefaultController:
                    case InvokeLogic.Const.DocController:
                        continue;
                }
                string desc = GetDescription(GetXml(), item.Value.FullName, "T:").Trim();
                if (string.IsNullOrEmpty(name) || name == item.Value.FullName)
                {
                    name = item.Value.FullName;
                    description = desc;
                    type = item.Value;
                }
                cDescrption.Add(item.Value.FullName, desc);
            }
            MDataTable.CreateFrom(cDescrption).Bind(View, "lbControllers");
        }
        private void LoadAction()
        {
            if (type == null) { return; }
            bool controllerHasToken = type.GetCustomAttributes(typeof(TokenAttribute), false).Length > 0;
            View.Set("labName", SetType.InnerText, name);
            View.Set("labDescription", SetType.InnerText, description);
            MethodInfo[] methods = type.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly);
            if (methods.Length > 0)
            {
                MDataTable dt = new MDataTable();
                dt.Columns.Add("attr,url,desc,name,fullname");
                foreach (MethodInfo method in methods)
                {
                    switch (method.Name)
                    {
                        case InvokeLogic.Const.BeforeInvoke:
                        case InvokeLogic.Const.EndInvoke:
                        case InvokeLogic.Const.CheckToken:
                        case InvokeLogic.Const.Default:
                            continue;
                    }
                    if (dt.FindRow("name='" + method.Name + "'") != null)
                    {
                        continue;
                    }
                    MDataRow row = dt.NewRow(true);

                    #region 属性处理
                    string attrText = "";
                    object[] attrs = method.GetCustomAttributes(true);
                    bool methodHasToken = false;
                    foreach (object attr in attrs)
                    {
                        if (attr.GetType().Name.StartsWith("Http"))
                        {
                            attrText += "[" + attr.GetType().Name.Replace("Attribute", "] ").Replace("Http", "").ToLower();
                        }
                        else if (attr.GetType().Name == InvokeLogic.Const.TokenAttribute)
                        {
                            methodHasToken = true;
                        }
                    }
                    if (string.IsNullOrEmpty(attrText))
                    {
                        attrText = "[get] ";
                    }
                    if (methodHasToken || controllerHasToken)
                    {
                        attrText += "[token]";
                    }
                    row.Set(0, attrText);
                    #endregion

                    #region Url
                    row.Set(1, "/" + type.Name.Replace("Controller", "").ToLower() + "/" + method.Name.ToLower());
                    #endregion

                    #region 描述
                    name = type.FullName + "." + method.Name;
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
                    description = GetDescription(actions, name, "M:");
                    row.Set(2, description);
                    #endregion

                    row.Set(3, method.Name);
                    row.Set(4, type.FullName);
                }
                dt.Bind(View, "labMethods");
            }
        }
        private void LoadDetail()
        {
            Dictionary<string, string> dicReturn = new Dictionary<string, string>();
            Dictionary<string, string> dicParas = new Dictionary<string, string>();
            //string[] items = Query<string>("c", "").Split('.');
            ////设置标题
            //string url = items[items.Length - 1].Replace("Controller", "").ToLower() + "/" + Query<string>("a", "").ToLower();
            //dic.Add("url", url);
            //View.LoadData(dic, "");
            //读取参数说明：
            XmlNode node = GetDescriptionNode(GetXml(), Query<string>("c") + "." + Query<string>("a"), "M:");
            if (node != null)
            {
                foreach (XmlNode item in node.ChildNodes)
                {
                    switch(item.Name.ToLower())
                    {
                        case "returns":
                            dicReturn.Add("returns", node.LastChild.InnerText.Trim());
                            break;
                        case "param":
                            dicParas.Add(item.Attributes["name"].Value, item.InnerText);
                            break;
                    }
                }
                if (dicReturn.Count > 0)
                {
                    View.LoadData(dicReturn, "");
                }
                if (dicParas.Count > 0)
                {
                    MDataTable.CreateFrom(dicParas).Bind(View, "labParas");
                }
            }
        }
    }
}
