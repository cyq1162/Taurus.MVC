using System;
using System.Data;
using System.Configuration;
using System.Web;

namespace Taurus.Logic
{
    public class Menu : CYQ.Data.Orm.OrmBase
    {
        public Menu()
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
        private string _MenuUrl;

        public string MenuUrl
        {
            get { return _MenuUrl; }
            set { _MenuUrl = value; }
        }
        private bool _IsNewWindow;
        /// <summary>
        /// ÊÇ·ñÐÂ¿ª¿Õ°×
        /// </summary>
        public bool IsNewWindow
        {
            get { return _IsNewWindow; }
            set { _IsNewWindow = value; }
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
