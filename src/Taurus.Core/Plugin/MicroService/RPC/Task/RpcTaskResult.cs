using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Taurus.Plugin.MicroService
{
    
    /// <summary>
    /// Rpc 调用返回结果【由任务调用后内部返回】
    /// </summary>
    public class RpcTaskResult
    {
        internal RpcTaskResult()
        {

        }
        /// <summary>
        /// 调用是否成功
        /// </summary>
        public bool IsSuccess { get; set; }
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
        private Dictionary<string, string> _Header = new Dictionary<string, string>();
        /// <summary>
        /// 调用返回的请求头
        /// </summary>
        public Dictionary<string, string> Header { get { return _Header; } set { _Header = value; } }

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
