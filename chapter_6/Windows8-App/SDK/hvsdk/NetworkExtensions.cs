// (c) Microsoft. All rights reserved
using Windows.Networking;
using Windows.Networking.Connectivity;

namespace HealthVault.Foundation
{
    public static class NetworkExtensions
    {
        public static string GetMachineName()
        {
            foreach (HostName hostname in NetworkInformation.GetHostNames())
            {
                if (hostname.DisplayName.Contains(".local"))
                {
                    return hostname.DisplayName.Remove(hostname.DisplayName.IndexOf(".local"));
                }
            }

            return null;
        }
    }
}