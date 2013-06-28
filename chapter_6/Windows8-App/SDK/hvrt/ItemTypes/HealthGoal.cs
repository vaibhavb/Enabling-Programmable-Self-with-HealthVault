// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class HealthGoal : IItemDataTyped
    {
        internal const string TypeIDString = "dad8bb47-9ad0-4f09-a020-0ff051d1d0f7";
        internal const string RootElement = "health-goal";

        private ItemProxy m_itemProxy;

        public HealthGoal()
        {
            Description = String.Empty;
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("name", Order = 1)]
        public CodableValue Name { get; set; }

        [XmlElement("description", Order = 2)]
        public string Description { get; set; }

        [XmlElement("start-date", Order = 3)]
        public ApproxDateTime StartDate { get; set; }

        [XmlElement("end-date", Order = 4)]
        public ApproxDateTime EndDate { get; set; }

        [XmlElement("associated-type-info", Order = 5)]
        public GoalAssociatedTypeInfo AssociatedTypeInfo { get; set; }

        [XmlElement("target-range", Order = 6)]
        public GoalRange TargetRange { get; set; }

        [XmlElement("goal-additional-ranges", Order = 7)]
        public GoalRange[] GoalAdditionalRanges { get; set; }

        [XmlElement("recurrence", Order = 8)]
        public GoalRecurrence Recurrence { get; set; }

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
            Name.ValidateRequired("Name");
            StartDate.ValidateOptional("StartDate");
            EndDate.ValidateOptional("EndDate");
            AssociatedTypeInfo.ValidateOptional("AssociatedTypeInfo");
            TargetRange.ValidateOptional("TargetRange");
            GoalAdditionalRanges.ValidateOptional("GoalAdditionalRanges");
            Recurrence.ValidateOptional("Recurrence");
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

        public static HealthGoal Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<HealthGoal>(xml);
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

        public bool ShouldSerializeDescription()
        {
            return !String.IsNullOrEmpty(Description);
        }

        public static VocabIdentifier VocabForWeightUnits()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.WeightUnits);
        }

        public static VocabIdentifier VocabForLabResultsUnits()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.LabResultsUnits);
        }

        public static VocabIdentifier VocabForName()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.GoalType);
        }

        public static VocabIdentifier VocabForRecurrence()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.RecurrenceIntervals);
        }
    }
}
