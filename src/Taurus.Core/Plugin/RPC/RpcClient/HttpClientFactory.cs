using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;

namespace Taurus.Plugin.Rpc
{
    /// <summary>
    /// HttpClient 默认为每个Uri创建一个Socket池。
    /// </summary>
    internal class HttpClientFactory
    {
        static MDictionary<string, HttpClient> clientDic = new MDictionary<string, HttpClient>();
        static HttpClientHandler defaultConfig = new HttpClientHandler()
        {
            // 禁用系统代理
            UseProxy = false,
            UseCookies = false,
            AllowAutoRedirect = false,
            MaxConnectionsPerServer = 2048
        };

        private static readonly object lockObj = new object();
        public static HttpClient Get(Uri uri, int timeout)
        {
            if (uri == null) { return null; }
            string key = uri.Authority + "-" + timeout;
            if (clientDic.ContainsKey(key))
            {
                return clientDic[key];
            }
            lock (lockObj)
            {
                if (!clientDic.ContainsKey(key))
                {
                    HttpClient httpClient = new HttpClient(defaultConfig);
                    if (timeout > 0)//带超时的。
                    {
                        httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                    }
                    clientDic.Add(key, httpClient);
                    return httpClient;
                }

            }
            return clientDic[key];
        }

    }
}
