// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class EmailAddress : IHealthVaultType, IConstrainedString
    {
        public EmailAddress()
        {
            Value = String.Empty;
        }

        public EmailAddress(string email)
        {
            Value = email;
            Validate();
        }

        #region IConstrainedString Members

        [XmlIgnore]
        public int Length
        {
            get { return (Value != null) ? Value.Length : 0; }
        }

        [XmlIgnore]
        public int MinLength
        {
            get { return 6; }
        }

        [XmlIgnore]
        public int MaxLength
        {
            get { return 128; }
        }

        [XmlIgnore]
        public bool InRange
        {
            get { return this.CheckRange(); }
        }

        [XmlText]
        public string Value { get; set; }

        #endregion

        #region IHealthVaultType Members

        public void Validate()
        {
            this.Validate("Value");
        }

        #endregion

        public override string ToString()
        {
            return Value;
        }

        public bool ShouldSerializeValue()
        {
            return !String.IsNullOrEmpty(Value);
        }
    }
}