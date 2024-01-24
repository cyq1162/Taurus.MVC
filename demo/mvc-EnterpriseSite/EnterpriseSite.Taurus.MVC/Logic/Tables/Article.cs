using System;
using System.Data;
using System.Configuration;
using System.Web;

namespace Taurus.Logic
{
    /// <summary>
    /// ндуб
    /// </summary>
    public class Article:CYQ.Data.Orm.OrmBase
    {
        public Article()
        {
            base.SetInit(this);
        }
        private int _ID;

        public int ID
        {
            get { return _ID; }
            set { _ID = value; }
        }
        private int _CateID;

        public int CateID
        {
            get { return _CateID; }
            set { _CateID = value; }
        }
        private string _Title;

        public string Title
        {
            get { return _Title; }
            set { _Title = value; }
        }
        //private string _Abstract;

        //public string Abstract
        //{
        //    get { return _Abstract; }
        //    set { _Abstract = value; }
        //}

        //private string _Tag;

        //public string Tag
        //{
        //    get { return _Tag; }
        //    set { _Tag = value; }
        //}

        private DateTime _CreateTime;

        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }

    }
}
