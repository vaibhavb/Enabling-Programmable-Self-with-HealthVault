// (c) Microsoft. All rights reserved

using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class PositiveInt : IHealthVaultType, IConstrainedInt
    {
        public PositiveInt()
        {
        }

        public PositiveInt(int num)
        {
            Value = num;
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
            get { return int.MaxValue; }
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