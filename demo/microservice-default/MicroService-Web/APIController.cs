using Taurus.Mvc;
using Taurus.Mvc.Attr;
using Taurus.Plugin.MicroService;

namespace Taurus.Controllers
{
    /// <summary>
    /// 微服务测试
    /// </summary>
    public class APIController : Controller
    {
        /// <summary>
        /// Get or Post 测试
        /// </summary>
        [HttpGet]
        public void Hello()
        {
            if (Request.UrlReferrer != null)
            {
                Write("From : " + Request.UrlReferrer.ToString() + "<br/>");
            }
            Write("Hello , MicroService : " + MvcConfig.RunUrl + Request.Url.LocalPath);
        }
    }
}
