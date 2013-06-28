// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Language : IHealthVaultTypeSerializable
    {
        public Language()
        {
        }
        
        public Language(CodableValue language, bool isPrimary)
        {
            LanguageValue = language;
            IsPrimary = new BooleanValue(isPrimary);
        }

        [XmlElement("language", Order = 1)]
        public CodableValue LanguageValue { get; set; }

        [XmlElement("is-primary", Order = 2)]
        public BooleanValue IsPrimary { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            LanguageValue.ValidateOptional("LanguageValue");
            IsPrimary.ValidateOptional("IsPrimary");
        }

        #endregion
        
        public static VocabIdentifier VocabForLanguage()
        {
            return new VocabIdentifier(VocabFamily.ISO, VocabName.Language);
        }
    }
}