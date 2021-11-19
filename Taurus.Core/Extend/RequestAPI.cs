using System;
using System.Collections.Generic;
using System.Text;
using System.Web;
namespace Taurus.Core
{
    /// <summary>
    /// 记录API请求（未实现）
    /// </summary>
    internal class RequestAPI
    {
        /// <summary>
        /// 记录请求、并进行黑名单处理。
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public static bool Record(HttpContext context)
        {
            return true;
        }
        #region 黑名单
        public static void AddBlackname(string ip)
        {

        }
        #endregion
    }
}
