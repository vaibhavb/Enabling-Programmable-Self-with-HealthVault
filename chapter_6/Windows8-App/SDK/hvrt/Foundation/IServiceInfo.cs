// (c) Microsoft. All rights reserved

using HealthVault.Types;
namespace HealthVault.Foundation
{
    public interface IServiceInfo
    {
        string ServiceUrl { get; set; }

        string ShellUrl { get; set; }
    }

    internal class ServiceInfoProxy : ServiceInfo, IServiceInfo
    {
    }

    public sealed class ServiceFactory
    {
        public static IServiceInfo CreateServiceInfo()
        {
            return new ServiceInfoProxy();
        }

        public static IServiceInfo CreateServiceInfo(string serviceUrl, string shellUrl)
        {
            var serviceInfoProxy = new ServiceInfoProxy();
            serviceInfoProxy.ServiceUrl = serviceUrl;
            serviceInfoProxy.ShellUrl = shellUrl;
            return serviceInfoProxy;
        }

        public static IServiceInfo CreateServiceInfo(Instance instance)
        {
            var serviceInfoProxy = new ServiceInfoProxy();
            serviceInfoProxy.ServiceUrl = instance.PlatformUrl;
            serviceInfoProxy.ShellUrl = instance.ShellUrl;
            return serviceInfoProxy;
        }
    }
}