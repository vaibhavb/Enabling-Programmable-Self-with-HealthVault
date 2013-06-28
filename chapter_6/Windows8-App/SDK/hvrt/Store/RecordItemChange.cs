using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HealthVault.Types;
using HealthVault.ItemTypes;
using HealthVault.Foundation;

namespace HealthVault.Store
{
    /// <summary>
    /// The Type of Change made to the RecordItem
    /// </summary>
    public enum RecordItemChangeType
    {
        Put,
        Remove
    }

    /// <summary>
    /// Used to track pending changes made to an item. These changes will be comitted to a remote store (cloud HealthVault)
    /// </summary>
    [XmlType("itemChange")]
    public sealed class RecordItemChange : IHealthVaultTypeSerializable
    {
        public RecordItemChange() 
        {
        }

        internal RecordItemChange(string typeID, ItemKey key, RecordItemChangeType changeType)
        {
            this.TypeID = typeID;
            this.InitChange(key, changeType);
        }
        
        /// <summary>
        /// Each change has a unique ID. We use this for duplicate detection
        /// </summary>
        [XmlElement("changeID")]
        public string ChangeID
        {
            get; set;
        }

        /// <summary>
        /// When the change was made. Used to maintain an approximate ordering of changes
        /// </summary>
        [XmlElement("timestamp")]
        public long Timestamp
        {
            get; set;
        }

        /// <summary>
        /// The type of change...
        /// </summary>
        [XmlElement("type")]
        public RecordItemChangeType ChangeType
        {
            get; set;
        }

        /// <summary>
        /// TypeID of the item that was changed
        /// </summary>
        [XmlElement("typeID")]
        public string TypeID
        {
            get; set;
        }

        /// <summary>
        /// The ItemKey of the item that was changed
        /// </summary>
        [XmlElement("key")]
        public ItemKey Key
        {
            get; set;
        }

        /// <summary>
        /// Tracks how many attempts were made to commit this change to the remote store
        /// </summary>
        [XmlElement("attempt")]
        public int Attempt
        {
            get; set;
        }

        [XmlIgnore]
        public string ItemID
        {
            get { return (this.Key != null) ? this.Key.ID : null; }
        }
        
        /// <summary>
        /// A commit may produce an updated key. Local caches must be updated to use this Key
        /// </summary>
        [XmlIgnore]
        public ItemKey UpdatedKey
        {
            get; set;
        }
        
        /// <summary>
        /// The commit may produce an updated item. 
        /// </summary>
        [XmlIgnore]
        public RecordItem UpdatedItem
        {
            get; set;
        }

        [XmlIgnore]
        public bool HasUpdatedItem
        {
            get { return (this.UpdatedItem != null);}
        }

        /// <summary>
        /// The data that is being comitted to the remote store
        /// </summary>
        [XmlIgnore]
        internal IItemDataTyped LocalData
        {
            get; set;
        }
        
        [XmlIgnore]
        internal bool HasLocalData
        {
            get { return (this.LocalData != null);}
        }

        public void SetChangeID()
        {
            this.ChangeID = "Win8_" + Guid.NewGuid().ToString("D");
        }

        public void SetTimestamp()
        {
            this.Timestamp = DateTimeOffset.Now.Ticks;
        }

        public bool IsChangeForType(string typeID)
        {
            return string.Equals(this.TypeID, typeID);
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public static RecordItemChange Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<RecordItemChange>(xml);
        }

        public void Validate()
        {
            this.TypeID.ValidateRequired("TypeID");
            this.ItemID.ValidateRequired("ItemID");
            this.ChangeID.ValidateRequired("ChangeID");
        }

        public static int Compare(RecordItemChange x, RecordItemChange y)
        {
            int cmp = x.Timestamp.CompareTo(y.Timestamp); 
            if (cmp == 0)
            {
                cmp = x.ItemID.CompareTo(y.ItemID);
            }
            return cmp;
        }

        public override string ToString()
        {
            return string.Format("{0}, TypeID={1}, ItemKey={2}", this.ChangeType, this.TypeID, this.Key.ToString());
        }

        public static RecordItemChange UpdateChange(string typeID, ItemKey key, RecordItemChangeType changeType, RecordItemChange existingChange)
        {
            if (existingChange == null)
            {
                return new RecordItemChange(typeID, key, changeType);
            }

            if (existingChange.ChangeType == RecordItemChangeType.Remove && changeType == RecordItemChangeType.Put)
            {
                // Can't put an item already marked for deletion
                throw new StoreException(StoreErrorNumber.ItemAlreadyDeleted);
            }

            if (existingChange.TypeID != typeID)
            {
                // Defensive code. Should never happen
                throw new StoreException(StoreErrorNumber.TypeIDMismatch);
            }

            existingChange.InitChange(key, changeType);

            return existingChange;
        }

        void InitChange(ItemKey key, RecordItemChangeType changeType)
        {
            this.Key = key;
            this.ChangeType = changeType;
            this.SetChangeID();
            this.SetTimestamp();
        }
    }
}
