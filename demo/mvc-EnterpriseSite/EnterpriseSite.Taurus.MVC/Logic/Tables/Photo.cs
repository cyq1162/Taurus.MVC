using System;
using System.Data;
using System.Configuration;
using System.Web;

namespace Taurus.Logic
{
    public class Photo : CYQ.Data.Orm.OrmBase
    {
        public Photo()
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
        private string _Description;

        public string Description
        {
            get { return _Description; }
            set { _Description = value; }
        }

        //private string _Tag;

        //public string Tag
        //{
        //    get { return _Tag; }
        //    set { _Tag = value; }
        //}
        private string _ImgUrl;

        public string ImgUrl
        {
            get { return _ImgUrl; }
            set { _ImgUrl = value; }
        }

        private DateTime _CreateTime;

        public DateTime CreateTime
        {
            get { return _CreateTime; }
            set { _CreateTime = value; }
        }
    }
}
