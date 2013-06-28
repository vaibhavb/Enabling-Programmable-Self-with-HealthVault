// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Weight : IItemDataTyped
    {
        internal const string RootElement = "weight";
        internal const string TypeIDString = "3d34d87e-7fc1-4153-800f-f56592cb0d17";

        private ItemProxy m_itemProxy;

        public Weight()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public Weight(StructuredDateTime when)
            : this(when, null)
        {
        }

        public Weight(StructuredDateTime when, WeightMeasurement weight)
            : this()
        {
            if (when == null)
            {
                throw new ArgumentNullException("when");
            }
            if (weight == null)
            {
                throw new ArgumentNullException("weight");
            }

            When = when;
            Value = weight;
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("when")]
        public StructuredDateTime When { get; set; }

        [XmlElement("value")]
        public WeightMeasurement Value { get; set; }

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

        public void Validate()
        {
            When.ValidateRequired("When");
            Value.ValidateRequired("Value");
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

        public static Weight Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Weight>(xml);
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
            return (Value != null) ? Value.ToString() : string.Empty;
        }
    }
}