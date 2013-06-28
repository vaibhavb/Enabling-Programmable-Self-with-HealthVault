// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Height : IItemDataTyped
    {
        internal const string RootElement = "height";
        internal const string TypeIDString = "40750a6a-89b2-455c-bd8d-b420a4cb500b";

        private ItemProxy m_itemProxy;

        public Height()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public Height(StructuredDateTime when)
            : this(when, null)
        {
        }

        public Height(StructuredDateTime when, LengthMeasurement length)
            : this()
        {
            if (when == null)
            {
                throw new ArgumentNullException("when");
            }
            if (length == null)
            {
                throw new ArgumentNullException("length");
            }
            When = when;
            Value = length;
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("when")]
        public StructuredDateTime When { get; set; }

        [XmlElement("value")]
        public LengthMeasurement Value { get; set; }

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

        public static Height Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Height>(xml);
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