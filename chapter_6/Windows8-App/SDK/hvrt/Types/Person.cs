// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Person : IHealthVaultTypeSerializable
    {
        public Person()
        {
            Organization = String.Empty;
            Training = String.Empty;
            IdentificationNumber = String.Empty;
        }

        [XmlElement("name", Order = 1)]
        public Name Name { get; set; }

        [XmlElement("organization", Order = 2)]
        public string Organization { get; set; }

        [XmlElement("professional-training", Order = 3)]
        public string Training { get; set; }

        [XmlElement("id", Order = 4)]
        public string IdentificationNumber { get; set; }

        [XmlElement("contact", Order = 5)]
        public Contact Contact { get; set; }

        [XmlElement("type", Order = 6)]
        public CodableValue Type { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Name.ValidateRequired("Name");
            Contact.ValidateOptional("Contact");
        }

        #endregion

        public static Person Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Person>(xml);
        }

        public bool ShouldSerializeOrganization()
        {
            return !String.IsNullOrEmpty(Organization);
        }

        public bool ShouldSerializeTraining()
        {
            return !String.IsNullOrEmpty(Training);
        }

        public bool ShouldSerializeIdentificationNumber()
        {
            return !String.IsNullOrEmpty(IdentificationNumber);
        }
    }
}