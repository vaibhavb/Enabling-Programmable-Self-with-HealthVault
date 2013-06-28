// (c) Microsoft. All rights reserved

using HealthVault.Foundation;
using System;
using System.Net;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    [XmlType("person-info")]
    public sealed class CreateAccountPersonInfo : IHealthVaultTypeSerializable
    {
        public CreateAccountPersonInfo()
        {
            DisplayName = String.Empty;
        }

        [XmlElement("name", Order = 1)]
        public string Name { get; set; }

        [XmlElement("record-display-name", Order = 2)]
        public string DisplayName { get; set; }

        [XmlElement("birthdate", Order = 3)]
        public DateTime BirthDate { get; set; }

        [XmlElement("contact-email", Order = 4)]
        public string ContactEmail { get; set; }

        [XmlElement("is-newsletter-subscriber", Order = 5)]
        public bool IsNewsletterSubscriber { get; set; }

        [XmlElement("preferred-culture", Order = 6)]
        public Culture PreferredCulture { get; set; }

        [XmlElement("preferred-uiculture", Order = 7)]
        public Culture PreferredUICulture { get; set; }

        [XmlElement("location", Order = 8)]
        public Location Location { get; set; }

        public void Validate()
        {
            Name.ValidateRequired("Name");
            BirthDate.ValidateRequired("BirthDate");
            ContactEmail.ValidateRequired("ContactEmail");
            PreferredCulture.ValidateRequired("PreferredCulture");
            PreferredUICulture.ValidateOptional("PreferredUICulture");
            Location.ValidateOptional("Location");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public bool ShouldSerializeDisplayName()
        {
            return !String.IsNullOrEmpty(DisplayName);
        }
    }
}