using System;
using System.Web;
using System.Collections.Generic;
using Taurus.Mvc;
using CYQ.Data;
using System.Threading;
using CYQ.Data.Tool;
using System.Net;
using Taurus.Plugin.Rpc;

namespace Taurus.Plugin.MicroService
{

    /// <summary>
    /// 链接预处理
    /// </summary>
    public static partial class Gateway
    {
        /// <summary>
        /// 已检测列表
        /// </summary>
        private static MDictionary<Uri, bool> preConnectionDic = new MDictionary<Uri, bool>();

        internal static void PreConnection(MDictionary<string, List<HostInfo>> keyValues)
        {
            Dictionary<string, byte> keyValuePairs = new Dictionary<string, byte>();
            foreach (var items in keyValues)
            {
                string lowerKey = items.Key.ToLower();
                if (lowerKey == MsConst.RegistryCenter || lowerKey == MsConst.RegistryCenterOfSlave || lowerKey == MsConst.Gateway || lowerKey.Contains("."))
                {
                    continue;//不需要对服务端进行预请求，域名也不需要进行。
                }
                foreach (var info in items.Value)
                {
                    if (!keyValuePairs.ContainsKey(info.Host))
                    {
                        keyValuePairs.Add(info.Host, 1);
                        Gateway.PreConnection(info);//对于新加入的请求，发起一次请求建立预先链接。
                    }
                }
            }
            keyValuePairs.Clear();
        }

        /// <summary>
        /// 预先建立链接【每次都会重新检测】
        /// </summary>
        internal static void PreConnection(HostInfo info)
        {
            Thread thread = new Thread(new ParameterizedThreadStart(PreConnectionOnThread));
            thread.IsBackground = true;
            thread.Start(info);
        }
        private static void PreConnectionOnThread(object uriObj)
        {

            HostInfo info = (HostInfo)uriObj;

            RpcTaskRequest request = new RpcTaskRequest();
            request.HttpMethod = "HEAD";
            request.Url = info.Host;
            request.Timeout = 2500;//超时设定。
            RpcTaskResult result = Rpc.Rest.StartTask(request);
            var uri = request.Uri;
            if (result.IsSuccess)
            {
                if (!preConnectionDic.ContainsKey(uri))
                {
                    preConnectionDic.Add(uri, true);
                }
                else
                {
                    preConnectionDic[uri] = true;
                }
                info.State = 1;
            }
            else
            {
                // 记录错误没有意义。
                info.State = -1;
                if (uri != null)
                {
                    if (preConnectionDic.ContainsKey(uri))
                    {
                        preConnectionDic[uri] = false;
                    }
                    else
                    {
                        preConnectionDic.Add(uri, false);
                    }
                }
            }
        }
    }


}
