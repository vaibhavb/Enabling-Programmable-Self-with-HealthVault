// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    [XmlType(RootElement)]
    public sealed class Cholesterol : IItemDataTyped
    {
        internal const string TypeIDString = "98f76958-e34f-459b-a760-83c1699add38";
        internal const string RootElement = "cholesterol-profile";
        private ItemProxy m_itemProxy;

        public Cholesterol()
        {
            m_itemProxy = new ItemProxy(TypeID, this);
        }

        public static string TypeID
        {
            get { return TypeIDString; }
        }

        [XmlElement("when", Order = 1)]
        public StructuredDateTime When { get; set; }

        [XmlElement("ldl", Order = 2)]
        public ConcentrationValue Ldl { get; set; }

        [XmlElement("hdl", Order = 3)]
        public ConcentrationValue Hdl { get; set; }

        [XmlElement("total-cholesterol", Order = 4)]
        public ConcentrationValue Total { get; set; }

        [XmlElement("triglyceride", Order = 5)]
        public ConcentrationValue Triglycerides { get; set; }
        
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
            Ldl.ValidateOptional("Ldl");
            Hdl.ValidateOptional("Hdl");
            Total.ValidateOptional("Total");
            Triglycerides.ValidateOptional("Triglycerides");
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

        public static Cholesterol Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Cholesterol>(xml);
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
            return String.Format("LDL={0}, HDL={1}", Ldl, Hdl);
        }
    }
}