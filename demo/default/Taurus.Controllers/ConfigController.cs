using Taurus.Mvc;
using Taurus.Plugin.MicroService;

namespace Taurus.Controllers
{
    public class ConfigController : Controller
    {

        public void Get()
        {
            Write(MsConfig.Server.Name + "," + MsConfig.Client.Name);
        }
    }
}
