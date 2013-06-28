// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Types;

namespace HealthVault.ItemTypes
{
    public interface IItemDataTyped : IHealthVaultTypeSerializable
    {
        [XmlIgnore]
        ItemType Type { get; }

        [XmlIgnore]
        ItemKey Key { get; set; }

        /// <summary>
        /// The record item this typed data is in.
        /// </summary>
        [XmlIgnore]
        RecordItem Item { get; }

        /// <summary>
        /// The data container this typed data is in.
        /// </summary>
        [XmlIgnore]
        ItemData ItemData { get; set; }

        //
        // For local edits, we have to update the item's effective date
        //
        DateTime GetDateForEffectiveDate();
    }
}