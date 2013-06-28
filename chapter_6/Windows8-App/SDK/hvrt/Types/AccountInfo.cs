// (c) Microsoft. All rights reserved

using System;
using HealthVault.Foundation;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class AccountInfo : IHealthVaultTypeSerializable
    {
        public AccountInfo()
        {
            FirstName = String.Empty;
            LastName = String.Empty;
            Gender = String.Empty;
            Country = String.Empty;
            PostalCode = String.Empty;
        }

        [XmlElement("first-name", Order = 1)]
        public string FirstName
        {
            get;
            set;
        }

        [XmlElement("last-name", Order = 2)]
        public string LastName
        {
            get;
            set;
        }

        [XmlElement("contact-email", Order = 3)]
        public EmailAddress ContactEmail
        {
            get;
            set;
        }

        [XmlElement("birthdate", Order = 4)]
        public DateTime BirthDate
        {
            get;
            set;
        }

        [XmlElement("gender", Order = 5)]
        public string Gender
        {
            get;
            set;
        }
        
        [XmlElement("country", Order = 6)]
        public string Country
        {
            get;
            set;
        }

        [XmlElement("postal-code", Order = 7)]
        public string PostalCode
        {
            get;
            set;
        }

        public void Validate()
        {
            FirstName.ValidateRequired("Name");
            LastName.ValidateRequired("LastName");
            ContactEmail.ValidateRequired("ContactEmail");
            BirthDate.ValidateRequired("BirthDate");
            Gender.ValidateRequired("Gender");
            Country.ValidateRequired("Country");
        }

        public string Serialize()
        {
            return this.ToXml();
        }
    }
}