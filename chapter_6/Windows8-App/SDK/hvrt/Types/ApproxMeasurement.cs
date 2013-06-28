// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ApproxMeasurement : IHealthVaultTypeSerializable
    {
        public ApproxMeasurement()
        {
            Display = String.Empty;
        }

        public ApproxMeasurement(string displayText)
            : this(displayText, null)
        {
        }

        public ApproxMeasurement(double val, string unitsText, string unitsCode, string unitsVocab)
            : this(null, new StructuredMeasurement(val, unitsText, unitsCode, unitsVocab))
        {
        }

        public ApproxMeasurement(string displayText, StructuredMeasurement measurement)
        {
            if (string.IsNullOrEmpty(displayText) && measurement != null)
            {
                displayText = measurement.ToString();
            }
            if (string.IsNullOrEmpty(displayText))
            {
                throw new ArgumentException("displayText");
            }
            Display = displayText;
            Measurement = measurement;
        }

        [XmlElement("display", Order = 1)]
        public string Display { get; set; }

        [XmlElement("structured", Order = 2)]
        public StructuredMeasurement Measurement { get; set; }

        [XmlIgnore]
        public bool HasMeasurement
        {
            get { return (Measurement != null); }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Display.ValidateRequired("Display");
            Measurement.ValidateOptional("Measurement");
        }

        #endregion

        public override string ToString()
        {
            if (Display != null)
            {
                return Display;
            }

            if (Measurement != null)
            {
                return Measurement.ToString();
            }

            return string.Empty;
        }

        public bool ShouldSerializeDisplay()
        {
            return !String.IsNullOrEmpty(Display);
        }
    }
}