// (c) Microsoft. All rights reserved

using System.Collections.Generic;
using System.Linq;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class PendingItem : IHealthVaultTypeSerializable
    {
        [XmlElement("thing-id", Order = 1)]
        public ItemKey Key { get; set; }

        [XmlElement("type-id", Order = 2)]
        public ItemType TypeID { get; set; }

        [XmlElement("eff-date", Order = 3)]
        public DateTime EffectiveDate { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Key.ValidateRequired("ID");
            TypeID.ValidateRequired("TypeID");
        }

        #endregion

        public static IEnumerable<ItemKey> Keys(IEnumerable<PendingItem> pendingItems)
        {
            if (pendingItems == null)
            {
                return null;
            }

            return (
                from pending in pendingItems
                select pending.Key
                );
        }
    }
}