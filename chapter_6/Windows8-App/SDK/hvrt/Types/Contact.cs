// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Contact : IHealthVaultTypeSerializable
    {
        [XmlElement("address", Order = 1)]
        public Address[] Address { get; set; }

        [XmlElement("phone", Order = 2)]
        public Phone[] Phone { get; set; }

        [XmlElement("email", Order = 3)]
        public Email[] Email { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Address.ValidateOptional("Address");
            Phone.ValidateOptional("Phone");
            Email.ValidateOptional("Email");
        }

        #endregion

        public static Contact Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Contact>(xml);
        }
    }
}