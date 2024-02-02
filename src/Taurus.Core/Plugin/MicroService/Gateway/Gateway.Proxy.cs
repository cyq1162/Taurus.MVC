using System;
using System.Web;
using System.Collections.Generic;
using Taurus.Mvc;
using CYQ.Data;
using System.Threading;
using CYQ.Data.Tool;
using System.Net;
using Taurus.Plugin.Rpc;

namespace Taurus.Plugin.MicroService
{

    public static partial class Gateway
    {
        /// <summary>
        /// 最后一次网关处理转发的时间
        /// </summary>
        public static DateTime LastProxyTime = DateTime.Now;

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


        #region 网关代理。
        /// <summary>
        /// 网关代理：从微服务主机获得相关信息转发并自动转发
        /// </summary>
        public static bool Proxy(HttpContext context)
        {
            if (!MsConfig.IsServer)
            {
                return false;
            }

            Uri uri = context.Request.Url;
            string module = GetModuleName(uri);
            List<HostInfo> infoList = Server.GetHostListWithCache(uri, uri.Host, module);
            if (infoList == null || infoList.Count == 0)
            {
                return false;
            }

            int count = infoList.Count;
            int max = 3;//最多循环3个节点，避免长时间循环卡机。
            bool isRCOfMaster = MsConfig.IsRegistryCenterOfMaster;
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
                bool isNext = info.Version < 0 || info.CallTime > DateTime.Now || (isRCOfMaster && info.RegTime < DateTime.Now.AddSeconds(-10));
                if (isNext)//正常5-10秒注册1次。
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

        private static bool Proxy(HttpContext context, string httpHost, string module, bool isVirtual)
        {
            Uri uri = new Uri(httpHost);
            if (!preConnectionDic.ContainsKey(uri) || !preConnectionDic[uri])
            {
                return false;
            }
            string rawUrl = context.Request.RawUrl;
            if (isVirtual && rawUrl.ToLower().StartsWith("/" + module.ToLower()))
            {
                rawUrl = rawUrl.Substring(module.Length + 1);
            }
            string url = httpHost.TrimEnd('/') + rawUrl;

            //return YarpProxy(context, url);
            return Proxy(context, url);
        }

        /// <summary>
        /// 网关代理：向指定的Url发起请求，并返回进行输出。
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <param name="url">请求地址</param>
        /// <returns></returns>
        public static bool Proxy(HttpContext context, string url)
        {
            LastProxyTime = DateTime.Now;
            try
            {
                #region 生成请求

                HttpRequest httpRequest = context.Request;
                byte[] data = null;
                if (httpRequest.HttpMethod != "GET" && httpRequest.ContentLength > 0)
                {
                    #region 处理接收文件上传
                    //Synchronous operations are disallowed. Call ReadAsync or set AllowSynchronousIO to true instead.”
                    data = new byte[(int)httpRequest.ContentLength];
                    httpRequest.InputStream.Position = 0;// 需要启用：context.Request.EnableBuffering();
                    httpRequest.InputStream.Read(data, 0, data.Length);
                    if (httpRequest.InputStream.Position < httpRequest.ContentLength)
                    {
                        //Linux CentOS-8 大文件下读不全，会延时，导致：Unexpected end of Stream, the content may have already been read by another component.
                        int max = 0;
                        int timeout = MsConfig.Server.GatewayTimeout * 1000;
                        while (httpRequest.InputStream.Position < httpRequest.ContentLength)
                        {
                            max++;
                            if (max > timeout)//60秒超时
                            {
                                context.Response.StatusCode = 413;
                                context.Response.Write("Timeout : Unexpected end of Stream , request entity too large");
                                return true;
                            }
                            Thread.Sleep(1);
                            httpRequest.InputStream.Read(data, (int)httpRequest.InputStream.Position, data.Length - (int)httpRequest.InputStream.Position);
                        }
                    }
                    #endregion
                }

                RpcTaskRequest rpcRequest = new RpcTaskRequest();
                rpcRequest.HttpMethod = httpRequest.HttpMethod;
                rpcRequest.Url = url;
                rpcRequest.Data = data;
                rpcRequest.Headers.Add(MsConst.HeaderKey, MsConfig.Server.RcKey);
                Uri uri = new Uri(url);
                foreach (string key in httpRequest.Headers.Keys)
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
                                rpcRequest.Headers.Add(key, uri.Host + (uri.Port == 443 ? "" : ":" + uri.Port));
                            }
                            break;
                        default:
                            rpcRequest.Headers.Add(key, httpRequest.Headers[key]);
                            break;
                    }
                }
                string realIP = httpRequest.UserHostAddress;
                string realPort = httpRequest.ServerVariables["REMOTE_PORT"];
                string xForwardedFor = rpcRequest.Headers["X-Forwarded-For"];

                if (xForwardedFor != null)
                {
                    rpcRequest.Headers.Set("X-Forwarded-For", xForwardedFor + "," + realIP);
                }
                else
                {
                    rpcRequest.Headers.Add("X-Forwarded-For", realIP);
                }
                rpcRequest.Headers.Add("X-Real-IP", realIP);
                if (!string.IsNullOrEmpty(realPort))
                {
                    rpcRequest.Headers.Set("X-Real-Port", realPort);
                }
                rpcRequest.Headers.Add("X-Request-ID", context.GetTraceID());
                #endregion

                #region 发起请求
                RpcTaskResult rpcResult = Rpc.Rest.StartTask(rpcRequest);
                #endregion

                #region 输出请求结果
                if (rpcResult.IsSuccess)
                {
                    try
                    {
                        context.Response.StatusCode = rpcResult.StatusCode;
                        foreach (string key in rpcResult.Headers.Keys)
                        {
                            //chrome 新出来的 :method等
                            //"Transfer-Encoding" 输出这个会造成时不时的503
                            if (key.StartsWith(":") || key == "Transfer-Encoding")
                            {
                                continue;
                            }
                            string value = rpcResult.Headers[key];
                            if (key == "Set-Cookie")
                            {
                                //处理切换域名
                                if (value.Contains("domain=" + uri.Host))
                                {
                                    value = value.Replace("domain=" + uri.Host, "domain=" + httpRequest.Url.Host);
                                }
                            }
                            context.Response.AppendHeader(key, value);
                        }
                    }
                    finally
                    {
                        if (rpcResult.ResultByte != null && rpcResult.ResultByte.Length > 0)
                        {
                            context.Response.BinaryWrite(rpcResult.ResultByte);
                        }
                    }
                    return true;
                }
                #endregion

            }
            catch (Exception err)
            {
                Log.Write(err);
            }
            return false;
        }


        #endregion
    }
}
