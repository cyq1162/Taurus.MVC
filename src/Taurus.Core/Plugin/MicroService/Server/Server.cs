using System;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 网关或注册中心端编码
    /// </summary>
    internal partial class Server
    {
        #region 对外开放接口或属性

        private static int _IsLiveOfMasterRC = -1;

        /// <summary>
        /// 相对自身，是否存在其它主注册中心。
        /// </summary>
        public static bool IsLiveOfMasterRC
        {
            get
            {
                if (_IsLiveOfMasterRC == -1)
                {
                    _IsLiveOfMasterRC = MsConfig.IsRegCenterOfMaster ? 0 : 1;
                }
                return _IsLiveOfMasterRC == 1;
            }
            set
            {
                _IsLiveOfMasterRC = value ? 1 : 0;
            }
        }

        #endregion

        /// <summary>
        /// 作为注册中心时的最后更新标识.
        /// </summary>
        internal static long Tick = 0;
        /// <summary>
        /// 注册中心 - 数据是否发生改变
        /// </summary>
        internal static bool IsChange = false;

        private static string _Host2 = null;
        /// <summary>
        /// 注册中心【存档】故障转移备用链接。
        /// </summary>
        internal static string Host2
        {
            get
            {
                if (MsConfig.IsRegCenterOfSlave)
                {
                    return MsConfig.Server.RcUrl;//从注册中心备份也指向主链接
                }
                if (_Host2 == null)
                {
                    if (MsConfig.IsGateway)
                    {
                        //仅网关读取配置文件。
                        _Host2 = IO.Read(MsConst.ServerHost2Path);
                    }
                    else
                    {
                        _Host2 = string.Empty;//注册中心。
                    }
                }
                return _Host2;
            }
            set
            {
                if (!MsConfig.IsRegCenterOfSlave)
                {
                    _Host2 = value;
                    if (MsConfig.IsGateway)
                    {
                        if (value != _RcUrl)
                        {
                            //仅网关写入配置文件【不存和初始配置一致的链接】
                            IO.Write(MsConst.ServerHost2Path, value);
                        }
                    }
                }
            }
        }
        private static readonly string _RcUrl = MsConfig.Server.RcUrl;

        /// <summary>
        /// 上次和注册中心同步IP的时间。
        /// </summary>
        internal static DateTime SyncIPTime;

        /// <summary>
        /// 注册中心更新同步配置的时间。
        /// </summary>
        internal static DateTime SyncConfigTime;
    }
}
