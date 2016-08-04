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
        XHtmlAction View { get; set; }
        HttpContext Context { get; }
        bool IsHttpGet { get; }
        bool IsHttpPost { get; }
        Type ControllerType { get; }
        string Action { get; }
        string Para { get; }
        string AjaxResult { get; set; }
    }
}
