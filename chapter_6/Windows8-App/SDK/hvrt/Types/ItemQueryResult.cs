// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.ItemTypes;

namespace HealthVault.Types
{
    public sealed class ItemQueryResult : IHealthVaultTypeSerializable
    {
        public ItemQueryResult()
        {
            Name = String.Empty;
        }

        [XmlAttribute("name")]
        public string Name { get; set; }

        [XmlElement("thing", Order = 1)]
        public RecordItem[] Items { get; set; }

        [XmlElement("unprocessed-thing-key-info", Order = 2)]
        public PendingItem[] PendingItems { get; set; }

        [XmlIgnore]
        public bool HasItems
        {
            get { return !Items.IsNullOrEmpty(); }
        }

        [XmlIgnore]
        public bool HasPending
        {
            get { return !PendingItems.IsNullOrEmpty(); }
        }

        [XmlIgnore]
        public IEnumerable<ItemKey> ItemKeys
        {
            get
            {
                if (!HasItems)
                {
                    return null;
                }

                return (
                    from item in Items
                    select item.Key
                    );
            }
        }

        [XmlIgnore]
        public IEnumerable<ItemKey> PendingKeys
        {
            get
            {
                if (!HasPending)
                {
                    return null;
                }

                return (
                    from pending in PendingItems
                    select pending.Key
                    );
            }
        }

        [XmlIgnore]
        public IEnumerable<ItemKey> AllKeys
        {
            get
            {
                if (HasItems)
                {
                    for (int i = 0; i < Items.Length; ++i)
                    {
                        yield return Items[i].Key;
                    }
                }
                if (HasPending)
                {
                    for (int i = 0; i < PendingItems.Length; ++i)
                    {
                        yield return PendingItems[i].Key;
                    }
                }
            }
        }

        [XmlIgnore]
        public IEnumerable<IItemDataTyped> TypedData
        {
            get
            {
                if (HasItems)
                {
                    foreach (RecordItem item in Items)
                    {
                        if (item.HasTypedData)
                        {
                            yield return item.TypedData;
                        }
                    }
                }
            }
        }

        [XmlIgnore]
        internal int KeyCount
        {
            get
            {
                int count = 0;
                if (HasItems)
                {
                    count += Items.Length;
                }
                if (HasPending)
                {
                    count += PendingItems.Length;
                }

                return count;
            }
        }

        [XmlIgnore]
        public RecordItem FirstItem
        {
            get { return HasItems ? Items[0] : null; }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Items.ValidateOptional("Items");
            PendingItems.ValidateOptional("PendingItems");
        }

        #endregion

        public ItemDataTypedList ToItemDataTypedList(IRecord record, ItemQuery originalQuery)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (originalQuery == null)
            {
                throw new ArgumentNullException("originalQuery");
            }

            return new ItemDataTypedList(record, originalQuery.View, Items, PendingKeys);
        }

        public static ItemQueryResult Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<ItemQueryResult>(xml);
        }

        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }
    }
}