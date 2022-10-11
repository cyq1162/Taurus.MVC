using System;
using System.Collections.Generic;
using System.Text;
using System.Net;

namespace Taurus.MicroService
{
    internal class RpcClient : WebClient
    {
        bool isHeadRequest = false;
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            //request.Proxy = null;
            if (isHeadRequest)
            {
                request.Method = "HEAD";
                isHeadRequest = false;
            }
            return request;
        }
        public void Head(string url)
        {
            isHeadRequest = true;
            DownloadData(url);
        }
    }
}
