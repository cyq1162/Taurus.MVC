using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Mvc;
using CYQ.Data;
using CYQ.Data.Table;
using CYQ.Data.Xml;
using CYQ.Data.Tool;
using System.Web;
using System.IO;
using Taurus.Logic;
using System.Runtime;

namespace Taurus.Controllers
{
    #region CodeFirst的数据表
    public class Connection
    {
        public const string TxtConn = "txt path={0}App_Data\\txtdb";
        public const string XmlConn = "xml path={0}App_Data\\txtdb";
        public const string Conn = "Conn";
    }

    public class Users : CYQ.Data.Orm.OrmBase
    {
        public Users()
        {
            base.SetInit(this, "users", Connection.TxtConn);
        }
        private int _ID;

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        private string _Name;

        public string Name
        {
            get { return _Name; }
            set { _Name = value; }
        }
        private bool _IsEnabled;

        public bool IsEnabled
        {
            get { return _IsEnabled; }
            set { _IsEnabled = value; }
        }
        private int _Sex;

        public int Sex
        {
            get { return _Sex; }
            set { _Sex = value; }
        }
        private string _HeadImgUrl;

        public string HeadImgUrl
        {
            get { return _HeadImgUrl; }
            set { _HeadImgUrl = value; }
        }
        private int? _UserType;

        public int? UserType
        {
            get { return _UserType; }
            set { _UserType = value; }
        }

        private DateTime _CreateTime;

        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

    }
    public class UserType : CYQ.Data.Orm.OrmBase
    {
        public UserType()
        {
            base.SetInit(this, "UserType", Connection.TxtConn);
        }
        private int _ID;

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        private string _TypeName;

        public string TypeName
        {
            get { return _TypeName; }
            set { _TypeName = value; }
        }

    }

    #endregion

    public class DemoController : Controller
    {


        #region Controller方法
        public override void Default()
        {
            if (IsHttpGet)
            {
                #region 查询并绑定下拉列表
                using (UserType ut = new UserType())
                {
                    ut.Select().Bind(View, "ddl" + ut.BaseInfo.TableName); ;
                }

                // View.OnForeach += new XHtmlAction.SetForeachEventHandler(View_OnForeach);
                // utTable.Bind(View);//取usertypeView或defaultView节点。

                // utTable.Bind(View, "ddl" + utTable.TableName);//绑定下拉框，指定节点名称。（用表名，是为了不写死ddlUserType）
                #endregion

                #region 根据条件ID查询，并绑定到html中。
                using (Users demo = new Users())
                {
                    if (demo.Fill())//不传参，使用自动取值
                    {
                        AppConfig.UI.AutoPrefixs = "txt,ddl,chb";
                        demo.UI.SetToAll(View);//自动填充输入框的值

                        View.LoadData(demo, "");//加载值，用于右侧CMS标签显示值
                    }
                }
                #endregion

                #region 分页查询列表，并显示列表数据

                //定义分页控制
                Pager pager = new Pager(View) { PageSize = 2 };//每页2条，以便显示分页

                MDataTable dt;
                //UI 操作View
                using (Users demo = new Users())
                {
                    dt = demo.Select(pager.PageIndex, pager.PageSize);//分页查询数据
                    pager.Bind(dt.RecordsAffected);//绑定分页控件，只需要记录总数。
                }

                #region 进行数据表关联
                dt.JoinOnName = "UserType";
                dt.Conn = Connection.XmlConn;//这里玩的花了一点（Users表是txt数据库，UserType是xml数据库）
                dt = dt.Join("UserType", "ID", "TypeName");
                #endregion

                #region 列表数据绑定到页面
                View.OnForeach += new XHtmlAction.SetForeachEventHandler(View_OnForeach);//formater
                dt.Bind(View);//取UsersView或defaultView节点。
                #endregion

                #endregion

            }
            //if (IsHttpPost)
            //{
            //    BtnEvent();
            //}

            //View 操作 UI
            //View.LoadData(ut.Select<UserType>());
            //View.SetForeach();

            // View.LoadData(dt.Rows[dt.Rows.Count - 1], "txt");// 自动格式化标签。

            //View.LoadData(dt);//装载数据
            //
            //View.SetForeach("divView2", SetType.InnerXml);
            //View.SetForeach("divView3", "自定义：{0} -${Name} -{2}<br />");



        }
        public void Show()
        {

        }
        #region 按钮事件
        public void BtnAdd()
        {

            using (Users u = new Users())
            {
                string path = SavePic();
                if (path != null)
                {
                    u.HeadImgUrl = path;
                }
                if (u.Insert(true))
                {
                    Reflesh(u.ID);
                }
            }
        }
        public void BtnUpdate()
        {
            using (Users u = new Users())
            {

                //u.LoadFrom(true);
                //Users u2 = u.RawData.ToEntity<Users>();
                //string name = u.Name;
                //u.Name = "111";
                string path = SavePic();
                if (path != null)
                {
                    u.HeadImgUrl = path;
                }
                if (u.Update(Query<int>("id"), true))
                {
                    Reflesh(Query<int>("id"));
                }
            }
        }
        public void BtnDelete()
        {

            using (Users u = new Users())
            {
                u.Delete(Query<int>("id"));//id  ?id=xxx
                Reflesh(1);
            }
        }
        #endregion
        #endregion

        #region 其它过程方法


        private void Reflesh(int id)
        {
            Context.Response.Redirect(Context.Request.Url.LocalPath + "?id=" + id, true);
        }


        private string SavePic()
        {
            if (Context.Request.Files != null)
            {
                HttpPostedFile file = Context.Request.Files[0];
                if (file != null && file.ContentLength > 0)
                {
                    string path = "/Upload/" + file.FileName;
                    string folder = AppConfig.WebRootPath + "Upload/";
                    if (!Directory.Exists(folder))
                    {
                        Directory.CreateDirectory(folder);
                    }
                    file.SaveAs(folder + file.FileName);
                    return path;
                }
            }
            return null;
        }

        //private void BtnEvent()
        //{
        //    if (IsClick("btnAdd"))
        //    {
        //        using (Users u = new Users())
        //        {
        //            string path = SavePic();
        //            if (path != null)
        //            {
        //                u.HeadImgUrl = path;
        //            }
        //            if (u.Insert(true))
        //            {
        //                Reflesh(u.ID);
        //            }
        //        }
        //    }
        //    if (IsClick("btnUpdate"))
        //    {
        //        using (Users u = new Users())
        //        {
        //            string path = SavePic();
        //            if (path != null)
        //            {
        //                u.HeadImgUrl = path;
        //            }
        //            if (u.Update(null, true))
        //            {
        //                Reflesh(u.ID);
        //            }
        //        }
        //    }
        //    else if (IsClick("btnDelete"))
        //    {
        //        using (Users u = new Users())
        //        {
        //            u.Delete();
        //            Reflesh(1);
        //        }
        //    }
        //}

        string View_OnForeach(string text, Dictionary<string, string> values, int rowIndex)
        {
            values["Sex"] = values["Sex"] == "1" ? "Boy" : "Girl";
            return text;
        }
        #endregion


    }
}
