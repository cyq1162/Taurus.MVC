using System;
using System.Data;
using System.Configuration;
using System.Web;

namespace Taurus.Logic
{
    /// <summary>
    /// Ê×Ò³Í¼Æ¬
    /// </summary>
    public class HomePhoto:CYQ.Data.Orm.OrmBase
    {
        public HomePhoto()
        {
            base.SetInit(this);
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
        private string _ImgUrl;

        public string ImgUrl
        {
            get { return _ImgUrl; }
            set { _ImgUrl = value; }
        }
        private string _LinkUrl;

        public string LinkUrl
        {
            get { return _LinkUrl; }
            set { _LinkUrl = value; }
        }

        private int _OrderNum;
        /// <summary>
        /// ÅÅÐò
        /// </summary>
        public int OrderNum
        {
            get { return _OrderNum; }
            set { _OrderNum = value; }
        }
    }
}
