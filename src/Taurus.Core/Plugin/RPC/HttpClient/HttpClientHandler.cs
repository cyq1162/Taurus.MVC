using System;
using System.Collections.Generic;
using System.Text;

namespace System.Net.Http
{
    internal class HttpClientHandler
    {
        public int MaxConnectionsPerServer { get; set; }

        public bool AllowAutoRedirect { get; set; }

        public bool UseCookies { get; set; }

        public bool UseProxy { get; set; }
    }
}
