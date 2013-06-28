// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class StructuredMeasurement : IHealthVaultTypeSerializable
    {
        public StructuredMeasurement()
        {
        }

        public StructuredMeasurement(double val, string unitsText)
            : this(val, new CodableValue(unitsText))
        {
        }

        public StructuredMeasurement(double val, string unitsText, string unitsCode, string unitsVocab)
            : this(val, new CodableValue(unitsText, unitsCode, unitsVocab))
        {
        }

        internal StructuredMeasurement(double val, CodableValue units)
        {
            if (units == null)
            {
                throw new ArgumentNullException("units");
            }
            Value = val;
            Units = units;
        }

        [XmlElement("value", Order = 1)]
        public double Value { get; set; }

        [XmlElement("units", Order = 2)]
        public CodableValue Units { get; set; }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            Units.ValidateRequired("Units");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion

        public override string ToString()
        {
            if (Units != null)
            {
                return string.Format("{0} {1}", Value, Units);
            }

            return Value.ToString();
        }
    }
}