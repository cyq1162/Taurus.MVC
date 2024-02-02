using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Threading;

namespace Taurus.Plugin.Rpc
{
    
    /// <summary>
    /// Rpc 调用返回结果【由任务调用后内部返回】
    /// </summary>
    public class RpcTaskResult
    {

        /// <summary>
        /// 调用是否成功：Gets a value that indicates if the HTTP response was successful.
        /// A value that indicates if the HTTP response was successful.  
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 执行返回 Http 状态码。
        /// </summary>
        public int StatusCode { get; set; }
        /// <summary>
        /// 调用返回的数据
        /// </summary>
        public byte[] ResultByte { get; set; }
        /// <summary>
        /// 调用返回的文本（由Result UTF8转码）
        /// </summary>
        public string ResultText
        {
            get
            {
                if (ResultByte == null || ResultByte.Length == 0)
                {
                    return "";
                }
                return Encoding.UTF8.GetString(ResultByte);
            }
        }
        private WebHeaderCollection _Headers = new WebHeaderCollection();
        /// <summary>
        /// 调用返回的请求头
        /// </summary>
        public WebHeaderCollection Headers { get { return _Headers; } set { _Headers = value; } }

        /// <summary>
        /// 调用异常时信息
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// 异常信息
        /// </summary>
        public string ErrorText
        {
            get
            {
                if (Error != null)
                {
                    return Error.Message;
                }
                return string.Empty;
            }
            set
            {
                Error=new Exception(value);
            }
        }
    }
}
