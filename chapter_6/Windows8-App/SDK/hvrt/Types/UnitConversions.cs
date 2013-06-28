// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    [XmlType(RootElement)]
    public sealed class UnitConversions : IHealthVaultTypeSerializable
    {
        internal const string RootElement = "unit-conversions";

        [XmlElement("base-unit-conversion")]
        public BaseUnitConversion BaseUnitConversion { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            BaseUnitConversion.ValidateRequired("BaseUnitConversion");
        }

        #endregion
    }

    public sealed class BaseUnitConversion : IHealthVaultTypeSerializable
    {
        [XmlElement("linear-conversion")]
        public LinearConversion LinearConversion { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            LinearConversion.ValidateOptional("LinearConversion");
        }

        #endregion
    }

    public sealed class LinearConversion : IHealthVaultTypeSerializable
    {
        [XmlElement("multiplier")]
        public double Multiplier { get; set; }

        [XmlElement("offset")]
        public double Offset { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
        }

        #endregion
    }
}
