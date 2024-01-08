using System.Net.Http;
using System.Web;
using System.IO;
using System;
using CYQ.Data;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// Net Core 版本
    /// </summary>
    internal partial class RpcTaskWorker
    {
        private static void AddHeader(RpcTask task, HttpRequestMessage message)
        {
            if (MsConfig.IsClient || MsConfig.IsServer)
            {
                //微服务请求头。
                message.Headers.Add(MsConst.HeaderKey, (MsConfig.IsClient ? MsConfig.Client.RcKey : MsConfig.Server.RcKey));
            }

            if (HttpContext.Current != null)
            {
                //分布式请求追踪ID。
                message.Headers.Add("X-Request-ID", HttpContext.Current.GetTraceID());
            }
            //message.Headers.Add("X-Real-IP", MvcConst.HostIP);
            //if (HttpContext.Current != null && HttpContext.Current.Request != null)
            //{
            //    message.Headers.Add("Referer", HttpContext.Current.Request.Url.AbsoluteUri);//当前运行地址。
            //}
            //else if (!string.IsNullOrEmpty(MvcConfig.RunUrl))
            //{
            //    message.Headers.Add("Referer", MvcConfig.RunUrl);//当前运行地址。
            //}
            if (task.Request.Header != null && task.Request.Header.Count > 0)
            {
                foreach (var item in task.Request.Header)
                {
                    message.Headers.Add(item.Key, item.Value);
                }
            }

        }

        /// <summary>
        /// HttpClient 并发性能太低，换高并发性能网关版本
        /// </summary>
        /// <param name="task"></param>
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
                MsLog.Write(err.Message, task.Request.Url, task.Request.Method);
            }
        }

        /// <summary>
        /// 后续再测试。
        /// </summary>
        /// <param name="task"></param>
        public static void ExeTaskAsync3(RpcTask task)
        {
            task.State = RpcTaskState.Running;
            Uri uri = task.Request.Uri;
            RpcClient wc = RpcClientPool.Create(uri);

            if (wc == null) { return; }
            byte[] bytes = null;
            try
            {
                if (task.Request.Header != null)
                {
                    foreach (string key in task.Request.Header.Keys)
                    {
                        if (key.StartsWith(":"))//chrome 新出来的 :method等
                        {
                            continue;
                        }
                        switch (key)
                        {
                            case "Connection"://引发异常 链接已关闭
                            case "Accept-Encoding"://引发乱码
                            case "Accept"://引发下载类型错乱
                                          //case "Referer":
                                break;
                            case "Host":
                                if (uri.Scheme == "https")
                                {
                                    //https 不调整 Host，可能会抛以下异常导致无法正常访问。
                                    //The SSL connection could not be established, see inner exception.
                                    wc.Headers.Add(key, uri.Host + (uri.Port == 443 ? "" : ":" + uri.Port));
                                }
                                break;
                            default:
                                wc.Headers.Add(key, task.Request.Header[key]);
                                break;
                        }
                    }
                }

                switch (task.Request.Method.ToUpper())
                {
                    case "GET":
                        bytes = wc.DownloadData(uri.ToString());
                        break;
                    case "HEAD":
                        wc.Head(uri.ToString());
                        break;
                    default:
                        byte[] data = task.Request.Data;
                        if (data == null)
                        {
                            data = new byte[0];
                        }
                        bytes = wc.UploadData(uri.ToString(), task.Request.Method, data);
                        break;
                }
                try
                {
                    foreach (string key in wc.ResponseHeaders.Keys)
                    {
                        //chrome 新出来的 :method等
                        //"Transfer-Encoding" 输出这个会造成时不时的503
                        if (key.StartsWith(":") || key == "Transfer-Encoding")
                        {
                            continue;
                        }
                        string value = wc.ResponseHeaders[key];
                        if (key == "Set-Cookie")
                        {
                            //处理切换域名
                            if (value.Contains("domain=" + uri.Host))
                            {
                                value = value.Replace("domain=" + uri.Host, "domain=" + uri.Host);
                            }
                        }
                        task.Result = new RpcTaskResult();
                        task.Result.IsSuccess = true;
                        task.Result.Header.Add(key, value);
                    }

                }
                catch (Exception err)
                {
                    Log.Write(err.Message, "Rpc");
                }

            }
            catch (Exception err)
            {
                RpcClientPool.RemoveFromPool(uri);
                MsLog.Write(err.Message, uri.ToString(), task.Request.Method);
            }
            finally
            {
                //if (wc.ResponseStatusCode > 0)
                //{
                //    context.Response.StatusCode = wc.ResponseStatusCode;
                //}
                RpcClientPool.AddToPool(uri, wc);
                if (bytes != null && bytes.Length > 0)
                {
                    task.Result.ResultByte = bytes;
                }
                task.State = RpcTaskState.Complete;
            }
        }
    }
}