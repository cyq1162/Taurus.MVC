using CYQ.Data.Xml;
using System;
using System.Web;
namespace Taurus.Core
{
    public interface IController
    {
        string APIResult { get; }
        int PageIndex { get; }
        int PageSize { get; }

        T Query<T>(Enum key);
        T Query<T>(string key);
        T Query<T>(string key, T defaultValue);
        T Query<T>(int paraIndex);
        T Query<T>(int paraIndex, T defaultValue);
        void SetQuery(string name, string value);
        T GetEntity<T>() where T : class;
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
        string Module { get; }
        string ControllerName { get; }
        string Action { get; }
        string Para { get; }
        void Write(string msg);
        void Write(string msg, bool isSuccess);
        void Write(object obj);
        void Write(object obj, bool isSuccess);
        string GetJson();
    }
}
