using System;
using System.Collections.Generic;
using Taurus.Mvc;
using Taurus.MicroService;
using System.Text;
using CYQ.Data;
using CYQ.Data.Tool;

namespace Taurus.Controllers.Test
{

    public class RpcController : Taurus.Mvc.Controller
    {
        static int i = 0;
        public void CallGet()
        {

            i++;
            RpcTask task = Rpc.StartGetAsync("ms", "/ms/hello?msg=" + DateTime.Now.Ticks);
            System.Diagnostics.Debug.WriteLine("-----------------------------" + i);
            task.Wait();
            string text = task.Result.IsSuccess ? task.Result.ResultText : task.Result.ErrorText;
            RpcTaskState state = task.State;
            if (string.IsNullOrEmpty(text) || !task.Result.IsSuccess)
            {

                if (state == RpcTaskState.Running)
                {
                    task.Wait();
                }
                Response.StatusCode = 404;
            }
            Write(text);
        }
        public void CallPost()
        {

            i++;
            RpcTask task = Rpc.StartPostAsync("ms", "/ms/hello?id=" + DateTime.Now.Ticks, Encoding.UTF8.GetBytes("id=2&msg=" + DateTime.Now.Ticks));//,
            System.Diagnostics.Debug.WriteLine("-----------------------------" + i);
            task.Wait();
            string text = task.Result.IsSuccess ? task.Result.ResultText : task.Result.ErrorText;
            RpcTaskState state = task.State;
            if (string.IsNullOrEmpty(text) || !task.Result.IsSuccess)
            {

                if (state == RpcTaskState.Running)
                {
                    task.Wait();
                }
                Response.StatusCode = 404;
            }
            Write(text);
        }

        public void CallPut()
        {

            i++;
            RpcTask task = Rpc.StartPutAsync("ms", "/ms/hello?id=" + DateTime.Now.Ticks, Encoding.UTF8.GetBytes("id=2&msg=" + DateTime.Now.Ticks));//,Encoding.UTF8.GetBytes("id=2&msg=" + DateTime.Now.Ticks)
            System.Diagnostics.Debug.WriteLine("-----------------------------" + i);
            task.Wait();
            string text = task.Result.IsSuccess ? task.Result.ResultText : task.Result.ErrorText;
            RpcTaskState state = task.State;
            if (string.IsNullOrEmpty(text) || !task.Result.IsSuccess)
            {

                if (state == RpcTaskState.Running)
                {
                    task.Wait();
                }
                Response.StatusCode = 404;
            }
            Write(text);
        }
        public void CallDelete()
        {

            i++;
            RpcTask task = Rpc.StartDeleteAsync("ms", "/ms/hello2?msg=" + DateTime.Now.Ticks);//,Encoding.UTF8.GetBytes("id=2&msg=" + DateTime.Now.Ticks)
            System.Diagnostics.Debug.WriteLine("-----------------------------" + i);
            task.Wait();
            string text = task.Result.IsSuccess ? task.Result.ResultText : task.Result.ErrorText;
            RpcTaskState state = task.State;
            if (string.IsNullOrEmpty(text) || !task.Result.IsSuccess)
            {

                if (state == RpcTaskState.Running)
                {
                    task.Wait();
                }
                Response.StatusCode = 404;
            }
            Write(text);
        }
        public void CallHead()
        {
            i++;
            RpcTask task = Rpc.StartHeadAsync("ms", "/ms/hello?msg=" + DateTime.Now.Ticks);//,Encoding.UTF8.GetBytes("id=2&msg=" + DateTime.Now.Ticks)
            //System.Diagnostics.Debug.WriteLine("-----------------------------" + i);
            task.Wait();
            if (!task.Result.IsSuccess)
            {
                Write(task.Result.ErrorText);
                return;
            }
            string text = JsonHelper.ToJson(task.Result.Header);
            Write(text);
        }
        public void CallTimeout()
        {
            i++;
            RpcTaskRequest request = new RpcTaskRequest();
            request.Url = Rpc.GetHost("ms") + "/ms/hello";
            request.Method = "Get";
            request.Timeout = 1;
            RpcTask task = Rpc.StartTaskAsync(request);
            System.Diagnostics.Debug.WriteLine("-----------------------------" + i);
            task.Wait();
            string text = task.Result.IsSuccess ? CYQ.Data.Tool.JsonHelper.ToJson(task.Result.Header) + "<br />" + task.Result.ResultText : task.Result.ErrorText;
            RpcTaskState state = task.State;
            if (string.IsNullOrEmpty(text) || !task.Result.IsSuccess)
            {

                if (state == RpcTaskState.Running)
                {
                    task.Wait();
                }
                Response.StatusCode = 404;
            }
            Write(text);
        }
        public void Call3()
        {
            using (MAction action = new MAction("Users"))
            {
                action.BeginTransation();

                if (action.Insert(true))
                {
                    RpcTask task = Rpc.StartPostAsync("ms", "/ms/post", null, null);
                    if (task.Result.IsSuccess)
                    {
                        RpcTask task2 = Rpc.StartPostAsync("ms", "/ms/post", null, null);
                        if (!task2.Result.IsSuccess)
                        {
                            //task.RollBack();
                            action.RollBack();
                        }
                    }
                }

            }
        }
        public void Https()
        {
            RpcTask task = Rpc.StartGetAsync("https://cyq1162.cnblogs.com");
            task.Wait();
            Write(JsonHelper.ToJson(task.Result.Header));
        }
        public void Http()
        {
            Dictionary<string, string> header = new Dictionary<string, string>();
            header.Add("Expect", "hello");
            Rpc.StartGetAsync("http://www.cyqdata.com", header).Wait();

            Rpc.StartGetAsync("http://www.cyqdata.com").Wait();
        }
    }
}
