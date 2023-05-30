using Taurus.Plugin.MicroService;

namespace Taurus.Plugin.Admin
{
    /// <summary>
    /// 登陆退出
    /// </summary>
    internal partial class AdminController
    {
        /// <summary>
        /// 代理 停止 客户端
        /// </summary>
        public void StopClient(string host)
        {
            string url = host + MsConfig.Server.RcPath + "/exit?mskey=" + MsConfig.Server.RcKey;
            RpcTask task = Rpc.StartGetAsync(url);
            Write(task.Result.ResultText);
        }
    }
}
