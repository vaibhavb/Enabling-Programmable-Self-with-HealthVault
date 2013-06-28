// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    [XmlType("group")]
    public sealed class ItemQuery : IHealthVaultTypeSerializable
    {
        private LazyField<ItemFilterCollection> m_filters;
        private LazyField<ItemView> m_view;

        public ItemQuery()
        {
            Name = String.Empty;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlIgnore]
        public NonNegativeInt MaxResults { get; set; }

        //
        // Workaround for WinRT + Serializer limitations
        // Can't serialize nullable objects as attributes
        //
        [XmlAttribute("max")]
        public string MaxResultsString
        {
            get { return NonNegativeInt.ToTextValue(MaxResults); }
            set { MaxResults = NonNegativeInt.FromText(value); }
        }

        [XmlIgnore]
        public NonNegativeInt MaxFullItems { get; set; }

        //
        // Workaround for WinRT limitations
        // Can't serialize nullable objects as attributes
        //
        [XmlAttribute("max-full")]
        public string MaxFullItemsString
        {
            get { return NonNegativeInt.ToTextValue(MaxFullItems); }
            set { MaxFullItems = NonNegativeInt.FromText(value); }
        }

        [XmlElement("id", Order = 1)]
        public string[] ItemIDs { get; set; }

        [XmlElement("key", Order = 2)]
        public ItemKey[] Keys { get; set; }

        [XmlElement("client-thing-id", Order = 3)]
        public string[] ClientIDs { get; set; }

        [XmlElement("filter", Order = 4)]
        public ItemFilterCollection Filters
        {
            get { return m_filters.Value; }
            set { m_filters.Value = value; }
        }

        [XmlElement("format", Order = 5)]
        public ItemView View
        {
            get { return m_view.Value; }
            set { m_view.Value = value; }
        }

        [XmlIgnore]
        public bool HasItemIDs
        {
            get { return !ItemIDs.IsNullOrEmpty(); }
        }

        [XmlIgnore]
        public bool HasFilters
        {
            get { return (m_filters.HasValue && !this.Filters.IsNullOrEmpty());}
        }
        
        [XmlIgnore]
        internal ItemFilter FirstFilter
        {
            get { return this.HasFilters ? this.Filters[0] : null;}
            set 
            {
                if (this.HasFilters)
                {
                    this.Filters[0] = value;
                }
                else
                {
                    this.Filters.Add(value);
                }
            }
        }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            View.ValidateRequired("View");

            ItemIDs.ValidateOptional("ItemIDs");
            Keys.ValidateOptional("Keys");
            Filters.ValidateOptional<ItemFilter>("Filter");
            ClientIDs.ValidateOptional("ClientIDs");
            MaxResults.ValidateOptional("MaxResults");
            MaxFullItems.ValidateOptional("MaxFullItems");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion

        public static ItemQuery Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<ItemQuery>(xml);
        }

        //------------------------------------------------------------------
        //
        // Factory methods
        // Constructor overloading in WinRT is limited
        //
        //------------------------------------------------------------------
        public static ItemQuery QueryForTypeID(string typeID)
        {
            var query = new ItemQuery();
            query.Name = typeID;
            query.Filters.Add(ItemFilter.FilterForType(typeID));
            query.View.TypeVersions.Add(typeID);

            return query;
        }

        public static ItemQuery QueryForKey(ItemKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            var query = new ItemQuery();
            query.Keys = new[] {key};

            return query;
        }

        public static ItemQuery QueryForKeys(IEnumerable<ItemKey> keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            var query = new ItemQuery();
            query.Keys = keys.ToArray();

            return query;
        }

        public static ItemQuery QueryForKeys(IList<ItemFilter> filters, int maxResults)
        {
            filters.ValidateRequired("filter");

            var query = new ItemQuery();
            query.View.SetSections(ItemSectionType.Core);
            query.Filters.AddRange(filters);
            query.MaxFullItems = new NonNegativeInt(0);
            if (maxResults > 0)
            {
                query.MaxResults = new NonNegativeInt(maxResults);
            }

            return query;
        }

        public static ItemQuery QueryForPending(IEnumerable<PendingItem> pendingItems)
        {
            if (pendingItems == null)
            {
                throw new ArgumentNullException("pendingItems");
            }

            return QueryForKeys(PendingItem.Keys(pendingItems));
        }

        #region XmlSerializerHints
        
        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }

        #endregion
    }
}