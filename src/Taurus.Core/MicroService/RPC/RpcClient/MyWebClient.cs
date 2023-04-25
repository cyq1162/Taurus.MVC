using System;
using System.Net;

namespace Taurus.MicroService
{
    internal class MyWebClient : WebClient
    {
        private int _ResponseStatusCode = 0;
        public int ResponseStatusCode
        {
            get
            {
                return _ResponseStatusCode;
            }
            set
            {
                _ResponseStatusCode = value;
            }
        }
        bool isHeadRequest = false;
        protected override WebRequest GetWebRequest(Uri address)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(address);
            request.AllowAutoRedirect = false;
            if (isHeadRequest)
            {
                request.Method = "HEAD";
                isHeadRequest = false;
            }
            return request;
        }
        protected override WebResponse GetWebResponse(WebRequest request)
        {
            HttpWebResponse response = null;
            try
            {
                response = (HttpWebResponse)base.GetWebResponse(request);

                return response;
            }
            catch (WebException err)
            {
                if(err.Response==null)
                {
                    throw;
                }
                response = (HttpWebResponse)err.Response;
            }
            finally
            {
                if (response != null)
                {
                    _ResponseStatusCode = (int)response.StatusCode;
                }

            }
            return response;
        }
        public void Head(string url)
        {
            isHeadRequest = true;
            DownloadData(url);
        }
    }
}
