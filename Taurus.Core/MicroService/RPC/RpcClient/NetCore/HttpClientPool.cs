using CYQ.Data.Tool;
using System;
using System.Net.Http;

namespace Taurus.MicroService
{
    internal class HttpClientPool
    {
        static MDictionary<string, HttpClient> rpcClientPool = new MDictionary<string, HttpClient>();

        public static HttpClient Create(Uri uri)
        {
            if (!rpcClientPool.ContainsKey(uri.Authority))
            {
                HttpClient httpClient = new HttpClient();
                rpcClientPool.Add(uri.Authority, httpClient);
                return httpClient;
            }
            else
            {
                return rpcClientPool[uri.Authority];
            }
        }
    }
}
