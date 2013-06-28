// (c) Microsoft. All rights reserved
using System;
using Windows.System.UserProfile;

namespace HealthVault.Foundation
{
    public class AppInfo : IValidatable
    {
        public AppInfo()
        {
            CultureCode = Windows.Globalization.ApplicationLanguages.Languages[0];
            InstanceName = NetworkExtensions.GetMachineName() ?? "Windows 8";
        }

        public Guid MasterAppId { get; set; }

        public string AppName { get; set; }

        public string InstanceName { get; set; }

        public string CultureCode { get; set; }

        public bool IsMultiInstanceAware { get; set; }

        #region IValidatable Members

        public void Validate()
        {
            MasterAppId.ValidateRequired("MasterAppId");
            InstanceName.ValidateRequired("InstanceName");
        }

        #endregion
    }
}