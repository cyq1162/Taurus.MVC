using CYQ.Data.Tool;
using System;
using System.Collections.Generic;
using System.Net.Http;

namespace Taurus.Plugin.MicroService
{
    internal class HttpClientPool
    {
        static MDictionary<string, List<HttpClient>> rpcClientPool = new MDictionary<string, List<HttpClient>>();

        public static HttpClient Create(Uri uri, int timeout)
        {
            if (uri == null) { return null; }
            if (!rpcClientPool.ContainsKey(uri.Authority))
            {
                HttpClient httpClient = new HttpClient();
                List<HttpClient> list = new List<HttpClient>();
                list.Add(httpClient);//默认。
                if (timeout > 0)//带超时的。
                {
                    HttpClient httpClient2 = new HttpClient();
                    httpClient2.Timeout = TimeSpan.FromMilliseconds(timeout);
                    list.Add(httpClient2);
                }
                rpcClientPool.Add(uri.Authority, list);
                return httpClient;
            }
            else
            {
                List<HttpClient> list = rpcClientPool[uri.Authority];
                if (timeout == 0)
                {
                    if (list.Count > 0)
                    {
                        return list[0];
                    }
                }
                foreach (var item in list)
                {
                    if (item.Timeout == TimeSpan.FromMilliseconds(timeout))
                    {
                        return item;
                    }
                }
                HttpClient httpClient = new HttpClient();
                httpClient.Timeout = TimeSpan.FromMilliseconds(timeout);
                list.Add(httpClient);
                return httpClient;
            }
        }
    }
}
