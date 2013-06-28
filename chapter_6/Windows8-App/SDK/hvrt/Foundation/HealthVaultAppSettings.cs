// (c) Microsoft. All rights reserved

using Windows.Storage;

namespace HealthVault.Foundation
{
    public sealed class HealthVaultAppSettings
    {
        public HealthVaultAppSettings()
        {
        }

        public HealthVaultAppSettings(string masterAppId) 
            : this(masterAppId, ServiceInfo.USPPEServiceUrl, ServiceInfo.USPPEShellUrl)
        {
        }

        public HealthVaultAppSettings(string masterAppId, string serviceUrl, string shellUrl) :
            this(masterAppId, serviceUrl, shellUrl, true)
        {
        }

        public HealthVaultAppSettings(string masterAppId, string serviceUrl, string shellUrl, bool isMultiInstanceAware) :
            this(masterAppId, serviceUrl, shellUrl, isMultiInstanceAware, false, 0, true, ApplicationData.Current.LocalFolder)
        {
        }
        
        public HealthVaultAppSettings(string masterAppId,
            string serviceUrl,
            string shellUrl,
            bool isMultiInstanceAware,
            bool useEncryption,
            int maxCachedItems,
            bool useWebAuthBroker,
            StorageFolder folder)
        {
            WebAuthorizer = useWebAuthBroker ? (IWebAuthorizerProxy)new WebAuthorizer() : new BrowserWebAuthorizer();
            MasterAppId = masterAppId;
            ServiceUrl = serviceUrl;
            ShellUrl = shellUrl;
            IsMultiInstanceAware = isMultiInstanceAware;
            UseEncryption = useEncryption;
            MaxCachedItems = maxCachedItems;
            Folder = folder;
        }

        public string MasterAppId { get; set; }

        public int MaxCachedItems { get; set; }

        public bool UseEncryption { get; set; }

        public string ServiceUrl { get; set; }

        /// <summary>
        /// Indicates whether the application supports more than one HealthVault web-service
        /// instance. For more information see the
        /// <a href="http://go.microsoft.com/?linkid=9830913">Global HealthVault Architecture</a> article.
        /// </summary>
        public bool IsMultiInstanceAware { get; set; }

        public string ShellUrl { get; set; }

        public string LiveIdHostName { get; set; }

        public StorageFolder Folder { get; set; }

        public IWebAuthorizerProxy WebAuthorizer { get; set; }

        public bool IsFirstParty { get; set; }
    }
}