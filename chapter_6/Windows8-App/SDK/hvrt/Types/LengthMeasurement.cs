// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class LengthMeasurement : IHealthVaultTypeSerializable
    {
        [XmlElement("m", Order = 1)]
        public PositiveDouble Value { get; set; }

        [XmlElement("display", Order = 2)]
        public DisplayValue DisplayValue { get; set; }

        [XmlIgnore]
        public bool HasDisplayValue
        {
            get { return (DisplayValue != null); }
        }

        [XmlIgnore]
        public double InMeters
        {
            get { return (Value != null) ? Value.Value : double.NaN; }
            set
            {
                Value = new PositiveDouble(value);
                DisplayValue = new DisplayValue(value, "meters", "m");
            }
        }

        [XmlIgnore]
        public double InCentimeters
        {
            get { return InMeters*100; }
            set
            {
                Value = new PositiveDouble(value/100);
                DisplayValue = new DisplayValue(value, "centimeters", "cm");
            }
        }

        [XmlIgnore]
        public double InKilometers
        {
            get { return InMeters/1000; }
            set
            {
                Value = new PositiveDouble(value*1000);
                DisplayValue = new DisplayValue(value, "kilometers", "km");
            }
        }

        [XmlIgnore]
        public double InInches
        {
            get { return (Value != null) ? MetersToInches(Value.Value) : double.NaN; }
            set
            {
                Value = new PositiveDouble(InchesToMeters(value));
                DisplayValue = new DisplayValue(value, "inches", "in");
            }
        }

        [XmlIgnore]
        public double InFeet
        {
            get { return InInches/12; }
            set
            {
                Value = new PositiveDouble(InchesToMeters(value*12));
                DisplayValue = new DisplayValue(value, "feet", "ft");
            }
        }

        [XmlIgnore]
        public double InMiles
        {
            get { return InFeet/5280; }
            set
            {
                Value = new PositiveDouble(InchesToMeters(value*5280*12));
                DisplayValue = new DisplayValue(value, "miles", "mi");
            }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Value.ValidateRequired("Value");
            DisplayValue.ValidateOptional("Display");
        }

        #endregion

        private static double CentimetersToInches(double cm)
        {
            return cm*0.3937;
        }

        private static double InchesToCentimeters(double inches)
        {
            return inches*2.54;
        }

        private static double MetersToInches(double meters)
        {
            return CentimetersToInches(meters*100);
        }

        private static double InchesToMeters(double inches)
        {
            return InchesToCentimeters(inches)/100;
        }
    }
}