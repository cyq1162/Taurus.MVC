using System;
using System.Data;
using System.Configuration;
using System.Web;

namespace Taurus.Logic
{
    public class PhotoClass : CYQ.Data.Orm.OrmBase
    {
        public PhotoClass()
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
    }
}
