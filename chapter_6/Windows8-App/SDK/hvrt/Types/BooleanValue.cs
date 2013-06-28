// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Types
{
    //
    // Nullable<T> is NOT allowed by WinRT
    //
    public sealed class BooleanValue : IHealthVaultType
    {
        public BooleanValue()
        {
        }

        public BooleanValue(bool val)
        {
            Value = val;
        }

        [XmlText]
        public bool Value { get; set; }

        #region IHealthVaultType Members

        public void Validate()
        {
        }

        #endregion

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}