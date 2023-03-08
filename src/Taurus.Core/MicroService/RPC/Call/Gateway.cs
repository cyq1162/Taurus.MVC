using System;
using System.Web;
using System.Collections.Generic;
using System.Net;
using Taurus.Mvc;

namespace Taurus.MicroService
{
    public static partial class Rpc
    {
        /// <summary>
        /// 微服务的核心类：网关代理（请求转发）
        /// </summary>
        internal static class Gateway
        {

            #region 网关代理。
            /// <summary>
            /// 网关代理转发方法
            /// </summary>
            public static bool Proxy(HttpContext context, bool isServerCall)
            {
                if ((isServerCall && !MsConfig.IsServer) || (!isServerCall && !MsConfig.IsClient))
                {
                    return false;
                }
                List<HostInfo> infoList = new List<HostInfo>();
                string module = string.Empty;
                IPAddress iPAddress;
                List<HostInfo> domainList = null;
                if (context.Request.Url.Host != "localhost" && !IPAddress.TryParse(context.Request.Url.Host, out iPAddress))
                {
                    module = context.Request.Url.Host;//域名转发优先。
                    domainList = isServerCall ? Server.GetHostList(module) : Client.GetHostList(module);
                    if (domainList == null || domainList.Count == 0)
                    {
                        return false;
                    }
                }

                if (context.Request.Url.LocalPath == "/")
                {
                    module = MvcConfig.DefaultUrl.TrimStart('/').Split('/')[0];
                }
                else
                {
                    module = context.Request.Url.LocalPath.TrimStart('/').Split('/')[0];
                }
                List<HostInfo> moduleList = isServerCall ? Server.GetHostList(module) : Client.GetHostList(module);

                if (domainList == null || domainList.Count == 0) { infoList = moduleList; }
                else if (moduleList == null || moduleList.Count == 0) { infoList = domainList; }
                else
                {
                    foreach (var item in domainList)//过滤掉不在域名下的主机
                    {
                        foreach (var keyValue in moduleList)
                        {
                            if (item.Host == keyValue.Host)
                            {
                                infoList.Add(item);
                                break;
                            }
                        }
                    }
                }

                if (infoList == null || infoList.Count == 0)
                {
                    return false;
                }
                else
                {
                    int max = 3;//最多循环3个节点，避免长时间循环卡机。
                    bool isRegCenter = MsConfig.IsRegCenterOfMaster;
                    HostInfo firstInfo = infoList[0];
                    if (firstInfo.CallIndex >= infoList.Count)
                    {
                        firstInfo.CallIndex = 0;//处理节点移除后，CallIndex最大值的问题。
                    }
                    for (int i = 0; i < infoList.Count; i++)
                    {
                        int callIndex = firstInfo.CallIndex + i;
                        if (callIndex >= infoList.Count)
                        {
                            callIndex = callIndex - infoList.Count;
                        }
                        HostInfo info = infoList[callIndex];
                        if (!isServerCall && info.Host == MsConfig.AppRunUrl)
                        {
                            continue;
                        }
                        if (info.Version < 0 || (info.CallTime > DateTime.Now && infoList.Count > 0) || (isRegCenter && info.RegTime < DateTime.Now.AddSeconds(-10)))//正常5-10秒注册1次。
                        {
                            continue;//已经断开服务的。
                        }
                        if (Proxy(context, info.Host, isServerCall))
                        {
                            firstInfo.CallIndex = callIndex + 1;//指向下一个。
                            return true;
                        }
                        else
                        {
                            info.CallTime = DateTime.Now.AddSeconds(10);//网络异常的，延时10s检测。
                            max--;
                            if (max == 0)
                            {
                                return true;
                            }
                        }

                    }
                    return true;
                }
            }

