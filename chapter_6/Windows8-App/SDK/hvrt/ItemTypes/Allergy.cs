// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Allergy : IItemDataTyped
    {
        internal const string TypeIDString = "52bf9104-2c5e-4f1f-a66d-552ebcc53df7";
        internal const string RootElement = "allergy";
        private ItemProxy m_itemProxy;

        public Allergy()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public Allergy(string name)
            : this()
        {
            Name = new CodableValue(name);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("name", Order = 1)]
        public CodableValue Name { get; set; }

        [XmlElement("reaction", Order = 2)]
        public CodableValue Reaction { get; set; }

        [XmlElement("first-observed", Order = 3)]
        public ApproxDateTime FirstObserved { get; set; }

        [XmlElement("allergen-type", Order = 4)]
        public CodableValue AllergenType { get; set; }

        [XmlElement("allergen-code", Order = 5)]
        public CodableValue AllergenCode { get; set; }

        [XmlElement("treatment-provider", Order = 6)]
        public Person TreatmentProvider { get; set; }

        [XmlElement("treatment", Order = 7)]
        public CodableValue Treatment { get; set; }

        [XmlElement("is-negated", Order = 8)]
        public BooleanValue IsNegated { get; set; }

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
            Reaction.ValidateOptional("Reaction");
            FirstObserved.ValidateOptional("FirstObserved");
            AllergenType.ValidateOptional("AllergenType");
            AllergenCode.ValidateOptional("AllergenCode");
            TreatmentProvider.ValidateOptional("TreatmentProvider");
            Treatment.ValidateOptional("Treatment");
            IsNegated.ValidateOptional("IsNegated");
        }
        
        public DateTime GetDateForEffectiveDate()
        {
            return (this.FirstObserved != null) ? this.FirstObserved.ToDateTime() : null;
        }

        #endregion

        public static Allergy Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Allergy>(xml);
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
            return (Name != null) ? Name.ToString() : string.Empty;
        }

        //-----------------------------------------
        //
        // Vocabularies
        //
        //-----------------------------------------
        public static VocabIdentifier VocabForName()
        {
            return new VocabIdentifier(VocabFamily.Snomed, VocabName.SnomedAllergiesFiltered);
        }

        public static VocabIdentifier VocabForAllergenType()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.AllergenType);
        }

        public static VocabIdentifier VocabForReaction()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.Reactions);
        }
    }
}