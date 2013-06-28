// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class IsValidHealthVaultAccountResponse : IHealthVaultType
    {
        public IsValidHealthVaultAccountResponse()
        {
        }

        [XmlElement("account-status", Order = 1)]
        public bool Status
        {
            get;
            set;
        }

        [XmlElement("selected-instance", Order = 2)]
        public Instance SelectedInstance
        {
            get;
            set;
        }

        [XmlElement("account-info", Order = 3)]
        public AccountInfo AccountInfo
        {
            get;
            set;
        }

        public void Validate()
        {
            AccountInfo.ValidateRequired("AccountInfo");
        }
    }
}