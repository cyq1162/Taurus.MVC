using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Mvc;
using CYQ.Data;
using Taurus.MicroService;

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
