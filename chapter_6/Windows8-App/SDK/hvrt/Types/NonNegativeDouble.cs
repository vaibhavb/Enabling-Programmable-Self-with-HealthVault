// (c) Microsoft. All rights reserved

using System.Xml;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class NonNegativeDouble : IHealthVaultType, IConstrainedDouble
    {
        public NonNegativeDouble()
        {
        }

        public NonNegativeDouble(double num)
        {
            Value = num;
            Validate();
        }

        #region IConstrainedDouble Members

        [XmlIgnore]
        public double Min
        {
            get { return 0; }
        }

        [XmlIgnore]
        public double Max
        {
            get { return int.MaxValue; }
        }

        [XmlIgnore]
        public bool InRange
        {
            get { return this.CheckRange(); }
        }

        [XmlText]
        public double Value { get; set; }

        #endregion

        #region IHealthVaultType Members

        public void Validate()
        {
            this.Validate("Value");
        }

        #endregion

        internal static string ToTextValue(NonNegativeDouble obj)
        {
            return (obj != null) ? XmlConvert.ToString(obj.Value) : null;
        }

        internal static NonNegativeDouble FromText(string text)
        {
            if (string.IsNullOrEmpty(text))
            {
                return null;
            }

            return new NonNegativeDouble(XmlConvert.ToDouble(text));
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}