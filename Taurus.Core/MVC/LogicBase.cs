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
        public string APIResult
        {
            get { return _IController.APIResult; }
        }
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
        public T Query<T>(int paraIndex)
        {
            return _IController.Query<T>(paraIndex);
        }

        public T Query<T>(int paraIndex, T defaultValue)
        {
            return _IController.Query<T>(paraIndex, defaultValue);
        }
        public void SetQuery(string name, string value)
        {
            _IController.SetQuery(name, value);
        }
        public HttpContext Context
        {
            get
            {
                return _IController.Context;
            }
        }
        public HttpRequest Request
        {
            get { return _IController.Request; }
        }

        public HttpResponse Response
        {
            get { return _IController.Response; }
        }
        public bool IsHttpGet
        {
            get { return _IController.IsHttpGet; }
        }

        public bool IsHttpPost
        {
            get { return _IController.IsHttpPost; }
        }
        public bool IsHttpHead
        {
            get { return _IController.IsHttpHead; }
        }
        public bool IsHttpPut
        {
            get { return _IController.IsHttpPut; }
        }
        public bool IsHttpDelete
        {
            get { return _IController.IsHttpDelete; }
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
        public string Module
        {
            get { return _IController.Module; }
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

        public void Write(string msg, bool isSuccess)
        {
            _IController.Write(msg, isSuccess);
        }
        public void Write(object obj)
        {
            _IController.Write(obj);
        }

        public void Write(object obj, bool isSuccess)
        {
            _IController.Write(obj, isSuccess);
        }

        public T GetEntity<T>() where T : class
        {
            return _IController.GetEntity<T>();
        }


        public string GetJson()
        {
            return _IController.GetJson();
        }


        public bool CheckFormat(string formatter, params string[] paras)
        {
            return _IController.CheckFormat(formatter, paras);
        }

       
    }
}
