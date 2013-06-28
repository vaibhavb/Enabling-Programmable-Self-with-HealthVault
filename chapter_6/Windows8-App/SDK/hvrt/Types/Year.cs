// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class Year : IConstrainedInt, IHealthVaultType
    {
        public Year()
        {
        }

        public Year(int val)
        {
            Value = val;
            Validate();
        }

        #region IConstrainedInt Members

        [XmlIgnore]
        public int Min
        {
            get { return 1000; }
        }

        [XmlIgnore]
        public int Max
        {
            get { return 9999; }
        }

        [XmlText]
        public int Value { get; set; }

        [XmlIgnore]
        public bool InRange
        {
            get { return this.CheckRange(); }
        }

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