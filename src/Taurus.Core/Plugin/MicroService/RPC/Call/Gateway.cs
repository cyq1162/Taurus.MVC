using System;
using System.Web;
using System.Collections.Generic;
using Taurus.Mvc;
using CYQ.Data;
using System.Threading;
using CYQ.Data.Tool;

namespace Taurus.Plugin.MicroService
{
    public static partial class Rpc
    {
        /// <summary>
        /// 微服务的核心类：网关代理（请求转发）
        /// </summary>
        internal static partial class Gateway
        {
            /// <summary>
            /// 最后一次网关处理转发的时间
            /// </summary>
            public static DateTime LastProxyTime = DateTime.Now;

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

                Uri uri = context.Request.Url;
                List<HostInfo> domainList = isServerCall ? Server.Gateway.GetHostList(uri.Host) : Client.Gateway.GetHostList(uri.Host);
                if (domainList == null || domainList.Count == 0)
                {
                    return false;
                }

                string url = uri.LocalPath == "/" ? MvcConfig.DefaultUrl : uri.LocalPath;
                string[] items = url.TrimStart('/').Split('/');
                string module = items[0];
                if (items.Length == 1 && (string.IsNullOrEmpty(module) || module.Contains(".")))
                {
                    module = "/";
                }

                List<HostInfo> moduleList = isServerCall ? Server.Gateway.GetHostList(module) : Client.Gateway.GetHostList(module);
                if (moduleList == null || moduleList.Count == 0)
                {
                    return false;
                }
                List<HostInfo> infoList = new List<HostInfo>();
                //存在域名，也存在模块，过滤出满足：域名+模块
                foreach (var domainItem in domainList)//过滤掉不在域名下的主机
                {
                    foreach (var moduleItem in moduleList)
                    {
                        if (domainItem.Host == moduleItem.Host)
                        {
                            if (uri.AbsoluteUri.StartsWith(domainItem.Host))
                            {
                                return false;//请求自身，直接返回，避免死循环。
                            }
                            infoList.Add(moduleItem);// 用模块，模块里有包含IsVirtual属性，而域名则没有。
                            break;
                        }
                    }
                }
                if (infoList.Count == 0)
                {
                    return false;
                }

