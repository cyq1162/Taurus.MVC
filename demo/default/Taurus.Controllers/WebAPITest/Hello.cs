using CYQ.Data;
using System;
using System.Collections.Generic;
using System.Text;
using Taurus.Mvc;
using Taurus.Mvc.Attr;
using CYQ.Data.Tool;
using CYQ.Data.Json;

namespace Taurus.Controllers
{
    public class AABase : Controller { }
    public class Hello : AABase
    {
        public override string APIResult
        {
            get
            {
                return base.APIResult;
            }
        }
        public override bool CheckToken(string token)
        {
            SetQuery("mypara1", "valueA");
            SetQuery("mypara2", "valueB");
            Write(JsonHelper.OutResult("result", false, "code", 404, "msg", "xxxx"));
            return false;
        }
        public override bool CheckAck(string ack)
        {
            return base.CheckAck(ack);
        }
        public void World()
        {

            Write("Hello World.");
            using (MAction action = new MAction("dynamiccomments"))
            {

            }
        }
        [Taurus.Mvc.Attr.Token]
        [HttpPost]
        public void Del()
        {
            Write("del...." + Query<string>("mypara1") + "," + Query<string>("mypara2") + Query<string>("aaa"));
        }
    }
}
