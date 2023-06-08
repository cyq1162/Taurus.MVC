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
            #region 共用方法

            private static string GetModuleName(Uri uri)
            {
                string url = uri.LocalPath == "/" ? MvcConfig.DefaultUrl : uri.LocalPath;
                string[] items = url.TrimStart('/').Split('/');
                string module = items[0];
                if (items.Length == 1 && (string.IsNullOrEmpty(module) || module.Contains(".")))
                {
                    module = "/";
                }
                return module;
            }
            #endregion
            /// <summary>
            /// 最后一次网关处理转发的时间
            /// </summary>
            public static DateTime LastProxyTime = DateTime.Now;

            #region 网关代理。
            /// <summary>
            /// 网关代理转发方法
            /// </summary>
            public static bool Proxy(HttpContext context)
            {
                if (!MsConfig.IsServer)
                {
                    return false;
                }

                Uri uri = context.Request.Url;
                string module = GetModuleName(uri);
                List<HostInfo> infoList = Server.Gateway.GetHostListWithCache(uri, uri.Host, module);
                if (infoList == null || infoList.Count == 0)
                {
                    return false;
                }

                int count = infoList.Count;
                int max = 3;//最多循环3个节点，避免长时间循环卡机。
                bool isRegCenterOfMaster = MsConfig.IsRegCenterOfMaster;
                string runUrl = MvcConfig.RunUrl;
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
                        callIndex = callIndex - count;//溢出后重置循环
                    }
                    HostInfo info = infoList[callIndex];//并发下有异步抛出
                    if (info.Version < 0 || info.CallTime > DateTime.Now || (isRegCenterOfMaster && info.RegTime < DateTime.Now.AddSeconds(-10)))//正常5-10秒注册1次。
                    {
                        continue;//已经断开服务的。
                    }
                    firstInfo.CallIndex = callIndex + 1;//指向下一个。
                    if (Proxy(context, info.Host, module, info.IsVirtual))
                    {
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

            public static bool Proxy(HttpContext context, string httpHost, string module, bool isVirtual)
            {
                Uri uri = new Uri(httpHost);
                if (!preConnectionDic.ContainsKey(uri) || !preConnectionDic[uri])
                {
                    return false;
                }

                HttpRequest request = context.Request;


                LastProxyTime = DateTime.Now;
                byte[] bytes = null, data = null;
                string rawUrl = request.RawUrl;
                if (isVirtual && rawUrl.ToLower().StartsWith("/" + module.ToLower()))
                {
                    rawUrl = rawUrl.Substring(module.Length + 1);
                }
                string url = httpHost.TrimEnd('/') + rawUrl;
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
                    wc.Headers.Add(MsConst.HeaderKey, MsConfig.Server.RcKey);
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
                    wc.Headers.Add("X-Request-ID", context.GetTraceID());
                    switch (request.HttpMethod)
                    {
                        case "GET":
                            bytes = wc.DownloadData(url);
                            break;
                        case "HEAD":
                            wc.Head(url);
                            break;
                        default:
                            if (data == null)
                            {
                                data = new byte[0];
                            }
                            bytes = wc.UploadData(url, request.HttpMethod, data);
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
                                    value = value.Replace("domain=" + uri.Host, "domain=" + request.Url.Host);
                                }
                            }
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
                    MsLog.Write(err.Message, url, request.HttpMethod);
                    return false;
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
                Uri uri = null;
                RpcClient wc = null;
                try
                {
                    uri = new Uri(info.Host);
                    if (uri == null) { return; }
                    wc = RpcClientPool.Create(uri);
                    if (wc == null) { return; }
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
                    if(uri!= null)
                    {
                        if (preConnectionDic.ContainsKey(uri))
                        {
                            preConnectionDic[uri] = false;
                        }
                        else
                        {
                            preConnectionDic.Add(uri, false);
                        }
                    }
                    //if (uri == null) // 记录错误没有意义。
                    //{
                    //    MsLog.Write(err.Message, "MicroService.Run.PreConnection(" + info.Host + ")", "GET", MsConfig.Server.Name);
                    //}
                    //else
                    //{
                        
                    //    MsLog.Write(err.Message, "MicroService.Run.PreConnection(" + uri.AbsoluteUri + ")", "GET", MsConfig.Server.Name);
                    //}

                }
                finally
                {
                    if (uri != null && wc != null)
                    {
                        RpcClientPool.AddToPool(uri, wc);
                    }
                }
            }
        }

    }
}
