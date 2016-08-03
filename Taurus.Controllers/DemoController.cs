using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;
using CYQ.Data;
using CYQ.Data.Table;
using CYQ.Data.Xml;
using CYQ.Data.Tool;
using System.Web;
using System.IO;

namespace Taurus.Controllers
{
    #region CodeFirst的数据表

    public class Users : CYQ.Data.Orm.OrmBase
    {
        public Users()
        {
            base.SetInit(this, "Users", "txt path={0}App_Data");
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
        private int _UserType;

        public int UserType
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
            base.SetInit(this, "UserType", "xml path={0}App_Data");
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
                InitData();
                MDataTable utTable = null;
                using (UserType ut = new UserType())
                {
                    utTable = ut.Select();
                }
                // View.OnForeach += new XHtmlAction.SetForeachEventHandler(View_OnForeach);
                utTable.Bind(View);//取usertypeView或defaultView节点。

                utTable.Bind(View, "ddl" + utTable.TableName);//绑定下拉框，指定节点名称。


                MDataTable dt;
                //UI 操作View
                using (Users demo = new Users())
                {
                    if (demo.Fill())
                    {
                        demo.UI.SetToAll(View);
                        View.LoadData(demo.RawData, "");
                    }
                    dt = demo.Select();
                }
                // View.OnForeach += new XHtmlAction.SetForeachEventHandler(View_OnForeach);
                dt.Bind(View);//取UsersView或defaultView节点。

            }
            if (IsHttpPost)
            {
                BtnEvent();
            }

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
        #endregion

        #region 其它过程方法

        private void InitData()
        {
            if (!DBTool.ExistsTable("UserType", "xml path={0}App_Data"))
            {
                using (UserType ut = new UserType())
                {
                    ut.Delete("1=1");//Clear All Data
                    for (int i = 1; i < 5; i++)
                    {
                        ut.TypeName = "Type" + i;
                        ut.Insert(InsertOp.None);
                    }
                }
            }
        }

        private void Reflesh(int id)
        {
            Context.Response.Redirect(Context.Request.Url.LocalPath + "?id=" + id, true);
        }


        private string SavePic()
        {
            HttpPostedFile file = Context.Request.Files[0];
            if (file != null && file.ContentLength > 0)
            {
                string path = "/Upload/" + file.FileName;
                string folder = AppDomain.CurrentDomain.BaseDirectory + "Upload/";
                if (!Directory.Exists(folder))
                {
                    Directory.CreateDirectory(folder);
                }
                file.SaveAs(folder + file.FileName);
                return path;
            }
            return null;
        }

        private void BtnEvent()
        {
            if (IsClick("btnAdd"))
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
            if (IsClick("btnUpdate"))
            {
                using (Users u = new Users())
                {
                    string path = SavePic();
                    if (path != null)
                    {
                        u.HeadImgUrl = path;
                    }
                    if (u.Update(null, true))
                    {
                        Reflesh(u.ID);
                    }
                }
            }
            else if (IsClick("btnDelete"))
            {
                using (Users u = new Users())
                {
                    u.Delete();
                    Reflesh(1);
                }
            }
        }

        //string View_OnForeach(string text, object[] values, int rowIndex)
        //{
        //    return "行号：" + text;
        //}
        #endregion
    }
}
