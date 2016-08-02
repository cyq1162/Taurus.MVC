using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Core;
using System.Web;
using CYQ.Data.Xml;

namespace Taurus.Core
{
    public abstract partial class ViewLogicBase : IViewBase
    {
        IViewBase _ICommon;

        public ViewLogicBase(IViewBase custom)
        {
            _ICommon = custom;
        }
        private ViewLogicBase()
        {

        }

        #region _ICommon
        public string Order
        {
            get { return _ICommon.Order; }
        }
        public string Sort
        {
            get { return _ICommon.Sort; }
        }
        public int PageIndex
        {
            get { return _ICommon.PageIndex; }
        }

        public int PageSize
        {
            get { return _ICommon.PageSize; }
        }
        public T Query<T>(Enum key)
        {
            return _ICommon.Query<T>(key);
        }
        public T Query<T>(string key)
        {
            return _ICommon.Query<T>(key);
        }

        public T Query<T>(string key, T defaultValue)
        {
            return _ICommon.Query<T>(key, defaultValue);
        }
        public HttpContext Context
        {
            get
            {
                return _ICommon.Context;
            }
        }
        public bool IsHttpGet
        {
            get { return _ICommon.IsHttpGet; }
        }

        public bool IsHttpPost
        {
            get { return _ICommon.IsHttpPost; }
        }
       
        public XHtmlAction View
        {
            get
            {
                return _ICommon.View;
            }
            set
            {
                _ICommon.View = value;
            }
        }
        #endregion


        public Type Controller
        {
            get { return _ICommon.Controller; }
        }

        public string Action
        {
            get { return _ICommon.Action; }
        }

        public string Para
        {
            get { return _ICommon.Para; }
        }
    }
}
