// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class RelatedItem : IHealthVaultTypeSerializable
    {
        public RelatedItem()
        {
            ItemID = String.Empty;
            Version = String.Empty;
            ClientID = String.Empty;
            RelationshipType = String.Empty;
        }

        [XmlElement("thing-id")]
        public string ItemID { get; set; }

        [XmlElement("version-stamp")]
        public string Version { get; set; }

        [XmlElement("client-thing-id")]
        public string ClientID { get; set; }

        [XmlElement("relationship-type")]
        public string RelationshipType { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
        }

        #endregion

        public bool ShouldSerializeItemID()
        {
            return !String.IsNullOrEmpty(ItemID);
        }

        public bool ShouldSerializeVersion()
        {
            return !String.IsNullOrEmpty(Version);
        }

        public bool ShouldSerializeClientID()
        {
            return !String.IsNullOrEmpty(ClientID);
        }

        public bool ShouldSerializeRelationshipType()
        {
            return !String.IsNullOrEmpty(RelationshipType);
        }
    }
}