// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class GeneralMeasurement : IHealthVaultTypeSerializable
    {
        public GeneralMeasurement()
        {
            Display = String.Empty;
        }

        [XmlElement("display", Order = 1)]
        public string Display { get; set; }

        [XmlElement("structured", Order = 2)]
        public StructuredMeasurement[] Structured { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Display.ValidateRequired("Display");
            Structured.ValidateOptional("Structured");
        }

        #endregion

        public bool ShouldSerializeDisplay()
        {
            return !String.IsNullOrEmpty(Display);
        }

        public override string ToString()
        {
            return Display ?? String.Empty;
        }
    }
}
