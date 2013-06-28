// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    public sealed class DietaryIntake : IItemDataTyped
    {
        internal const string TypeIDString = "089646a6-7e25-4495-ad15-3e28d4c1a71d";
        internal const string RootElement = "dietary-intake";
        private ItemProxy m_itemProxy;

        public DietaryIntake()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("food-item", Order = 1)]
        public CodableValue FoodItem { get; set; }

        [XmlElement("serving-size", Order = 2)]
        public CodableValue ServingSize { get; set; }

        [XmlElement("servings-consumed", Order = 3)]
        public NonNegativeDouble ServingsConsumed { get; set; }

        [XmlElement("meal", Order = 4)]
        public CodableValue Meal { get; set; }

        [XmlElement("when", Order = 5)]
        public StructuredDateTime When { get; set; }

        [XmlElement("energy", Order = 6)]
        public FoodEnergyValue Calories { get; set; }

        [XmlElement("energy-from-fat", Order = 7)]
        public FoodEnergyValue CaloriesFromFat { get; set; }

        [XmlElement("total-fat", Order = 8)]
        public WeightMeasurement TotalFat { get; set; }

        [XmlElement("saturated-fat", Order = 9)]
        public WeightMeasurement SaturatedFat { get; set; }

        [XmlElement("trans-fat", Order = 10)]
        public WeightMeasurement TransFat { get; set; }

        [XmlElement("monounsaturated-fat", Order = 11)]
        public WeightMeasurement MonounsaturatedFat { get; set; }

        [XmlElement("polyunsaturated-fat", Order = 12)]
        public WeightMeasurement PolyunsaturatedFat { get; set; }

        [XmlElement("protein", Order = 13)]
        public WeightMeasurement Protein { get; set; }

        [XmlElement("carbohydrates", Order = 14)]
        public WeightMeasurement Carbohydrates { get; set; }

        [XmlElement("dietary-fiber", Order = 15)]
        public WeightMeasurement Fiber { get; set; }

        [XmlElement("sugars", Order = 16)]
        public WeightMeasurement Sugars { get; set; }

        [XmlElement("sodium", Order = 17)]
        public WeightMeasurement Sodium { get; set; }

        [XmlElement("cholesterol", Order = 18)]
        public WeightMeasurement Cholesterol { get; set; }

        [XmlElement("calcium", Order = 19)]
        public WeightMeasurement Calcium { get; set; }

        [XmlElement("iron", Order = 20)]
        public WeightMeasurement Iron { get; set; }

        [XmlElement("magnesium", Order = 21)]
        public WeightMeasurement Magnesium { get; set; }

        [XmlElement("phosphorus", Order = 22)]
        public WeightMeasurement Phosphorus { get; set; }

        [XmlElement("potassium", Order = 23)]
        public WeightMeasurement Potassium { get; set; }

        [XmlElement("zinc", Order = 24)]
        public WeightMeasurement Zinc { get; set; }

        [XmlElement("vitamin-A-RAE", Order = 25)]
        public WeightMeasurement VitaminA { get; set; }

        [XmlElement("vitamin-E", Order = 26)]
        public WeightMeasurement VitaminE { get; set; }

        [XmlElement("vitamin-D", Order = 27)]
        public WeightMeasurement VitaminD { get; set; }

        [XmlElement("vitamin-C", Order = 28)]
        public WeightMeasurement VitaminC { get; set; }

        [XmlElement("thiamin", Order = 29)]
        public WeightMeasurement Thiamin { get; set; }

        [XmlElement("riboflavin", Order = 30)]
        public WeightMeasurement Riboflavin { get; set; }

        [XmlElement("niacin", Order = 31)]
        public WeightMeasurement Niacin { get; set; }

        [XmlElement("vitamin-B-6", Order = 32)]
        public WeightMeasurement VitaminB6 { get; set; }

        [XmlElement("folate-DFE", Order = 33)]
        public WeightMeasurement Folate { get; set; }

        [XmlElement("vitamin-B-12", Order = 34)]
        public WeightMeasurement VitaminB12 { get; set; }

        [XmlElement("vitamin-K", Order = 35)]
        public WeightMeasurement VitaminK { get; set; }

        [XmlArray("additional-nutrition-facts", Order = 36)]
        [XmlArrayItem("nutrition-fact")]
        public NutritionFact[] AdditionalNutritionFacts { get; set; }

        #region IItemDataTyped Members

        [XmlIgnore]
        public ItemType Type
        {
            get { return m_itemProxy.Item.Type; }
        }

        [XmlIgnore]
        public ItemKey Key
        {
            get { return m_itemProxy.Item.Key; }
            set { m_itemProxy.Item.Key = value; }
        }

        [XmlIgnore]
        public RecordItem Item
        {
            get { return m_itemProxy; }
        }

        [XmlIgnore]
        public ItemData ItemData
        {
            get { return m_itemProxy.ItemData; }
            set { m_itemProxy.ItemData = value; }
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            FoodItem.ValidateRequired("FoodItem");

            ServingSize.ValidateOptional("ServingSize");
            ServingsConsumed.ValidateOptional("ServingsConsumed");
            Meal.ValidateOptional("Meal");
            When.ValidateOptional("When");

            Calories.ValidateOptional("Calories");
            CaloriesFromFat.ValidateOptional("CaloriesFromFat");
            TotalFat.ValidateOptional("TotalFat");
            SaturatedFat.ValidateOptional("SaturatedFat");
            MonounsaturatedFat.ValidateOptional("MonounsaturatedFat");
            //
            // FINISH THIS!!
            // 
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return (this.When != null) ? this.When.ToDateTime() : null;
        }

        #endregion

        public static DietaryIntake Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<DietaryIntake>(xml);
        }

        public static ItemQuery QueryFor()
        {
            return ItemQuery.QueryForTypeID(TypeID);
        }

        public static ItemFilter FilterFor()
        {
            return ItemFilter.FilterForType(TypeID);
        }

        //-----------------------------------------
        //
        // Vocabularies
        //
        //-----------------------------------------
        public static VocabIdentifier VocabForFoodItem()
        {
            return new VocabIdentifier(VocabFamily.USDA, VocabName.FoodDescription);
        }

        public static VocabIdentifier VocabForMeal()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.DietaryIntakeMeals);
        }
    }
}