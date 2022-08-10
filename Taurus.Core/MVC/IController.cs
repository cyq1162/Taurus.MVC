using CYQ.Data.Xml;
using System;
using System.Web;
namespace Taurus.Core
{
    public interface IController
    {
        /// <summary>
        /// 缓存Write方法输出的结果，用于最后输出
        /// </summary>
        string APIResult { get; }
        /// <summary>
        /// 获取参数:page
        /// </summary>
        int PageIndex { get; }
        /// <summary>
        /// 获取参数:rows
        /// </summary>
        int PageSize { get; }
        /// <summary>
        /// 获取参数方法
        /// </summary>
        T Query<T>(Enum key);
        T Query<T>(string key);
        T Query<T>(string key, T defaultValue);
        T Query<T>(int paraIndex);
        T Query<T>(int paraIndex, T defaultValue);
        void SetQuery(string name, string value);
        /// <summary>
        /// 从Post过来的数据中获得实体类型的转换
        /// </summary>
        T GetEntity<T>() where T : class;
        /// <summary>
        /// MVC 的视图引擎
        /// </summary>
        XHtmlAction View { get; set; }
        HttpContext Context { get; }
        HttpRequest Request { get; }
        HttpResponse Response { get; }
        bool IsHttpGet { get; }
        bool IsHttpPost { get; }
        bool IsHttpHead { get; }
        bool IsHttpPut { get; }
        bool IsHttpDelete { get; }
        Type ControllerType { get; }
        /// <summary>
        /// 路由：模块参数
        /// </summary>
        string Module { get; }
        /// <summary>
        /// 路由：控制器参数
        /// </summary>
        string ControllerName { get; }
        /// <summary>
        /// 路由：方法参数
        /// </summary>
        string Action { get; }
        /// <summary>
        /// 路由：第一个参数
        /// </summary>
        string Para { get; }
        /// <summary>
        /// 输出结果
        /// </summary>
        void Write(string msg);
        void Write(string msg, bool isSuccess);
        void Write(object obj);
        void Write(object obj, bool isSuccess);
        /// <summary>
        /// 获取Post请求，非标准请求头时，从数据流读取请求数据。
        /// </summary>
        /// <returns></returns>
        string GetJson();
    }
}
