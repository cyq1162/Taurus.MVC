using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Taurus.Plugin.DistributedTransaction;

namespace Microsoft.AspNetCore.Http
{
    public static partial class DTCExtensions
    {
        public static void AddTaurusDtc(this IServiceCollection services)
        {
            services.AddHttpContext();
        }

        public static IApplicationBuilder UseTaurusDtc(this IApplicationBuilder builder)
        {
            return UseTaurusDtc(builder, StartType.None);
        }
        public static IApplicationBuilder UseTaurusDtc(this IApplicationBuilder builder, StartType startType)
        {
            builder.UseHttpContext();
            switch (startType)
            {
                case StartType.Client:
                    DTC.Client.Start();
                    break;
                case StartType.Server:
                    DTC.Server.Start();
                    break;
                case StartType.Both:
                    DTC.Start();
                    break;
            }
            return builder;
        }
    }
    public enum StartType
    {
        /// <summary>
        /// 不设定，应用程序启动或重启时，不先启动数据扫描，由程序涉及调用相关函数时自动启动数据扫描。
        /// </summary>
        None,
        /// <summary>
        /// 启动时，进行客户端数据扫描。
        /// </summary>
        Client,
        /// <summary>
        /// 启动时，进行服务端数据扫描。
        /// </summary>
        Server,
        /// <summary>
        /// 启动时，对客户端和服务端都启动数据扫描。
        /// </summary>
        Both
    }
}
