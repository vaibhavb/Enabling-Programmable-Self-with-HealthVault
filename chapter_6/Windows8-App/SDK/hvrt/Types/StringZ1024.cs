// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class StringZ1024 : IHealthVaultType, IConstrainedString
    {
        private ConstrainedStringImpl m_value = new ConstrainedStringImpl(0, 1024);

        public StringZ1024()
        {
        }

        public StringZ1024(string val)
        {
            m_value.Value = val;
        }

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