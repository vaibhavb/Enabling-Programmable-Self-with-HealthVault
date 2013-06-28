// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.ItemTypes;

namespace HealthVault.Types
{
    [XmlType(RootElement)]
    public sealed class NutritionInformation : IHealthVaultTypeSerializable
    {
        internal const string RootElement = "nutrition-info";
        
        [XmlElement("calories")]
        public NutritionInformationMeasurement Calories { get; set; }

        // In grams
        [XmlElement("water")]
        public NutritionInformationMeasurement Water { get; set; }

        [XmlElement("protein")]
        public NutritionInformationMeasurement Protein { get; set; }

        [XmlElement("total-fat")]
        public NutritionInformationMeasurement TotalFat { get; set; }

        [XmlElement("dietary-fiber")]
        public NutritionInformationMeasurement Fiber { get; set; }

        [XmlElement("sugars")]
        public NutritionInformationMeasurement Sugars { get; set; }

        [XmlElement("carbohydrates")]
        public NutritionInformationMeasurement Carbohydrates { get; set; }

        // In milligrams
        [XmlElement("calcium")]
        public NutritionInformationMeasurement Calcium { get; set; }

        [XmlElement("iron")]
        public NutritionInformationMeasurement Iron { get; set; }

        [XmlElement("magnesium")]
        public NutritionInformationMeasurement Magnesium { get; set; }

        [XmlElement("phosphorus")]
        public NutritionInformationMeasurement Phosphorus { get; set; }

        [XmlElement("potassium")]
        public NutritionInformationMeasurement Potassium { get; set; }

        [XmlElement("sodium")]
        public NutritionInformationMeasurement Sodium { get; set; }

        [XmlElement("zinc")]
        public NutritionInformationMeasurement Zinc { get; set; }

        // In milligrams
        [XmlElement("vitamin-c")]
        public NutritionInformationMeasurement VitaminC { get; set; }

        [XmlElement("thiamin")]
        public NutritionInformationMeasurement Thiamin { get; set; }

        [XmlElement("riboflavin")]
        public NutritionInformationMeasurement Riboflavin { get; set; }

        [XmlElement("niacin")]
        public NutritionInformationMeasurement Niacin { get; set; }

        [XmlElement("vitamin-b6")]
        public NutritionInformationMeasurement VitaminB6 { get; set; }

        [XmlElement("vitamin-e")]
        public NutritionInformationMeasurement VitaminE { get; set; }

        // In micrograms
        [XmlElement("vitamin-b12")]
        public NutritionInformationMeasurement VitaminB12 { get; set; }

        [XmlElement("vitamin-d")]
        public NutritionInformationMeasurement VitaminD { get; set; }

        [XmlElement("vitamin-k")]
        public NutritionInformationMeasurement VitaminK { get; set; }

        // In mcg_DFE
        [XmlElement("folate")]
        public NutritionInformationMeasurement Folate { get; set; }

        // In mcg_RAE
        [XmlElement("vitamin-a")]
        public NutritionInformationMeasurement VitaminA { get; set; }

        // In grams
        [XmlElement("saturated-fat")]
        public NutritionInformationMeasurement SaturatedFats { get; set; }

        [XmlElement("monounsaturated-fat")]
        public NutritionInformationMeasurement MonounsaturatedFats { get; set; }

        [XmlElement("polyunsaturated-fat")]
        public NutritionInformationMeasurement PolyunsaturatedFats { get; set; }

        [XmlElement("trans-fat")]
        public NutritionInformationMeasurement TransFats { get; set; }

        // In milligrams
        [XmlElement("cholesterol")]
        public NutritionInformationMeasurement Cholesterol { get; set; }

        [XmlElement("caffeine")]
        public NutritionInformationMeasurement Caffeine { get; set; }

        [XmlArray("servings")]
        public NutritionInformationServing[] Servings { get; set; }

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

    [XmlType("serving")]
    public sealed class NutritionInformationServing : IHealthVaultTypeSerializable
    {
        [XmlElement("desc")]
        public string Description { get; set; }

        [XmlElement("amount")]
        public double Amount { get; set; }

        [XmlElement("weight")]
        public NutritionInformationMeasurement Weight { get; set; }

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

    public sealed class NutritionInformationMeasurement : IHealthVaultTypeSerializable
    {
        [XmlAttribute("value")]
        public double Value { get; set; }

        [XmlAttribute("units-of-measure")]
        public string Units { get; set; }

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