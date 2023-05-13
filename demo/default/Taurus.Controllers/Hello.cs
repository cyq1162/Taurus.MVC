using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Text;

namespace Taurus.Controllers
{
    public class Hello:Taurus.Mvc.Controller
    {
        public void World()
        {
            Write("Hello World.");
            using (MAction action = new MAction("dynamiccomments"))
            {

            }
        }
    }
}
