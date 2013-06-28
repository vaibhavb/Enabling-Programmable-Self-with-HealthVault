// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class FoodEnergyValue : IHealthVaultTypeSerializable
    {
        public FoodEnergyValue()
        {
        }

        public FoodEnergyValue(double calories)
        {
            CaloriesValue = calories;
        }

        [XmlElement("calories", Order = 1)]
        public NonNegativeDouble Calories { get; set; }

        [XmlElement("display", Order = 2)]
        public DisplayValue Display { get; set; }

        [XmlIgnore]
        public double CaloriesValue
        {
            get { return (Calories != null) ? Calories.Value : double.NaN; }
            set
            {
                Calories = new NonNegativeDouble(value);
                Display = new DisplayValue(value, "cal");
            }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Calories.ValidateRequired("Calories");
            Display.ValidateOptional("Display");
        }

        #endregion
    }
}