                int count = infoList.Count;
                int max = 3;//最多循环3个节点，避免长时间循环卡机。
                bool isRegCenter = MsConfig.IsRegCenterOfMaster;
                HostInfo firstInfo = infoList[0];
                if (firstInfo.CallIndex >= count)
                {
                    firstInfo.CallIndex = 0;//处理节点移除后，CallIndex最大值的问题。
                }
                for (int i = 0; i < count; i++)
                {
                    int callIndex = firstInfo.CallIndex + i;
                    if (callIndex >= count)
                    {
                        callIndex = callIndex - count;
                    }
                    //if (callIndex < 0 || callIndex >= infoList.Count)
                    //{

                    //}
                    HostInfo info = infoList[callIndex];//并发下有异步抛出
                    if (!isServerCall && info.Host == MvcConfig.RunUrl)
                    {
                        continue;
                    }
                    if (info.Version < 0 || info.CallTime > DateTime.Now || (isRegCenter && info.RegTime < DateTime.Now.AddSeconds(-10)))//正常5-10秒注册1次。
                    {
                        continue;//已经断开服务的。
                    }
                    if (Proxy(context, info.Host, module, info.IsVirtual, isServerCall))
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
                            context.Response.StatusCode = 502;
                            context.Response.Write("502 Bad gateway.");
                            return true;
                        }
                    }

                }
                context.Response.StatusCode = 502;
                context.Response.Write("502 Bad gateway.");
                return true;


            }

            public static bool Proxy(HttpContext context, string host, string module, bool isVirtual, bool isServerCall)
            {
                Uri uri = new Uri(host);
                if (!preConnectionDic.ContainsKey(uri) || !preConnectionDic[uri])
                {
                    return false;
                }

                HttpRequest request = context.Request;
                //if (request.Url.Authority == uri.Authority)
                //{
                //    return false;//请求自身，直接返回，避免死循环。
                //}

                LastProxyTime = DateTime.Now;
                byte[] bytes = null, data = null;
                string rawUrl = request.RawUrl;
                if (isVirtual && rawUrl.ToLower().StartsWith("/" + module.ToLower()))
                {
                    rawUrl = rawUrl.Substring(module.Length + 1);
                }
                string url = host.TrimEnd('/') + rawUrl;
                if (request.HttpMethod != "GET" && request.ContentLength > 0)
                {
                    //Synchronous operations are disallowed. Call ReadAsync or set AllowSynchronousIO to true instead.”
                    data = new byte[(int)request.ContentLength];
                    request.InputStream.Position = 0;// 需要启用：context.Request.EnableBuffering();
                    request.InputStream.Read(data, 0, data.Length);
                    if (request.InputStream.Position < request.ContentLength)
                    {
                        //Linux CentOS-8 大文件下读不全，会延时，导致：Unexpected end of Stream, the content may have already been read by another component.
                        int max = 0;
                        int timeout = MsConfig.Server.GatewayTimeout * 1000;
                        while (request.InputStream.Position < request.ContentLength)
                        {
                            max++;
                            if (max > timeout)//60秒超时
                            {
                                context.Response.StatusCode = 413;
                                context.Response.Write("Timeout : Unexpected end of Stream , request entity too large");
                                return true;
                            }
                            Thread.Sleep(1);
                            request.InputStream.Read(data, (int)request.InputStream.Position, data.Length - (int)request.InputStream.Position);
                        }
                    }
                }


                RpcClient wc = RpcClientPool.Create(uri);
                if (wc == null) { return false; }
                try
                {
                    wc.Headers.Add(MsConst.HeaderKey, (isServerCall ? MsConfig.Server.RcKey : MsConfig.Client.RcKey));
                    //if (!string.IsNullOrEmpty(MsConfig.App.RunUrl))
                    //{
                    //    wc.Headers.Add("Referer", MsConfig.App.RunUrl);//当前运行地址。
                    //}
                    foreach (string key in request.Headers.Keys)
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
                                wc.Headers.Add(key, request.Headers[key]);
                                break;
                        }
                    }
                    string realIP = request.UserHostAddress;
                    string realPort = request.ServerVariables["REMOTE_PORT"];
                    string xForwardedFor = wc.Headers["X-Forwarded-For"];

                    if (xForwardedFor != null)
                    {
                        wc.Headers.Set("X-Forwarded-For", xForwardedFor + "," + realIP);
                    }
                    else
                    {
                        wc.Headers.Add("X-Forwarded-For", realIP);
                    }
                    wc.Headers.Add("X-Real-IP", realIP);
                    if (!string.IsNullOrEmpty(realPort))
                    {
                        wc.Headers.Set("X-Real-Port", realPort);
                    }
                    if (request.HttpMethod == "GET")
                    {
                        bytes = wc.DownloadData(url);
                    }
                    else
                    {
                        if (data == null)
                        {
                            data = new byte[0];
                        }
                        bytes = wc.UploadData(url, request.HttpMethod, data);
                    }
                    try
                    {
                        //context.Response.AppendHeader("Content-Length", bytes.Length.ToString());
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
                                    value = value.Replace("domain=" + uri.Host, "domain=" + request.Url.Host);
                                }
                            }
                            //if (key == "Content-Type" && !value.Contains("html") && value.Split(';').Length == 1) // 不返回可能造成html显示成字符串内容。
                            //{
                            //    continue;
                            //}
                            context.Response.AppendHeader(key, value);
                        }

                    }
                    catch (Exception err)
                    {
                        Log.WriteLogToTxt(err.Message, "MicroService");
                    }

                }
                catch (Exception err)
                {
                    RpcClientPool.RemoveFromPool(uri);
                    MsLog.Write(err.Message, url, request.HttpMethod, isServerCall ? MsConfig.Server.Name : MsConfig.Client.Name);
                    return false;
                    //RpcClient 内部已重写处理，已无需要在外部进行判断。
                    //string msg = err.Message;/
                    //bool isHasStatusCode = msg.Contains("(1") || msg.Contains("(2") || msg.Contains("(3") || msg.Contains("(4") || msg.Contains("(5") || msg.Contains("(6");
                    //if (!isHasStatusCode || msg.Contains("Connection refused") || msg.Contains("Could not find file") || msg.Contains("无法连接到远程服务器") || msg.Contains("未能解析此远程名称") || msg.Contains("不知道这样的主机"))//!err.Message.Contains("(40")400 系列，机器是通的， 404) Not Found
                    //{
                    //    RpcClientPool.RemoveFromPool(uri);
                    //    MsLog.Write(msg, url, request.HttpMethod, isServerCall ? MsConfig.Server.Name : MsConfig.Client.Name);
                    //    return false;
                    //}
                    //else
                    //{
                    //    //解析状态码
                    //    int i = msg.IndexOf('(');
                    //    int end = msg.IndexOf(')', i);
                    //    if (end > i)
                    //    {
                    //        string code = msg.Substring(i + 1, end - i - 1);
                    //        context.Response.StatusCode = int.Parse(code);
                    //    }
                    //}
                }
                finally
                {
                    if (wc.ResponseStatusCode > 0)
                    {
                        context.Response.StatusCode = wc.ResponseStatusCode;
                    }
                    RpcClientPool.AddToPool(uri, wc);
                    if (bytes != null && bytes.Length > 0)
                    {
                        context.Response.BinaryWrite(bytes);
                    }

                }
                return true;

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

        /// <summary>
        /// 链接预处理
        /// </summary>
        internal static partial class Gateway
        {
            /// <summary>
            /// 已检测列表
            /// </summary>
            private static MDictionary<Uri, bool> preConnectionDic = new MDictionary<Uri, bool>();

            /// <summary>
            /// 预先建立链接【每次都会重新检测】
            /// </summary>
            internal static void PreConnection(HostInfo info)
            {
                Thread thread = new Thread(new ParameterizedThreadStart(PreConnectionOnThread));
                thread.IsBackground = true;
                thread.Start(info);
            }
            private static void PreConnectionOnThread(object uriObj)
            {
                HostInfo info = (HostInfo)uriObj;
                Uri uri = new Uri(info.Host);
                RpcClient wc = RpcClientPool.Create(uri);
                if (wc != null)
                {
                    try
                    {
                        wc.Timeout = 2500;//超时设定。
                        wc.DownloadData(uri.AbsoluteUri);
                        if (!preConnectionDic.ContainsKey(uri))
                        {
                            preConnectionDic.Add(uri, true);
                        }
                        else
                        {
                            preConnectionDic[uri] = true;
                        }
                        info.State = 1;
                    }
                    catch (Exception err)
                    {
                        info.State = -1;
                        if (preConnectionDic.ContainsKey(uri))
                        {
                            preConnectionDic[uri] = false;
                        }
                        else
                        {
                            preConnectionDic.Add(uri, false);
                        }
                        MsLog.Write(err.Message, "MicroService.Run.PreConnection(" + uri.AbsoluteUri + ")", "GET", MsConfig.Server.Name);
                    }
                    finally
                    {
                        RpcClientPool.AddToPool(uri, wc);
                    }
                }
            }

        }
    }
}
