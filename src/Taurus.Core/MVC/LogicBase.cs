using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Mvc;
using System.Web;
using CYQ.Data.Xml;

namespace Taurus.Mvc
{
    public abstract partial class LogicBase
    {
        protected Controller Controller;
        /// <summary>
        /// 需要传递控制器进来
        /// </summary>
        public LogicBase(Controller controller)
        {
            Controller = controller;
        }
        private LogicBase()
        {

        }

        public T Query<T>(string key)
        {
            return Controller.Query<T>(key);
        }

        public T Query<T>(string key, T defaultValue)
        {
            return Controller.Query<T>(key, defaultValue);
        }
       
        public HttpContext Context
        {
            get
            {
                return Controller.Context;
            }
        }
        public HttpRequest Request
        {
            get { return Controller.Request; }
        }

        public HttpResponse Response
        {
            get { return Controller.Response; }
        }
        public bool IsHttpGet
        {
            get { return Controller.IsHttpGet; }
        }

        public bool IsHttpPost
        {
            get { return Controller.IsHttpPost; }
        }
        public bool IsHttpHead
        {
            get { return Controller.IsHttpHead; }
        }
        public bool IsHttpPut
        {
            get { return Controller.IsHttpPut; }
        }
        public bool IsHttpDelete
        {
            get { return Controller.IsHttpDelete; }
        }
        public XHtmlAction View
        {
            get
            {
                return Controller.View;
            }
            set
            {
                Controller.View = value;
            }
        }


        public Type ControllerType
        {
            get { return Controller.ControllerType; }
        }
        public string ModuleName
        {
            get { return Controller.ModuleName; }
        }
        public string ControllerName
        {
            get { return Controller.ControllerName; }
        }
        public string MethodName
        {
            get { return Controller.MethodName; }
        }

        public string Para
        {
            get { return Controller.Para; }
        }

        public void Write(string msg)
        {
            Controller.Write(msg);
        }

        public void Write(string msg, bool isSuccess)
        {
            Controller.Write(msg, isSuccess);
        }
        public void Write(object obj)
        {
            Controller.Write(obj);
        }

        public void Write(object obj, bool isSuccess)
        {
            Controller.Write(obj, isSuccess);
        }
    }
}
