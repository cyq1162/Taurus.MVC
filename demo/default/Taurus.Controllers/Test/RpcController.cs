using System;
using System.Collections.Generic;
using Taurus.Mvc;
using Taurus.MicroService;
using System.Text;
namespace Taurus.Controllers.Test
{

    public class RpcController : Controller
    {
        static int i = 0;
        public void Call()
        {

            i++;
            //bool result = tt.WaitOne(1000);
            //result = tt.WaitOne(3000);
            //result = task.Set();
            //result = task.WaitOne();
            //result = task.WaitOne();
            //RpcTask task = MSRpc.GetAsync("http://localhost:8000/ms/hello");
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
            RpcTask task = Rpc.StartTaskAsync(new RpcTaskRequest() { Method = "Put", Url = Rpc.GetHost("ms") + "/ms/hello2" });//,Encoding.UTF8.GetBytes("id=2&msg=" + DateTime.Now.Ticks)
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
            RpcTask task = Rpc.StartTaskAsync(new RpcTaskRequest() { Method = "Delete", Url = Rpc.GetHost("ms") + "/ms/hello2" });//,Encoding.UTF8.GetBytes("id=2&msg=" + DateTime.Now.Ticks)
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
            RpcTask task = Rpc.StartTaskAsync(new RpcTaskRequest() { Method = "Head", Url = Rpc.GetHost("ms") + "/ms/hello2" });//,Encoding.UTF8.GetBytes("id=2&msg=" + DateTime.Now.Ticks)
            System.Diagnostics.Debug.WriteLine("-----------------------------" + i);
            task.Wait();
            string text = task.Result.IsSuccess ? CYQ.Data.Tool.JsonHelper.ToJson(task.Result.Header) : task.Result.ErrorText;
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
             // RpcTask task =Rpc.s

            //    Taurus.MicroService.
            //    Rpc.GetHost.Proxy(Context, Rpc.GetHost("ms") + "/ms/hello", false);
        }
    }
}
