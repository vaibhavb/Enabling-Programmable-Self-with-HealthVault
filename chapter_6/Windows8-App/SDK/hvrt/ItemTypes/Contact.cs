// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Contact : IItemDataTyped
    {
        internal const string RootElement = "person";
        internal const string TypeIdString = "25c94a9f-9d3d-4576-96dc-6791178a8143";

        private ItemProxy m_itemProxy;
        private Person m_personProxy;

        public Contact()
            : this(new Person())
        {
        }

        public Contact(Person person)
        {
            m_personProxy = person;
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIdString; }
        }

        [XmlIgnore]
        public Person Person
        {
            get { return m_personProxy; }
            set { m_personProxy = value; }
        }

        [XmlElement("name", Order = 1)]
        public Name Name
        {
            get { return m_personProxy.Name; }
            set { m_personProxy.Name = value; }
        }

        [XmlElement("organization", Order = 2)]
        public string Organization
        {
            get { return m_personProxy.Organization; }
            set { m_personProxy.Organization = value; }
        }

        [XmlElement("professional-training", Order = 3)]
        public string Training
        {
            get { return m_personProxy.Training; }
            set { m_personProxy.Training = value; }
        }

        [XmlElement("id", Order = 4)]
        public string IdentificationNumber
        {
            get { return m_personProxy.IdentificationNumber; }
            set { m_personProxy.IdentificationNumber = value; }
        }

        [XmlElement("contact", Order = 5)]
        public Types.Contact ContactInformation
        {
            get { return m_personProxy.Contact; }
            set { m_personProxy.Contact = value; }
        }

        [XmlElement("type", Order = 6)]
        public CodableValue ContactType
        {
            get { return m_personProxy.Type; }
            set { m_personProxy.Type = value; }
        }

        #region IItemDataTyped

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

        #endregion

        #region IItemDataTyped Members

        public void Validate()
        {
            m_personProxy.Validate();
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return null;
        }

        #endregion

        public static Contact Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Contact>(xml);
        }

        public static ItemQuery QueryFor()
        {
            return ItemQuery.QueryForTypeID(TypeID);
        }

        public static ItemFilter FilterFor()
        {
            return ItemFilter.FilterForType(TypeID);
        }

        public static VocabIdentifier VocabForContactType()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.PersonTypes);
        }
    }
}