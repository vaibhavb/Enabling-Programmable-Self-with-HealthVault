using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.Store
{
    [XmlType("recordItemChanges")]
    public sealed class RecordItemChangeList : IEnumerable<RecordItemChange>, IHealthVaultTypeSerializable
    {
        Dictionary<string, RecordItemChange> m_changes;  // Maps ItemID --> Change

        public RecordItemChangeList()
        {
            m_changes = new Dictionary<string,RecordItemChange>();
        }

        [XmlIgnore]
        public int Count
        {
            get { return m_changes.Count;}
        }
        
        [XmlIgnore]
        public bool HasChanges
        {
            get { return (m_changes.Count > 0);}
        }
                                
        public IList<RecordItemChange> GetQueue()
        {
            List<RecordItemChange> changes = m_changes.Values.ToList();
            changes.Sort((x, y) => RecordItemChange.Compare(x, y));
            return changes;
        }

        public IList<ItemKey> GetItemKeys()
        {
            return (
                from value in m_changes.Values
                select value.Key
            ).ToList();
        }
                
        public RecordItemChange GetChange(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                throw new ArgumentException("itemID");
            }

            RecordItemChange change = null;
            if (!m_changes.TryGetValue(itemID, out change))
            {
                change = null;  
            }

            return change;
        }
        
        public bool HasChangeForItem(string itemID)
        {
            return (this.GetChange(itemID) != null);
        }

        public bool HasChangesForType(string typeID)
        {
            if (string.IsNullOrEmpty(typeID))
            {
                throw new ArgumentException("typeID");
            }

            foreach(RecordItemChange change in m_changes.Values)
            {
                if (change.IsChangeForType(typeID))
                {
                    return true;
                }
            }

            return false;
        }

        //
        // In practice, RecordChangeToItem is accessed by SynchronizedStore, which prevents simultaneous changes to 
        // an item by taking an item lock
        //
        public void TrackChangeToItem(RecordItem item, RecordItemChangeType changeType)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            item.Key.ValidateRequired("item.Key");
            item.Type.ValidateRequired("item.Type");

            this.TrackChange(item.Type.ID, item.Key, changeType);
        }
        
        public void TrackChange(string typeID, ItemKey key, RecordItemChangeType changeType)
        {
            if (string.IsNullOrEmpty(typeID))
            {
                throw new ArgumentException("typeID");
            }
            if (key == null)
            {
                throw new ArgumentException("key");
            }

            m_changes[key.ID] = RecordItemChange.UpdateChange(typeID, key, changeType, this.GetChange(key.ID));
        }

        public void RemoveChangeForItem(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                throw new ArgumentException("itemID");
            }

            m_changes.Remove(itemID);
        }
        
        public void RemoveChangesForType(string typeID)
        {
            if (string.IsNullOrEmpty(typeID))
            {
                throw new ArgumentException("typeID");
            }

            IList<RecordItemChange> changes = m_changes.Values.ToList();
            foreach(RecordItemChange change in changes)
            {
                if (change.IsChangeForType(typeID))
                {
                    this.RemoveChangeForItem(change.ItemID);
                }
            }
        }

        /// <summary>
        /// Note: the XmlSerializer requires this specific method signature 
        /// </summary>
        /// <param name="change"></param>
        public void Add(RecordItemChange change)
        {
            change.ValidateRequired("change");

            if (m_changes.ContainsKey(change.ItemID))
            {
                throw new StoreException(StoreErrorNumber.DuplicateChange);
            }

            m_changes.Add(change.ItemID, change);
        }
        
        public IEnumerator<RecordItemChange> GetEnumerator()
        {
            return m_changes.Values.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public static RecordItemChangeList Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<RecordItemChangeList>(xml);
        }

        public void Validate()
        {            
        }
    }
}
