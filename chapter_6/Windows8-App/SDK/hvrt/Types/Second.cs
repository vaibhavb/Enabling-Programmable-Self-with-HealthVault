// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class Second : IConstrainedInt, IHealthVaultType
    {
        public Second()
        {
        }

        public Second(int val)
        {
            Value = val;
            Validate();
        }

        #region IConstrainedInt Members

        [XmlIgnore]
        public int Min
        {
            get { return 0; }
        }

        [XmlIgnore]
        public int Max
        {
            get { return 59; }
        }

        [XmlIgnore]
        public bool InRange
        {
            get { return this.CheckRange(); }
        }

        [XmlText]
        public int Value { get; set; }

        #endregion

        #region IHealthVaultType Members

        public void Validate()
        {
            this.Validate("Value");
        }

        #endregion

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}