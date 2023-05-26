
namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 微服务应用程序编码
    /// </summary>
    internal partial class Client
    {
        #region 对外开放的方法或属性
        /// <summary>
        /// 注册中心是否在线
        /// </summary>
        public static bool IsLiveOfMasterRC = false;
        #endregion
        /// <summary>
        /// 读取：注册中心时的最后更新标识.
        /// </summary>
        internal static long Tick = 0;//从注册中心读取的标识号【用于发送做对比】
        /// <summary>
        /// 读取：注册中心【存档】故障转移备用链接。
        /// </summary>
        private static string _Host2 = null;
        /// <summary>
        /// 读取：从注册中心读取的备用链接
        /// </summary>
        internal static string Host2
        {
            get
            {
                if (_Host2 == null)
                {
                    _Host2 = IO.Read(MsConst.ClientHost2Path);//首次读取，以便于恢复。
                }
                return _Host2;
            }
            set
            {
                _Host2 = value;
                if (value != _RcUrl)
                {
                    //【不存和初始配置一致的链接】
                    IO.Write(MsConst.ClientHost2Path, value);
                }


            }
        }
        private static string _RcUrl = MsConfig.Client.RcUrl;
    }
}