            public static bool Proxy(HttpContext context, string host, bool isServerCall)
            {
                Uri uri = new Uri(host);
                HttpRequest request = context.Request;
                string url = String.Empty;
                byte[] bytes = null;
                url = host + request.RawUrl;
                RpcClient wc = RpcClientPool.Create(uri);
                try
                {
                    wc.Headers.Add(MsConst.HeaderKey, (isServerCall ? MsConfig.ServerKey : MsConfig.ClientKey));
                    wc.Headers.Add("X-Real-IP", request.UserHostAddress);
                    if (!string.IsNullOrEmpty(MsConfig.AppRunUrl))
                    {
                        wc.Headers.Add("Referer", MsConfig.AppRunUrl);//当前运行地址。
                    }
                    foreach (string key in request.Headers.Keys)
                    {
                        switch (key)
                        {
                            case "Connection"://引发异常 链接已关闭
                            case "Accept-Encoding"://引发乱码
                            case "Accept"://引发下载类型错乱
                            case "Referer":
                                break;
                            default:
                                wc.Headers.Add(key, request.Headers[key]);
                                break;
                        }

                    }
                    if (request.HttpMethod == "GET")
                    {
                        bytes = wc.DownloadData(url);
                    }
                    else
                    {
                        byte[] data = null;
                        if (request.ContentLength > 0)
                        {
                            //Synchronous operations are disallowed. Call ReadAsync or set AllowSynchronousIO to true instead.”
                            data = new byte[(int)request.ContentLength];
                            request.InputStream.Read(data, 0, data.Length);
                        }
                        bytes = wc.UploadData(url, request.HttpMethod, data);
                    }
                    try
                    {
                        //context.Response.AppendHeader("Content-Length", bytes.Length.ToString());
                        foreach (string key in wc.ResponseHeaders.Keys)
                        {
                            switch (key)
                            {
                                case "Transfer-Encoding"://输出这个会造成时不时的503
                                    continue;
                            }
                            if (key == "Content-Type" && wc.ResponseHeaders[key].Split(';').Length == 1)
                            {
                                continue;
                            }
                            context.Response.AppendHeader(key, wc.ResponseHeaders[key]);
                        }

                    }
                    catch
                    {

                    }
                    context.Response.BinaryWrite(bytes);
                    bytes = null;
                    return true;
                }
                catch (Exception err)
                {
                    if (err.Message.Contains("(404) Not Found"))
                    {
                        return true;
                    }
                    MsLog.Write(err.Message, url, request.HttpMethod, isServerCall ? MsConfig.ServerName : MsConfig.ClientName);
                    return false;
                }
                finally
                {
                    RpcClientPool.AddToPool(uri, wc);
                }
            }

            //private static bool Proxy(HttpContext context, string host, bool isServerCall)
            //{
            //    return Proxy3(context, host, isServerCall).Result;
            //}
            //private static async System.Threading.Tasks.Task<bool> Proxy3(HttpContext context, string host, bool isServerCall)
            //{
            //    HttpRequest request = context.Request;
            //    string url = String.Empty;
            //    try
            //    {
            //        byte[] bytes = null;

            //        url = host + request.RawUrl;//.Substring(module.Length + 1);
            //        HttpMessageHandler handler = null;
            //        using (HttpClient wc = new HttpClient())
            //        {
            //            wc.DefaultRequestHeaders.Add(Const.HeaderKey, (isServerCall ? MSConfig.ServerKey : MSConfig.ClientKey));
            //            wc.DefaultRequestHeaders.Add("X-Real-IP", request.UserHostAddress);
            //            if (!string.IsNullOrEmpty(MSConfig.AppRunUrl))
            //            {
            //                wc.DefaultRequestHeaders.Add("Referer", MSConfig.AppRunUrl);//当前运行地址。
            //            }
            //            foreach (string key in request.Headers.Keys)
            //            {
            //                switch (key)
            //                {
            //                    case "Connection"://引发异常 链接已关闭
            //                    case "Accept-Encoding"://引发乱码
            //                    case "Accept"://引发下载类型错乱
            //                    case "Referer":
            //                        break;
            //                    default:
            //                        wc.DefaultRequestHeaders.Add(key, request.Headers[key]);
            //                        break;
            //                }

            //            }
            //            HttpResponseMessage mes;
            //            if (request.HttpMethod == "GET")
            //            {
            //                mes = await wc.GetAsync(url);
            //            }
            //            else
            //            {
            //                byte[] data = null;
            //                if (request.ContentLength > 0)
            //                {
            //                    //Synchronous operations are disallowed. Call ReadAsync or set AllowSynchronousIO to true instead.”
            //                    data = new byte[(int)request.ContentLength];
            //                    request.InputStream.Read(data, 0, data.Length);
            //                }
            //                StreamContent content = new StreamContent(request.InputStream, (int)(request.ContentLength ?? 0));
            //                mes = await wc.PostAsync(url, content);
            //            }
            //            System.Threading.Tasks.Task<byte[]> bytes1 = mes.Content.ReadAsByteArrayAsync();
            //            bytes1.Wait();
            //            bytes = bytes1.Result;
            //            try
            //            {
            //                //context.Response.AppendHeader("Content-Length", bytes.Length.ToString());
            //                foreach (var key in mes.Headers)
            //                {
            //                    switch (key.Key)
            //                    {
            //                        case "Transfer-Encoding"://输出这个会造成时不时的503
            //                            continue;
            //                    }
            //                    string value = string.Empty;
            //                    foreach (var v in key.Value)
            //                    {
            //                        value = v;
            //                        break;
            //                    }
            //                    if (key.Key == "Content-Type" && value.Split(';').Length == 1)
            //                    {
            //                        continue;
            //                    }
            //                    context.Response.AppendHeader(key.Key, value);
            //                }

            //            }
            //            catch
            //            {

            //            }
            //        }
            //        context.Response.BinaryWrite(bytes);
            //        return true;
            //    }
            //    catch (Exception err)
            //    {
            //        LogWrite(err.Message, url, request.HttpMethod, isServerCall ? MSConfig.ServerName : MSConfig.ClientName);
            //        return false;
            //    }
            //}

            #endregion

        }

    }
}
