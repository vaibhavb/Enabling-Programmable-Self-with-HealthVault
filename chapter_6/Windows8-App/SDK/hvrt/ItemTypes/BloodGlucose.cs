// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class BloodGlucose : IItemDataTyped
    {
        internal const string TypeIDString = "879e7c04-4e8a-4707-9ad3-b054df467ce4";
        internal const string RootElement = "blood-glucose";
        private ItemProxy m_itemProxy;

        public BloodGlucose()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("when", Order = 1)]
        public StructuredDateTime When { get; set; }

        [XmlElement("value", Order = 2)]
        public BloodGlucoseMeasurement Value { get; set; }

        [XmlElement("glucose-measurement-type", Order = 3)]
        public CodableValue MeasurementType { get; set; }

        [XmlElement("outside-operating-temp", Order = 4)]
        public BooleanValue OutsideOperatingTemperature { get; set; }

        [XmlElement("is-control-test", Order = 5)]
        public BooleanValue IsControlTest { get; set; }

        [XmlElement("normalcy", Order = 6)]
        public OneToFive Normalcy { get; set; }

        [XmlElement("measurement-context", Order = 7)]
        public CodableValue MeasurementContext { get; set; }

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
            When.ValidateRequired("When");
            Value.ValidateRequired("Value");
            MeasurementType.ValidateOptional("MeasurementType");
            OutsideOperatingTemperature.ValidateOptional("OutsideOperatingTemperature");
            IsControlTest.ValidateOptional("IsControlTest");
            Normalcy.ValidateOptional("Normalcy");
            MeasurementContext.ValidateOptional("MeasurementContext");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public HealthVault.Types.DateTime GetDateForEffectiveDate()
        {
            return (this.When != null) ? this.When.ToDateTime() : null;
        }

        #endregion

        public static BloodGlucose Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<BloodGlucose>(xml);
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
        public static VocabIdentifier VocabForMeasurementType()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.GlucoseMeasurementType);
        }

        public static VocabIdentifier VocabForMeasurementContext()
        {
            return new VocabIdentifier(VocabFamily.HealthVault, VocabName.GlucoseMeasurementContext);
        }
    }
}