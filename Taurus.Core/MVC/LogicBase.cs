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
        IController _IViewBase;

        public LogicBase(IController controller)
        {
            _IViewBase = controller;
        }
        private LogicBase()
        {

        }

        #region _ICommon
        public string Order
        {
            get { return _IViewBase.Order; }
        }
        public string Sort
        {
            get { return _IViewBase.Sort; }
        }
        public int PageIndex
        {
            get { return _IViewBase.PageIndex; }
        }

        public int PageSize
        {
            get { return _IViewBase.PageSize; }
        }
        public T Query<T>(Enum key)
        {
            return _IViewBase.Query<T>(key);
        }
        public T Query<T>(string key)
        {
            return _IViewBase.Query<T>(key);
        }

        public T Query<T>(string key, T defaultValue)
        {
            return _IViewBase.Query<T>(key, defaultValue);
        }
        public HttpContext Context
        {
            get
            {
                return _IViewBase.Context;
            }
        }
        public bool IsHttpGet
        {
            get { return _IViewBase.IsHttpGet; }
        }

        public bool IsHttpPost
        {
            get { return _IViewBase.IsHttpPost; }
        }

        public XHtmlAction View
        {
            get
            {
                return _IViewBase.View;
            }
            set
            {
                _IViewBase.View = value;
            }
        }
        #endregion


        public Type ControllerType
        {
            get { return _IViewBase.ControllerType; }
        }

        public string Action
        {
            get { return _IViewBase.Action; }
        }

        public string Para
        {
            get { return _IViewBase.Para; }
        }


        public string AjaxResult
        {
            get
            {
                return _IViewBase.AjaxResult;
            }
            set
            {
                _IViewBase.AjaxResult = value; ;
            }
        }
    }
}
