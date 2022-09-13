using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
using Taurus.Mvc;
using CYQ.Data.Tool;
using CYQ.Data.Table;
namespace Taurus.Controllers
{
    public class HomeController : Controller
    {
        public override void Default()
        {
            Response.Redirect("/");
        }
        //public override bool BeforeInvoke(string methodName)
        //{
        //    ViewEngine.
        //    return true;
        //} 
        public void Index()
        {
            //Context.WebSockets.IsWebSocketRequest
            //IList<MicroService.HostInfo> list = new List<MicroService.HostInfo>();
            //MicroService.HostInfo hostInfo = new MicroService.HostInfo();
            //hostInfo.Host = "d1";
            //MicroService.HostInfo hostInfo2 = new MicroService.HostInfo();
            //hostInfo2.Host = "d2";
            //list.Add(hostInfo);
            //list.Add(hostInfo2);
            //string json = JsonHelper.ToJson(hostInfo);
            //MDataTable dt = MDataTable.CreateFrom(json);
            //MDataTable dt2 = MDataTable.CreateFrom(list);

            //string jsonList = JsonHelper.ToJson(list);

            //MicroService.HostInfo hostInfo3 = (MicroService.HostInfo)ConvertTool.ChangeType(json, typeof(MicroService.HostInfo));
            //string host = hostInfo2.Host;

            //List<MicroService.HostInfo> hostInfoList = (List<MicroService.HostInfo>)ConvertTool.ChangeType(jsonList, typeof(List<MicroService.HostInfo>));
            //string host22 = hostInfoList[0].Host;
            //HttpCookie cookie = new HttpCookie("taurus");
            //cookie.Value = "taurus.Test";
            //Response.Cookies.Add(cookie);
            //string path = @"C:\Users\Administrator\Pictures\temp\timg.jpg";
            //HttpPostedFile hpf = Taurus.Core.HttpPostedFileExtend.Create(path);
            //hpf.SaveAs(System.IO.Path.GetDirectoryName(path) + "\\ddd.jpg");
        }
        public void About() { }
        public void Contact() { }
        public void WebAPI() { }
    }
}
