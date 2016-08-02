using CYQ.Data.Xml;
using System;
using System.Collections.Generic;
using System.Text;
using System.Web;

namespace Taurus.Core
{
    /// <summary>
    /// 该类只限框架使用，外部继承应该使用AjaxController或ViewController
    /// </summary>
    public abstract class ControllerProperty : IViewBase
    {

        /// <summary>
        /// datagrid分页的页码数
        /// </summary>
        public int PageIndex
        {
            get
            {
                return Query<int>("page");
            }
        }
        /// <summary>
        /// datagrid分页的页容量
        /// </summary>
        public int PageSize
        {
            get
            {
                return Query<int>("rows");
            }
        }

        /// <summary>
        /// 排序字段名
        /// </summary>
        public string Sort
        {
            get
            {
                return Query<string>("sort", "");
            }

        }
        /// <summary>
        /// 排序类型（升或降）
        /// </summary>
        public string Order
        {
            get
            {
                return Query<string>("order", "desc");
            }
        }
        public T Query<T>(Enum key)
        {
            return Query<T>(key.ToString(), default(T));
        }
        public T Query<T>(string key)
        {
            return Query<T>(key, default(T));
        }
        public T Query<T>(string key, T defaultValue)
        {
            return QueryTool.Query<T>(key, defaultValue, false);
        }


        public virtual XHtmlAction View
        {
            get { return null; }
            set { }
        }


        public HttpContext Context
        {
            get { return HttpContext.Current; }
        }


        public bool IsHttpGet
        {
            get { return Context.Request.RequestType == "GET"; }
        }

        public bool IsHttpPost
        {
            get { return Context.Request.RequestType == "POST"; }
        }

        /// <summary>
        /// 当前的Controller{controller}
        /// </summary>
        public virtual Type Controller
        {
            get { return null; }
        }
        /// <summary>
        /// 当前的Action名称{action}
        /// </summary>
        public virtual string Action
        {
            get { return null; }
        }
        /// <summary>
        /// 当前的参数{id}
        /// </summary>
        public virtual string Para
        {
            get { return null; }
        }
    }
}
