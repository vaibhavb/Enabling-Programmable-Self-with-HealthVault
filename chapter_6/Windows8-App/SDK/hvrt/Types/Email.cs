// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Email : IHealthVaultTypeSerializable
    {
        public Email()
        {
            Description = String.Empty;
        }

        public Email(string address)
        {
            Address = new EmailAddress(address);
        }

        [XmlElement("description", Order = 1)]
        public string Description { get; set; }

        [XmlElement("is-primary", Order = 2)]
        public BooleanValue IsPrimary { get; set; }

        [XmlElement("address", Order = 3)]
        public EmailAddress Address { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Address.ValidateRequired("Address");
        }

        #endregion
        
        public bool ShouldSerializeDescription()
        {
            return !String.IsNullOrEmpty(Description);
        }

        public static VocabIdentifier VocabForDescription()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.EmailTypes);
        }
    }
}