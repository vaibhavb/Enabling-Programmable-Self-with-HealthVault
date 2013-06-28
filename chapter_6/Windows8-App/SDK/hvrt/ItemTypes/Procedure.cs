// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Procedure : IItemDataTyped
    {
        internal const string TypeIDString = "df4db479-a1ba-42a2-8714-2b083b88150f";
        internal const string RootElement = "procedure";
        private ItemProxy m_itemProxy;

        public Procedure()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public Procedure(string name)
            : this()
        {
            Name = new CodableValue(name);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("when", Order = 1)]
        public ApproxDateTime When { get; set; }

        [XmlElement("name", Order = 2)]
        public CodableValue Name { get; set; }

        [XmlElement("anatomic-location", Order = 3)]
        public CodableValue AnatomicLocation { get; set; }

        [XmlElement("primary-provider", Order = 4)]
        public Person PrimaryProvider { get; set; }

        [XmlElement("secondary-provider", Order = 5)]
        public Person SecondaryProvider { get; set; }

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
            When.ValidateOptional("When");
            AnatomicLocation.ValidateOptional("AnatomicLocation");
            PrimaryProvider.ValidateOptional("PrimaryProvider");
            SecondaryProvider.ValidateOptional("SecondaryProvider");
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return (this.When != null) ? this.When.ToDateTime() : null;
        }

        #endregion

        public static Procedure Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Procedure>(xml);
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

        public static VocabIdentifier VocabForName()
        {
            return new VocabIdentifier(VocabFamily.Snomed, VocabName.SnomedProceduresFiltered);
        }

        public static VocabIdentifier VocabForAnatomicLocation()
        {
            return new VocabIdentifier(VocabFamily.Snomed, VocabName.SnomedBodyLocationFiltered);
        }
    }
}