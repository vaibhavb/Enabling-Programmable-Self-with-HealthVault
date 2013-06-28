// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class BloodGlucoseMeasurement : IHealthVaultTypeSerializable
    {
        public BloodGlucoseMeasurement()
        {
        }

        public BloodGlucoseMeasurement(double mmolPerL)
        {
            Value = new PositiveDouble(mmolPerL);
        }

        [XmlElement("mmolPerL", Order = 1)]
        public PositiveDouble Value { get; set; }

        [XmlElement("display", Order = 2)]
        public DisplayValue Display { get; set; }
        
        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Value.ValidateRequired("Value");
            Display.ValidateOptional("Display");
        }

        #endregion
    }
}