// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class BloodPressure : IItemDataTyped
    {
        internal const string TypeIDString = "ca3c57f4-f4c1-4e15-be67-0a3caf5414ed";
        internal const string RootElement = "blood-pressure";
        private ItemProxy m_itemProxy;

        public BloodPressure()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public BloodPressure(StructuredDateTime when, NonNegativeInt systolic, NonNegativeInt diastolic)
            : this()
        {
            if (when == null)
            {
                throw new ArgumentNullException("when");
            }

            if (systolic == null)
            {
                throw new ArgumentNullException("systolic");
            }

            if (diastolic == null)
            {
                throw new ArgumentNullException("diastolic");
            }

            When = when;
            Systolic = systolic;
            Diastolic = diastolic;
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("when", Order = 1)]
        public StructuredDateTime When { get; set; }

        [XmlElement("systolic", Order = 2)]
        public NonNegativeInt Systolic { get; set; }

        [XmlElement("diastolic", Order = 3)]
        public NonNegativeInt Diastolic { get; set; }

        [XmlElement("pulse", Order = 4)]
        public NonNegativeInt Pulse { get; set; }

        [XmlElement("irregular-heartbeat", Order = 5)]
        public BooleanValue IrregularHeartbeat { get; set; }

        [XmlIgnore]
        public int SystolicValue
        {
            get { return Systolic != null ? Systolic.Value : -1; }
            set { Systolic = new NonNegativeInt(value); }
        }

        [XmlIgnore]
        public int DiastolicValue
        {
            get { return Diastolic != null ? Diastolic.Value : -1; }
            set { Diastolic = new NonNegativeInt(value); }
        }

        [XmlIgnore]
        public int PulseValue
        {
            get { return Pulse != null ? Pulse.Value : -1; }
            set { Pulse = new NonNegativeInt(value); }
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
            When.ValidateRequired("When");
            Systolic.ValidateRequired("Systolic");
            Diastolic.ValidateRequired("Diastolic");
            Pulse.ValidateOptional("Pulse");
            IrregularHeartbeat.ValidateOptional("IrregularHeartbeat");
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

        public static BloodPressure Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<BloodPressure>(xml);
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
            return String.Format("{0}/{1}", Systolic, Diastolic);
        }
    }
}