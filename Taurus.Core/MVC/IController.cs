using CYQ.Data.Xml;
using System;
using System.Web;
namespace Taurus.Core
{
    public interface IController
    {
        int PageIndex { get; }
        int PageSize { get; }
        string Sort { get; }
        string Order { get; }
        T Query<T>(Enum key);
        T Query<T>(string key);
        T Query<T>(string key, T defaultValue);
        T Query<T>(int paraIndex);
        T Query<T>(int paraIndex, T defaultValue);
        T GetEntity<T>() where T : class;
        XHtmlAction View { get; set; }
        HttpContext Context { get; }
        HttpRequest Request { get; }
        HttpResponse Response { get; }
        bool IsHttpGet { get; }
        bool IsHttpPost { get; }
        Type ControllerType { get; }
        string Action { get; }
        string Para { get; }
        void Write(string msg);
        void Write(string msg, bool isSuccess);
        string GetJson();
        string CheckFormat(string errMsg, params string[] paras);
    }
}
