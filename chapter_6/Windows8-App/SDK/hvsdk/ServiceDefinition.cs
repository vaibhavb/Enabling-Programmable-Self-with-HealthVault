// (c) Microsoft. All rights reserved

namespace HealthVault.Foundation
{
    public class ServiceDefinition
    {
        public const string DefaultLiveIdAuthPolicy = "HBI";

        public string LiveIdAuthPolicy { get; set; }

        public ServiceDefinition()
        {
            LiveIdAuthPolicy = "";
        }

        public ServiceDefinition(string liveIdAuthPolicy)
        {
            LiveIdAuthPolicy = liveIdAuthPolicy;
        }
    }
}
