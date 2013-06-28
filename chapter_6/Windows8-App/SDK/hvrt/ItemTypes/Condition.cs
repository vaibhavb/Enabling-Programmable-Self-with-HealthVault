// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Condition : IItemDataTyped
    {
        internal const string TypeIDString = "7ea7a1f9-880b-4bd4-b593-f5660f20eda8";
        internal const string RootElement = "condition";
        private ItemProxy m_itemProxy;

        public Condition()
        {
            StopReason = String.Empty;
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public Condition(string name)
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

        [XmlElement("onset-date", Order = 2)]
        public ApproxDateTime OnsetDate { get; set; }

        [XmlElement("status", Order = 3)]
        public CodableValue Status { get; set; }

        [XmlElement("stop-date", Order = 4)]
        public ApproxDateTime StopDate { get; set; }

        [XmlElement("stop-reason", Order = 5)]
        public string StopReason { get; set; }

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
            OnsetDate.ValidateOptional("OnsetDate");
            Status.ValidateOptional("Status");
            StopDate.ValidateOptional("StopDate");
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return null;
        }

        #endregion

        public static Condition Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Condition>(xml);
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

        public bool ShouldSerializeStopReason()
        {
            return !String.IsNullOrEmpty(StopReason);
        }

        //-----------------------------------------
        //
        // Vocabularies
        //
        //-----------------------------------------
        public static VocabIdentifier VocabForName()
        {
            return new VocabIdentifier(VocabFamily.Snomed, VocabName.SnomedConditionsFiltered);
        }

        public static VocabIdentifier VocabForStatus()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.ConditionOccurrence);
        }
    }
}