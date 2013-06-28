// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Phone : IHealthVaultTypeSerializable
    {
        public Phone()
        {
            Description = String.Empty;
            Number = String.Empty;
        }

        public Phone(string number) : this()
        {
            if (string.IsNullOrEmpty(number))
            {
                throw new ArgumentException("number");
            }

            Number = number;
        }

        [XmlElement("description", Order = 1)]
        public string Description { get; set; }

        [XmlElement("is-primary", Order = 2)]
        public BooleanValue IsPrimary { get; set; }

        [XmlElement("number", Order = 3)]
        public string Number { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Number.ValidateRequired("Number");
        }

        #endregion

        public bool ShouldSerializeDescription()
        {
            return !String.IsNullOrEmpty(Description);
        }

        public bool ShouldSerializeNumber()
        {
            return !String.IsNullOrEmpty(Number);
        }

        public static VocabIdentifier VocabForDescription()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.PhoneTypes);
        }
    }
}