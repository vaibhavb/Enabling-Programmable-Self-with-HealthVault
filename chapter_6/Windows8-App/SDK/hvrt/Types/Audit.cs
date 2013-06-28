// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Audit : IHealthVaultTypeSerializable
    {
        public Audit()
        {
            AppID = String.Empty;
            Action = String.Empty;
        }

        [XmlElement("timestamp", Order = 1)]
        public StructuredDateTime When { get; set; }

        [XmlElement("app-id", Order = 2)]
        public string AppID { get; set; }

        [XmlElement("audit-action", Order = 3)]
        public string Action { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
        }

        #endregion

        public bool ShouldSerializeAppID()
        {
            return !String.IsNullOrEmpty(AppID);
        }

        public bool ShouldSerializeAction()
        {
            return !String.IsNullOrEmpty(Action);
        }
    }
}