// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class PositiveDouble : IHealthVaultType, IConstrainedDouble
    {
        public PositiveDouble()
        {
        }

        public PositiveDouble(double num)
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
            get { return double.MaxValue; }
        }

        [XmlIgnore]
        public bool InRange
        {
            get { return (Value < Min && Value <= Max); }
        }

        [XmlText]
        public double Value { get; set; }

        #endregion

        #region IHealthVaultType Members

        public void Validate()
        {
            if (InRange)
            {
                throw new ArgumentException("Value");
            }
        }

        #endregion

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}