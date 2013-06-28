// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Address : IHealthVaultTypeSerializable
    {
        public Address()
        {
            Description = String.Empty;
            City = String.Empty;
            State = String.Empty;
            PostalCode = String.Empty;
            Country = String.Empty;
            County = String.Empty;
        }

        [XmlElement("description", Order = 1)]
        public string Description { get; set; }

        [XmlElement("is-primary", Order = 2)]
        public BooleanValue IsPrimary { get; set; }

        [XmlElement("street", Order = 3)]
        public string[] Street { get; set; }

        [XmlElement("city", Order = 4)]
        public string City { get; set; }

        [XmlElement("state", Order = 5)]
        public string State { get; set; }

        [XmlElement("postcode", Order = 6)]
        public string PostalCode { get; set; }

        [XmlElement("country", Order = 7)]
        public string Country { get; set; }

        [XmlElement("county", Order = 8)]
        public string County { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Street.ValidateRequired("Street");
            City.ValidateRequired("City");
            PostalCode.ValidateRequired("PostalCode");
            Country.ValidateRequired("Country");
        }

        #endregion

        public bool ShouldSerializeDescription()
        {
            return !String.IsNullOrEmpty(Description);
        }

        public bool ShouldSerializeCity()
        {
            return !String.IsNullOrEmpty(City);
        }

        public bool ShouldSerializeState()
        {
            return !String.IsNullOrEmpty(State);
        }

        public bool ShouldSerializePostalCode()
        {
            return !String.IsNullOrEmpty(PostalCode);
        }

        public bool ShouldSerializeCountry()
        {
            return !String.IsNullOrEmpty(Country);
        }

        public bool ShouldSerializeCounty()
        {
            return !String.IsNullOrEmpty(County);
        }

        public static VocabIdentifier VocabForCountry()
        {
            return new VocabIdentifier(VocabFamily.ISO, VocabName.Country);
        }
    }
}