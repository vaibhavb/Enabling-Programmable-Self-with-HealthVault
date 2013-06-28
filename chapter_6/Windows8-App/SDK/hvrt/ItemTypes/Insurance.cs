// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Insurance : IItemDataTyped
    {
        internal const string RootElement = "payer";
        internal const string TypeIDString = "9366440c-ec81-4b89-b231-308a4c4d70ed";

        private ItemProxy m_itemProxy;

        public Insurance()
        {
            PlanName = String.Empty;
            CarrierId = String.Empty;
            GroupNumber = String.Empty;
            PlanCode = String.Empty;
            SubscriberId = String.Empty;
            PersonCode = String.Empty;
            SubscriberName = String.Empty;
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("plan-name", Order = 1)]
        public String PlanName { get; set; }

        [XmlElement("coverage-type", Order = 2)]
        public CodableValue CoverageType { get; set; }

        [XmlElement("carrier-id", Order = 3)]
        public String CarrierId { get; set; }

        [XmlElement("group-num", Order = 4)]
        public String GroupNumber { get; set; }

        [XmlElement("plan-code", Order = 5)]
        public String PlanCode { get; set; }

        [XmlElement("subscriber-id", Order = 6)]
        public String SubscriberId { get; set; }

        [XmlElement("person-code", Order = 7)]
        public String PersonCode { get; set; }

        [XmlElement("subscriber-name", Order = 8)]
        public String SubscriberName { get; set; }

        [XmlElement("subscriber-dob", Order = 9)]
        public StructuredDateTime SubscriberDob { get; set; }

        [XmlElement("is-primary", Order = 10)]
        public BooleanValue IsPrimary { get; set; }

        [XmlElement("expiration-date", Order = 11)]
        public StructuredDateTime ExpirationDate { get; set; }

        [XmlElement("contact", Order = 12)]
        public Types.Contact Contact { get; set; }

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

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            PlanName.ValidateRequired("PlanName");
            CoverageType.ValidateOptional("CoverageType");
            SubscriberDob.ValidateOptional("SubscriberDob");
            IsPrimary.ValidateOptional("IsPrimary");
            ExpirationDate.ValidateOptional("ExpirationDate");
            Contact.ValidateOptional("Contact");
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return null;
        }

        #endregion

        public static Insurance Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Insurance>(xml);
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
            return PlanName ?? string.Empty;
        }

        public bool ShouldSerializePlanName()
        {
            return !String.IsNullOrEmpty(PlanName);
        }

        public bool ShouldSerializeCarrierId()
        {
            return !String.IsNullOrEmpty(CarrierId);
        }

        public bool ShouldSerializeGroupNumber()
        {
            return !String.IsNullOrEmpty(GroupNumber);
        }

        public bool ShouldSerializePlanCode()
        {
            return !String.IsNullOrEmpty(PlanCode);
        }

        public bool ShouldSerializeSubscriberId()
        {
            return !String.IsNullOrEmpty(SubscriberId);
        }

        public bool ShouldSerializePersonCode()
        {
            return !String.IsNullOrEmpty(PersonCode);
        }

        public bool ShouldSerializeSubscriberName()
        {
            return !String.IsNullOrEmpty(SubscriberName);
        }

        //-----------------------------------------
        //
        // Vocabularies
        //
        //-----------------------------------------
        public static VocabIdentifier VocabForCoverageType()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.CoverageTypes);
        }
    }
}