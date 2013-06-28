// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    [XmlType("vocabulary-key")]
    public sealed class VocabIdentifier : IHealthVaultTypeSerializable
    {
        private string m_keyString;

        public VocabIdentifier()
        {
            Name = String.Empty;
            Family = String.Empty;
            Version = String.Empty;
            CodeValue = String.Empty;
            Language = String.Empty;
        }

        public VocabIdentifier(string family, string name) : this()
        {
            if (string.IsNullOrEmpty(family))
            {
                throw new ArgumentException("family");
            }
            if (string.IsNullOrEmpty(name))
            {
                throw new ArgumentException("name");
            }

            Name = name;
            Family = family;
        }

        [XmlElement("name", Order = 1)]
        public string Name { get; set; }

        [XmlElement("family", Order = 2)]
        public string Family { get; set; }

        [XmlElement("version", Order = 3)]
        public string Version { get; set; }

        [XmlElement("code-value", Order = 5)]
        public string CodeValue { get; set; }

        [XmlAttribute("xml:lang")]
        public string Language { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Name.ValidateRequired("Name");
        }

        #endregion

        public string GetKey()
        {
            if (m_keyString == null)
            {
                m_keyString = string.Format("{0}_{1}_{2}", Name, Family, Version);
            }

            return m_keyString;
        }

        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }

        public bool ShouldSerializeFamily()
        {
            return !String.IsNullOrEmpty(Family);
        }

        public bool ShouldSerializeVersion()
        {
            return !String.IsNullOrEmpty(Version);
        }

        public bool ShouldSerializeCodeValue()
        {
            return !String.IsNullOrEmpty(CodeValue);
        }

        public bool ShouldSerializeLanguage()
        {
            return !String.IsNullOrEmpty(Language);
        }
    }
}