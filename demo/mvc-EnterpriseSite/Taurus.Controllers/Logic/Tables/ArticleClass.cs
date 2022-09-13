using System;
using System.Data;
using System.Configuration;
using System.Web;


namespace Taurus.Logic
{
    /// <summary>
    /// 分类
    /// </summary>
    public class ArticleClass : CYQ.Data.Orm.OrmBase
    {
        public ArticleClass()
        {
            base.SetInit(this);
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
        private int _Count;
        /// <summary>
        /// 分类的文章总数
        /// </summary>
        public int Count
        {
            get { return _Count; }
            set { _Count = value; }
        }
        private int _LastArticleID;
        /// <summary>
        /// 分类的最后一次ID。
        /// </summary>
        public int LastArticleID
        {
            get { return _LastArticleID; }
            set { _LastArticleID = value; }
        }
        private int _OrderNum;
        /// <summary>
        /// 排序
        /// </summary>
        public int OrderNum
        {
            get { return _OrderNum; }
            set { _OrderNum = value; }
        }
    }
}
