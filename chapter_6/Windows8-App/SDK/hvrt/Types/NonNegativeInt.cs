// (c) Microsoft. All rights reserved

using System.Xml;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class NonNegativeInt : IHealthVaultType, IConstrainedInt
    {
        public NonNegativeInt()
        {
        }

        public NonNegativeInt(int num)
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

        internal static string ToTextValue(NonNegativeInt obj)
        {
            return (obj != null) ? XmlConvert.ToString(obj.Value) : null;
        }

        internal static NonNegativeInt FromText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return new NonNegativeInt(XmlConvert.ToInt32(text));
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}