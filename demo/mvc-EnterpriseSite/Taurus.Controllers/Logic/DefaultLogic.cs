using CYQ.Data.Orm;
using CYQ.Data.Table;
using CYQ.Data.Xml;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taurus.Core;
using Taurus.Logic;
using Taurus.Mvc;

namespace Taurus.Controllers.Logic
{
    /// <summary>
    /// 不想建一个Taurus.Logic项目来存逻辑，只好把业务逻辑写在这里。
    /// </summary>
    public class DefaultLogic : LogicBase
    {
        public DefaultLogic(Controller controller)
            : base(controller)
        {

        }
        #region 页面共用部分
        public void BindMenu()
        {
            using (Menu m = new Menu())
            {
                m.Select("order by ordernum").Bind(View);
            }
        }
        #endregion

        #region 首页
        public void BindHomePhoto()
        {
            using (HomePhoto hp = new HomePhoto())
            {
                hp.Select("TypeName='首页左侧图' order by orderNum").Bind(View, "homephoto1");
                hp.Select("TypeName='首页右侧图' order by orderNum").Bind(View, "homephoto2");
            }
        }

        public void BindTopArticle()
        {
            ArticleClass ac = DBFast.Find<ArticleClass>("Name='行业资讯' or Name='最新资讯'");
            if (ac != null)
            {
                View.Set("lnkMore", SetType.Href, "/articlelist?id=" + ac.ID);
                using (Article a = new Article())
                {
                    View.OnForeach += View_OnForeach;
                    a.Select(3, "CateID=" + ac.ID + " order by id desc").Bind(View);
                }
            }
        }

        string View_OnForeach(string text, Dictionary<string, string> values, int rowIndex)
        {
            values["CreateTime"] = FormatDate(values["CreateTime"]);
            return text;
        }

        /// <summary>
        /// 格式化日期格式为[yyyy-MM-dd]的形式
        /// </summary>
        /// <param name="objDate"></param>
        /// <returns></returns>
        protected string FormatDate(object objDate)
        {
            if (objDate == null)
            {
                return "";
            }
            DateTime CurrentDate;
            DateTime.TryParse(Convert.ToString(objDate), out CurrentDate);
            return CurrentDate.ToString("yyyy-MM-dd HH:mm");
        }
        #endregion

        #region 文章列表
        public void BindArticleClass()
        {
            using (ArticleClass a = new ArticleClass())
            {
                a.Select("order by orderNum asc").Bind(View);
            }
        }
        public void BindArticleList()
        {
            MDataTable dt;
            using (Article a = new Article())
            {
                dt = a.Select("CateID=" + Query<string>("id") + " order by id desc");
            }
            dt.Bind(View);
            View.Set("labCount", dt.RecordsAffected.ToString());
        }
        public void BindArticleDetail()
        {
            using (Article a = new Article())
            {
                if (a.Fill(Query<string>("id")))
                {
                    View.Set("txtTitle", a.Title);
                    View.Set("txtBody", ArticleBody.Get(a.ID));
                    View.Set("title",a.Title + " - Taurus.MVC");
                }
            }
        }
        #endregion

        #region 产品列表
        public void BindPhotoClass()
        {
            using (PhotoClass a = new PhotoClass())
            {
                a.Select().Bind(View);
            }
        }
        public void BindPhotoList()
        {
            PhotoClass pc = DBFast.Find<PhotoClass>(Query<string>("id"));
            if (pc != null)
            {
                View.Set("title", pc.Name + " - Taurus.MVC");
            }
            using (Photo a = new Photo())
            {
                a.Select("CateID=" + Query<string>("id")).Bind(View);
            }
        }
        #endregion
       
       
       
        
    }
}
