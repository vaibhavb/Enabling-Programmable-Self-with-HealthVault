// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ItemFilter : IHealthVaultTypeSerializable
    {
        private LazyField<StringCollection> m_typeIDs;

        public ItemFilter()
        {
            ItemState = String.Empty;
            CreatedByAppID = String.Empty;
            CreatedByPersonID = String.Empty;
            UpdatedByAppID = String.Empty;
            UpdatedByPersonID = String.Empty;
            XPath = String.Empty;
        }

        public ItemFilter(IEnumerable<string> typeIDs) : this()
        {
            if (typeIDs != null)
            {
                TypeIDs.AddRange(typeIDs);
            }
        }

        [XmlElement("type-id", Order = 1)]
        public StringCollection TypeIDs
        {
            get { return m_typeIDs.Value; }
            set { m_typeIDs.Value = value; }
        }

        [XmlElement("thing-state", Order = 2)]
        public string ItemState { get; set; }

        [XmlElement("eff-date-min", Order = 3)]
        public DateTime EffectiveDateMin { get; set; }

        [XmlElement("eff-date-max", Order = 4)]
        public DateTime EffectiveDateMax { get; set; }

        [XmlElement("created-app-id", Order = 5)]
        public string CreatedByAppID { get; set; }

        [XmlElement("created-person-id", Order = 6)]
        public string CreatedByPersonID { get; set; }

        [XmlElement("updated-app-id", Order = 7)]
        public string UpdatedByAppID { get; set; }

        [XmlElement("updated-person-id", Order = 8)]
        public string UpdatedByPersonID { get; set; }

        [XmlElement("created-date-min", Order = 9)]
        public DateTime CreatedDateMin { get; set; }

        [XmlElement("created-date-max", Order = 10)]
        public DateTime CreatedDateMax { get; set; }

        [XmlElement("updated-date-min", Order = 11)]
        public DateTime UpdatedDateMin { get; set; }

        [XmlElement("updated-date-max", Order = 12)]
        public DateTime UpdatedDateMax { get; set; }

        [XmlElement("xpath", Order = 13)]
        public string XPath { get; set; }

        [XmlElement("updated-end-date-max", Order = 14)]
        public DateTime UpdatedEndDateMax { get; set; }

        [XmlElement("updated-end-date-min", Order = 15)]
        public DateTime UpdatedEndDateMin { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            TypeIDs.ValidateRequired<string>("TypeIDs");
        }

        #endregion

        public static ItemFilter Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<ItemFilter>(xml);
        }

        //
        // Factory methods, since constructor overloading in WinRT is limited
        //
        public static ItemFilter FilterForType(string typeID)
        {
            if (string.IsNullOrEmpty(typeID))
            {
                throw new ArgumentException("typeID");
            }

            var filter = new ItemFilter();
            filter.TypeIDs.Add(typeID);

            return filter;
        }

        #region XmlSerializerHints
        
        public bool ShouldSerializeItemState()
        {
            return !String.IsNullOrEmpty(ItemState);
        }

        public bool ShouldSerializeCreatedByAppID()
        {
            return !String.IsNullOrEmpty(CreatedByAppID);
        }

        public bool ShouldSerializeCreatedByPersonID()
        {
            return !String.IsNullOrEmpty(CreatedByPersonID);
        }

        public bool ShouldSerializeUpdatedByAppID()
        {
            return !String.IsNullOrEmpty(UpdatedByAppID);
        }

        public bool ShouldSerializeUpdatedByPersonID()
        {
            return !String.IsNullOrEmpty(UpdatedByPersonID);
        }

        public bool ShouldSerializeXPath()
        {
            return !String.IsNullOrEmpty(XPath);
        }

        #endregion
    }
}