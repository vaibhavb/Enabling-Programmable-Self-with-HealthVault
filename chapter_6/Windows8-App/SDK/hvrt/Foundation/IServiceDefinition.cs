// (c) Microsoft. All rights reserved

namespace HealthVault.Foundation
{
    public interface IServiceDefinition
    {
        string LiveIdAuthPolicy { get; set; }
    }

    internal class ServiceDefinitionProxy : ServiceDefinition, IServiceDefinition
    {
    }

    public sealed class ServiceDefinitionFactory
    {
        public static IServiceDefinition CreateServiceDefinition()
        {
            return new ServiceDefinitionProxy();
        }
    }
}
