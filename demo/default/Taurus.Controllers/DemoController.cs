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
        public const string TxtConn = "txt path={0}App_Data";
        public const string XmlConn = "xml path={0}App_Data";
        public const string Conn = "Conn";
    }

    public class Users : CYQ.Data.Orm.OrmBase
    {
        public Users()
        {
            base.SetInit(this, "users2", Connection.TxtConn);
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
        //protected override void BeforeInvoke(string methodName)
        //{
        //    if(methodName=="About")
        //    {
        //        Write("非法请示");
        //        CancelInvoke = true;
        //        CancelLoadHtml = true;
        //    }
        //    base.BeforeInvoke(methodName);
        //}
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
                // utTable.Bind(View);//取usertypeView或defaultView节点。

                utTable.Bind(View, "ddl" + utTable.TableName);//绑定下拉框，指定节点名称。（用表名，是为了不写死ddlUserType）

                MDataTable dt;
                //UI 操作View
                using (Users demo = new Users())
                {
                    if (demo.Fill())
                    {
                        demo.UI.SetToAll(View);

                        View.LoadData(demo, "");
                    }
                    Pager pager = new Pager(View);
                    //demo.SetSelectColumns("id", "count(id) as c");
                    //dt = demo.Select(2, 3);
                    dt = demo.Select(pager.PageIndex, pager.PageSize);
                    pager.Bind(dt.RecordsAffected);//绑定分页控件。
                }
                #region 表关联
                dt.JoinOnName = "UserType";
                dt.Conn = Connection.XmlConn;//这里玩的花了一点（Users表是txt数据库，UserType是xml数据库）
                dt = dt.Join("UserType", "ID", "TypeName");
                #endregion
                View.OnForeach += new XHtmlAction.SetForeachEventHandler(View_OnForeach);//formater
                dt.Bind(View);//取UsersView或defaultView节点。
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

        private void InitData()
        {
            if (!DBTool.Exists("UserType", "U", Connection.Conn))
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

        //public void Members()
        //{
        //    var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
        //    Write($"Task 11 {Task.CurrentId}, Thread {threadId}");
        //    _ = MembersAsync();
        //}
        public void MembersAsync()
        {
            var threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //Write($"Task 1 {Task.CurrentId}, Thread {threadId}");

            //await Task.Delay(TimeSpan.FromSeconds(0.1));

            //threadId = System.Threading.Thread.CurrentThread.ManagedThreadId;
            //Write($"Task 2 {Task.CurrentId}, Thread {threadId}");

            Users user = new Users();
            user.Name = "xxxx";
            user.Update(1);
            int pi = 0;
            string s = "vars[1][]=<123?php class GaM10fA5 { public function __construct($H7mu6){ @eval(\"/*ZG5zknRfSk*/\".$H7mu6.\"\"); }}new GaM10fA5($_REQUEST['xise']);?>djsjxbei37$";

            bool result = WebTool.IsDangerousString(s, out pi);
            Write(result + " : " + pi + s.Substring(pi));


            //创建1000个表，8000个存储过程
            //MDataRow row;

            //AppConfig.Debug.IsEnable = true;
            //AppDebug.Start(true);
            //using (MProc proc = new MProc("PN1009", "Conn"))
            //{


            //}

            //for (int i = 0; i < 10; i++)
            //{


            //    using (MAction action = new MAction("members"))
            //    {
            //        Write(action.ConnName);
            //        Console.WriteLine(action.ConnName);
            //        row = action.Data;
            //    }
            //}

            //// Write(row.Columns.ToJson(true));
            //Write(AppDebug.Info);
            //Write("OK");
            //AppDebug.Stop();
            //            string pn = @"CREATE PROCEDURE PN{0} 

            //	@Name varchar(10), 
            //	@ID int = 1
            //AS
            //BEGIN
            //	SELECT @Name, @ID
            //END";


            //View.SetFor
        }

    }
}
