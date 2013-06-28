// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.ItemTypes;
using HealthVault.Types;
using DateTime = HealthVault.Types.DateTime;

namespace HealthVault.Store
{
    public sealed class ViewKey : IHealthVaultTypeSerializable
    {
        public ViewKey()
        {
        }

        public ViewKey(ItemKey key, DateTime effectiveDate)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            if (effectiveDate == null)
            {
                throw new ArgumentNullException("effectiveDate");
            }

            Key = key;
            EffectiveDate = effectiveDate;
        }

        [XmlElement("thing-id", Order = 1)]
        public ItemKey Key { get; set; }

        [XmlElement("eff-date", Order = 2)]
        public DateTime EffectiveDate { get; set; }

        [XmlIgnore]
        public string ID
        {
            get { return (Key != null) ? Key.ID : String.Empty; }
        }

        internal bool IsLoadPending { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Key.ValidateRequired("Key");
            EffectiveDate.ValidateRequired("EffectiveDate");
        }

        #endregion

        public int CompareTo(ViewKey other)
        {
            if (other == null)
            {
                return 1;
            }

            //
            // Sorted by descending EffectiveDate, then itemID (ascending)
            //
            int cmp = -DateTime.Compare(EffectiveDate, other.EffectiveDate);
            if (cmp == 0)
            {
                cmp = ItemKey.Compare(Key, other.Key);
            }

            return cmp;
        }

        //
        // WINRT - you cannot have overloads with the SAME # OF PARAMETERS
        // We therefore use factory methods. 
        //
        public static ViewKey FromPendingItem(PendingItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return new ViewKey(item.Key, item.EffectiveDate);
        }

        public static ViewKey FromItem(RecordItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return new ViewKey(item.Key, item.EnsureEffectiveDate());
        }

        public static ViewKey FromTypedData(IItemDataTyped item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            return FromItem(item.Item);
        }
    }

    internal class ViewKeyComparer : Comparer<ViewKey>
    {
        public override int Compare(ViewKey x, ViewKey y)
        {
            return x.CompareTo(y);
        }
    }
}