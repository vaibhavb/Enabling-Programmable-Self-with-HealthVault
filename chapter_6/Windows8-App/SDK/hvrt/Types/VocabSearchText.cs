// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public enum VocabMatchType
    {
        [XmlEnum] Prefix,
        [XmlEnum] Contains,
        [XmlEnum] FullText,
    }

    public sealed class VocabSearchText : IHealthVaultType, IConstrainedString
    {
        private ConstrainedStringImpl m_value = new ConstrainedStringImpl(1, 255);

        public VocabSearchText()
        {
            Value = String.Empty;
        }

        public VocabSearchText(string text)
        {
            MatchType = VocabMatchType.FullText;
            Value = text;
        }

        [XmlAttribute("search-mode")]
        public VocabMatchType MatchType { get; set; }

        #region IConstrainedString Members

        [XmlIgnore]
        public int Length
        {
            get { return m_value.Length; }
        }

        [XmlIgnore]
        public int MinLength
        {
            get { return m_value.MinLength; }
        }

        [XmlIgnore]
        public int MaxLength
        {
            get { return m_value.MaxLength; }
        }

        [XmlText]
        public string Value
        {
            get { return m_value.Value; }
            set { m_value.Value = value; }
        }

        [XmlIgnore]
        public bool InRange
        {
            get { return m_value.InRange; }
        }

        #endregion

        #region IHealthVaultType Members

        public void Validate()
        {
            this.Validate("Value");
        }

        #endregion
    }
}