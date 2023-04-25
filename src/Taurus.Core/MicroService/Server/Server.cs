using System;

namespace Taurus.MicroService
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
                if (_Host2 == null)
                {
                    _Host2 = IO.Read(MsConst.ServerHost2Path);//首次读取，以便于恢复。
                }
                return _Host2;
            }
            set
            {
                _Host2 = value;
                if (MsConfig.Server.Name.ToLower() == MsConst.Gateway)
                {
                    if (!string.IsNullOrEmpty(value))
                    {
                        IO.Write(MsConst.ServerHost2Path, value);
                    }
                    else
                    {
                        IO.Delete(MsConst.ServerHost2Path);
                    }
                }
            }
        }
        /// <summary>
        /// 备份链接最新注册时间
        /// </summary>
        internal static DateTime Host2LastRegTime = DateTime.MinValue;
    }
}
