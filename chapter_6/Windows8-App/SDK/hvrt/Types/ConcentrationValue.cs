// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ConcentrationValue : IHealthVaultTypeSerializable
    {
        public ConcentrationValue()
        {
        }

        public ConcentrationValue(double num)
        {
            Value = new NonNegativeDouble(num);
            Validate();
        }

        [XmlElement("mmolPerL", Order = 1)]
        public NonNegativeDouble Value { get; set; }

        [XmlElement("display", Order = 2)]
        public DisplayValue DisplayValue { get; set; }

        [XmlIgnore]
        public bool HasDisplayValue
        {
            get { return (DisplayValue != null); }
        }
        
        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Value.ValidateRequired("Value");
            DisplayValue.ValidateOptional("Display");
        }

        public override string ToString()
        {
            return DisplayValue != null && !String.IsNullOrEmpty(DisplayValue.Text) ?
                DisplayValue.Text :
                Value.ToString();
        }
    }
}