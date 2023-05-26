using CYQ.Data;
using Taurus.Mvc;

namespace Taurus.Plugin.MicroService
{
    /// <summary>
    /// 对应【AppSettings】可配置项
    /// </summary>
    public static partial class MsConfig
    {

        #region 只读属性

        /// <summary>
        /// 当前程序是否作为客务端运行：微服务应用程序
        /// </summary>
        public static bool IsClient
        {
            get
            {
                return !string.IsNullOrEmpty(MsConfig.Client.Name) && !string.IsNullOrEmpty(MsConfig.Client.RcUrl) && MsConfig.Client.RcUrl != MvcConfig.RunUrl;
            }
        }

        /// <summary>
        /// 当前程序是否作为服务端运行：网关或注册中心
        /// </summary>
        public static bool IsServer
        {
            get
            {
                return IsRegCenter || IsGateway;
            }
        }
        /// <summary>
        /// 是否注册中心
        /// </summary>
        public static bool IsRegCenter
        {
            get
            {
                return MsConfig.Server.Name.ToLower() == MsConst.RegCenter;
            }
        }
        /// <summary>
        /// 是否网关中心
        /// </summary>
        public static bool IsGateway
        {
            get
            {
                return MsConfig.Server.Name.ToLower() == MsConst.Gateway;
            }
        }
        /// <summary>
        /// 是否注册中心（主）
        /// </summary>
        public static bool IsRegCenterOfMaster
        {
            get
            {
                return IsRegCenter && (string.IsNullOrEmpty(MsConfig.Server.RcUrl) || MsConfig.Server.RcUrl == MvcConfig.RunUrl);
            }
        }

        /// <summary>
        /// 是否注册中心（主）
        /// </summary>
        public static bool IsRegCenterOfSlave
        {
            get
            {
                return IsRegCenter && (!string.IsNullOrEmpty(MsConfig.Server.RcUrl) && MsConfig.Server.RcUrl != MvcConfig.RunUrl);
            }
        }
        #endregion


        #region 注册中心 - 数据库配置

        //private static string _MsConn = null;
        ///// <summary>
        ///// 微服务 - 注册中心  数据库链接配置
        ///// </summary>
        //public static string MsConn
        //{
        //    get
        //    {
        //        if (_MsConn == null)
        //        {
        //            _MsConn = AppConfig.GetConn("MsConn");
        //        }
        //        return _MsConn;
        //    }
        //    set
        //    {
        //        _MsConn = value;
        //    }
        //}

        //private static string _MsTableName;
        ///// <summary>
        ///// 异常日志表名（默认为MsRegCenter，可配置）
        ///// </summary>
        //public static string MsTableName
        //{
        //    get
        //    {
        //        if (string.IsNullOrEmpty(_MsTableName))
        //        {
        //            _MsTableName = AppConfig.GetApp("MsTableName", "MsRegCenter");
        //        }
        //        return _MsTableName;
        //    }
        //    set
        //    {
        //        _MsTableName = value;
        //    }
        //}
        #endregion
    }
}
