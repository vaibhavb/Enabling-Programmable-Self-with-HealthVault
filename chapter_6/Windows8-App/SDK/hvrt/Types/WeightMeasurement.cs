// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class WeightMeasurement : IHealthVaultTypeSerializable
    {
        /// <summary>
        /// In Kg, always
        /// </summary>
        [XmlElement("kg", Order = 1)]
        public PositiveDouble Value { get; set; }

        [XmlElement("display", Order = 2)]
        public DisplayValue DisplayValue { get; set; }

        [XmlIgnore]
        public bool HasDisplayValue
        {
            get { return (DisplayValue != null); }
        }

        [XmlIgnore]
        public double InKg
        {
            get { return (Value != null) ? Value.Value : double.NaN; }
            set
            {
                Value = new PositiveDouble(value);
                DisplayValue = new DisplayValue(value, "kilogram", "kg");
            }
        }

        [XmlIgnore]
        public double InGrams
        {
            get { return InKg*1000; }
            set
            {
                Value = new PositiveDouble(value/1000);
                DisplayValue = new DisplayValue(value, "gram", "g");
            }
        }

        [XmlIgnore]
        public double InMilligrams
        {
            get { return InGrams*1000; }
            set
            {
                Value = new PositiveDouble(value/(1000*1000));
                DisplayValue = new DisplayValue(value, "milligram", "mg");
            }
        }

        [XmlIgnore]
        public double InPounds
        {
            get { return KgToPounds(InKg); }
            set
            {
                Value = new PositiveDouble(PoundsToKg(value));
                DisplayValue = new DisplayValue(value, "pound", "lb");
            }
        }

        [XmlIgnore]
        public double InOunces
        {
            get { return InPounds*16; }
            set
            {
                Value = new PositiveDouble(PoundsToKg(value/16));
                DisplayValue = new DisplayValue(value, "ounce", "oz");
            }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Value.ValidateRequired("Kg");
            DisplayValue.ValidateOptional("DisplayValue");
        }

        #endregion

        private static double KgToPounds(double kg)
        {
            return kg*2.20462262185;
        }

        private static double PoundsToKg(double lbs)
        {
            return lbs*0.45359237;
        }
    }
}