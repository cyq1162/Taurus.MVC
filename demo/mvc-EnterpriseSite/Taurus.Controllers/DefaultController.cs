using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Taurus.Controllers.Logic;
using Taurus.Mvc;

namespace Taurus.Controllers
{
    public partial class DefaultController:Controller
    {
        DefaultLogic logic;
        public DefaultController()
        {
            logic = new DefaultLogic(this);
        }
        public override void Default()
        {
            if (!IsHttpPost)
            {
                logic.BindMenu();
                logic.BindHomePhoto();
                logic.BindTopArticle();
            }
        }
        public void ArticleDetail()
        {
            logic.BindMenu();
            logic.BindArticleClass();
            logic.BindArticleDetail();
        }
        public void ArticleList()
        {
            logic.BindMenu();
            logic.BindArticleClass();
            logic.BindArticleList();
        }
        public void PhotoList()
        {
            logic.BindMenu();
            logic.BindPhotoClass();
            logic.BindPhotoList();
        }
    }
}
