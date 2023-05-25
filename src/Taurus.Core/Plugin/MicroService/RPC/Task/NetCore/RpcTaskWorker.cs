using System.Net.Http;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Web;
using Taurus.Mvc;
using System.IO;
using System;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// Net Core 版本
    /// </summary>
    internal partial class RpcTaskWorker
    {
        private static void AddHeader(RpcTask task, HttpRequestMessage message)
        {
            message.Headers.Add(MsConst.HeaderKey, (MsConfig.IsClient ? MsConfig.Client.RcKey : MsConfig.Server.RcKey));
            //message.Headers.Add("X-Real-IP", MvcConst.HostIP);
            if (HttpContext.Current != null && HttpContext.Current.Request != null)
            {
                message.Headers.Add("Referer", HttpContext.Current.Request.Url.AbsoluteUri);//当前运行地址。
            }
            else if (!string.IsNullOrEmpty(MvcConfig.RunUrl))
            {
                message.Headers.Add("Referer", MvcConfig.RunUrl);//当前运行地址。
            }
            if (task.Request.Header != null && task.Request.Header.Count > 0)
            {
                foreach (var item in task.Request.Header)
                {
                    message.Headers.Add(item.Key, item.Value);
                }
            }

        }

        public static void ExeTaskAsync(RpcTask task)
        {
            task.State = RpcTaskState.Running;
            HttpClient httpClient = HttpClientPool.Create(task.Request.Uri, task.Request.Timeout);

            //Task<HttpResponseMessage>[] httpResponseMessages = new Task<HttpResponseMessage>[1000];

            //for (int i = 0; i < 1000; i++)
            //{
            //    HttpRequestMessage request2 = new HttpRequestMessage(HttpMethod.Get, task.Request.Url);
            //    AddHeader(task, request2);
            //    httpResponseMessages[i] = httpClient.SendAsync(request2);

            //}
            //for (int i = 0; i < 1000; i++)
            //{
            //    string bb = httpResponseMessages[i].Result.Content.ReadAsStringAsync().Result;
            //    System.Diagnostics.Debug.WriteLine(i + " : " + bb);
            //}



            try
            {
                //if (task.Request.HttpMethod.ToUpper() == "GET")
                //{
                //    HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, task.Request.Url);
                //    AddHeader(task, request);
                //    task.task = httpClient.SendAsync(request);
                //}
                //else
                //{
                //                MultipartFormDataContent =》multipart / form - data

                //FormUrlEncodedContent =》application / x - www - form - urlencoded

                //StringContent =》application / json等

                //StreamContent =》binary

                HttpRequestMessage request = new HttpRequestMessage(new HttpMethod(task.Request.Method), task.Request.Url);
                if (task.Request.Data != null)
                {
                    request.Content = new StreamContent(new MemoryStream(task.Request.Data)) { };
                }
                AddHeader(task, request);
                task.task = httpClient.SendAsync(request);
                //}
            }
            catch (Exception err)
            {
                task.State = RpcTaskState.Complete;
                task.Result = new RpcTaskResult() { Error = err };
                MsLog.Write(err.Message, task.Request.Url, task.Request.Method, "Rpc.ExeTaskAsync()");
            }
        }
    }
}