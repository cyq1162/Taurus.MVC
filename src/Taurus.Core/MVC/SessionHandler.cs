﻿using System;
using System.Web;
using System.Web.SessionState;

namespace Taurus.Mvc
{
    /// <summary>
    /// 实现IHttpModule中使用Session
    /// </summary>
    internal class SessionHandler : IHttpHandler, IRequiresSessionState
    {
        internal static readonly SessionHandler Instance = new SessionHandler();
        public bool IsReusable
        {
            get
            {
                return true;
            }
        }

        public void ProcessRequest(HttpContext context)
        {

        }
    }
}
