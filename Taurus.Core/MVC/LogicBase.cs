using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;
using System.Web;
using CYQ.Data.Xml;

namespace Taurus.Core
{
    public abstract partial class LogicBase : IController
    {
        IController _IController;

        public LogicBase(IController controller)
        {
            _IController = controller;
        }
        private LogicBase()
        {

        }

        #region _ICommon
        public string Order
        {
            get { return _IController.Order; }
        }
        public string Sort
        {
            get { return _IController.Sort; }
        }
        public int PageIndex
        {
            get { return _IController.PageIndex; }
        }

        public int PageSize
        {
            get { return _IController.PageSize; }
        }
        public T Query<T>(Enum key)
        {
            return _IController.Query<T>(key);
        }
        public T Query<T>(string key)
        {
            return _IController.Query<T>(key);
        }

        public T Query<T>(string key, T defaultValue)
        {
            return _IController.Query<T>(key, defaultValue);
        }
        public HttpContext Context
        {
            get
            {
                return _IController.Context;
            }
        }
        public bool IsHttpGet
        {
            get { return _IController.IsHttpGet; }
        }

        public bool IsHttpPost
        {
            get { return _IController.IsHttpPost; }
        }

        public XHtmlAction View
        {
            get
            {
                return _IController.View;
            }
            set
            {
                _IController.View = value;
            }
        }
        #endregion


        public Type ControllerType
        {
            get { return _IController.ControllerType; }
        }

        public string Action
        {
            get { return _IController.Action; }
        }

        public string Para
        {
            get { return _IController.Para; }
        }

        public void Write(string msg)
        {
            _IController.Write(msg);
        }

        public void Write(string msg, bool isSucess)
        {
            _IController.Write(msg);
        }
    }
}
