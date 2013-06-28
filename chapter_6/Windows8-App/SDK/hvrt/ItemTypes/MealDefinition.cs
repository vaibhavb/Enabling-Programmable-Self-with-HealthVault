// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class MealDefinition : IItemDataTyped
    {
        internal const string TypeIDString = "074e122a-335a-4a47-a63d-00a8f3e79e60";
        internal const string RootElement = "meal-definition";
        private ItemProxy m_itemProxy;

        public MealDefinition()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("name", Order=1)]
        public CodableValue Name { get; set; }

        [XmlElement("meal-type", Order=2)]
        public CodableValue MealType { get; set; }

        [XmlElement("description", Order=3)]
        public string Description { get; set; }

        [XmlArray("dietary-items", Order = 4)]
        [XmlArrayItem("dietary-item")]
        public DietaryIntakeItem[] DietaryIntakeItems { get; set; }

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
            Name.ValidateRequired("Name");

            MealType.ValidateOptional("MealType");
            DietaryIntakeItems.ValidateOptional("DietaryIntakeItems");
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return null;
        }
        #endregion

        public static MealDefinition Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<MealDefinition>(xml);
        }

        public static ItemQuery QueryFor()
        {
            return ItemQuery.QueryForTypeID(TypeID);
        }

        public static ItemFilter FilterFor()
        {
            return ItemFilter.FilterForType(TypeID);
        }

        public override string ToString()
        {
            return Name.ToString();
        }

        public static VocabIdentifier VocabForMealType()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.DietaryIntakeMeals);
        }
    }
}