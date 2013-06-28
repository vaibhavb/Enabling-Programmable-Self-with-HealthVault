using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using HealthVault.Foundation;
using HealthVault.Types;
using Windows.Foundation;
using Windows.Storage;

namespace HealthVault.Store
{
    /// <summary>
    /// Maintains a PERSISTENT Change Table. Thread Safe. 
    /// 
    /// We can replace the internal implementation without impacting callers. 
    /// Currently:
    ///     1. Each change is stored as its own object in the changeStore
    ///     2. For efficiency, the ChangeTable maintains an in-memory index of the ItemIDs of the Items that changed
    /// </summary>
    public sealed class RecordItemChangeTable
    {
        IObjectStore m_changeStore;         // Object store that stores actual change information
        HashSet<string> m_itemIDIndex;      // Index of IDs of items that changed
        CrossThreadLock m_lock;

        public RecordItemChangeTable(StorageFolder container, int maxCachedItems)
            : this(new FolderObjectStore(container), LRUCache<string, object>.Create(maxCachedItems))
        {
            
        }

        internal RecordItemChangeTable(IObjectStore changeStore, ICache<string, object> cache)
        {
            if (changeStore == null)
            {
                throw new ArgumentNullException("changeStore");
            }
            if (cache != null)
            {
                m_changeStore = new CachingObjectStore(changeStore, cache);
            }
            else
            {
                m_changeStore = changeStore;
            }
            m_lock = new CrossThreadLock(false);
        }

        // Return the # of pending changes
        public IAsyncOperation<int> GetChangeCountAsync()
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.EnsureIndexAsync();
                    return (m_itemIDIndex.Count);
                }
            });
        }
        
        // Return Ids of items that changed
        public IAsyncOperation<IList<string>> GetIDsOfChangedItemsAsync()
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.EnsureIndexAsync();
                    List<string> itemIDs =  m_itemIDIndex.ToList();
                    return (IList<string>) itemIDs;
                }
            });
        }
        
        // Get ids of items that changed for a given type ID
        public IAsyncOperation<IList<string>> GetIDsOfChangedItemsForTypeAsync(string typeID)
        {
            if (string.IsNullOrEmpty(typeID))
            {
                throw new ArgumentException("typeID");
            }

            return AsyncInfo.Run(async cancelToken =>
            {
                List<string> ids = new List<string>();
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.EnsureIndexAsync();
                    foreach (string itemID in m_itemIDIndex)
                    {
                        RecordItemChange change = await this.GetChangeAsync(itemID);
                        if (change != null && change.IsChangeForType(typeID))
                        {
                            ids.Add(itemID);
                        }
                    }

                    return (IList<string>) ids;
                }
            });
        }
        
        // Gets the current change information for the given itemID
        public IAsyncOperation<RecordItemChange> GetChangeForItemAsync(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                throw new ArgumentException("itemID");
            }
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return await this.GetChangeAsync(itemID);
                }
            });
        }
        
        // Returns a list of item IDs... in "queue" order...with the oldest change first...                
        public IAsyncOperation<IList<string>> GetChangeQueueAsync()
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    List<RecordItemChange> changes = await this.GetAllChangesAsync();
                    changes.Sort((x, y) => RecordItemChange.Compare(x, y));
                    List<string> queue = (
                        from change in changes
                        select change.ItemID
                    ).ToList();
                    
                    return (IList<string>) queue;
                }
            });
        }
        
        // Returns all changes registered with the change table
        public IAsyncOperation<IList<RecordItemChange>> GetChangesAsync()
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return (IList<RecordItemChange>) await this.GetAllChangesAsync();
                }
            });            
        }

        public IAsyncOperation<IList<RecordItemChange>> GetChangesForItemsAsync(IList<string> itemIDs)
        {
            if (itemIDs.IsNullOrEmpty())
            {
                throw new ArgumentException("itemID");
            }

            return AsyncInfo.Run<IList<RecordItemChange>>(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    List<RecordItemChange> changes = new List<RecordItemChange>();
                    foreach(string id in itemIDs)
                    {
                        RecordItemChange change = await this.GetChangeAsync(id);
                        if (change != null)
                        {
                            changes.Add(change);
                        }
                    }
                    return changes;
                }
            });
        }

        // Are there any pending changes? 
        public IAsyncOperation<bool> HasChangesAsync()
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.EnsureIndexAsync();
                    return (m_itemIDIndex.Count > 0);
                }
            });
        }

        // Are there any changes pending for the given item ID
        public IAsyncOperation<bool> HasChangesForItemAsync(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                throw new ArgumentException("itemID");
            }

            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.EnsureIndexAsync();
                    return m_itemIDIndex.Contains(itemID);
                } 
            });
        }
        
        // Are there any changes for the given type ID?
        public IAsyncOperation<bool> HasChangesForTypeAsync(string typeID)
        {
            if (string.IsNullOrEmpty(typeID))
            {
                throw new ArgumentException("typeID");
            }

            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.EnsureIndexAsync();
                    foreach(string itemID in m_itemIDIndex)
                    {
                        RecordItemChange change = await this.GetChangeAsync(itemID);
                        if (change != null && change.IsChangeForType(typeID))
                        {
                            return true;
                        }
                    }

                    return false;
                }
            });
        }

        // TRACK CHANGES for the given typeID & key
        public IAsyncAction TrackChangeAsync(string typeID, ItemKey key, RecordItemChangeType changeType)
        {
            if (string.IsNullOrEmpty(typeID))
            {
                throw new ArgumentException("typeID");
            }
            if (key == null)
            {
                throw new ArgumentException("key");
            }

            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    RecordItemChange change = await this.GetChangeAsync(key.ID);
                    change = RecordItemChange.UpdateChange(typeID, key, changeType, change);

                    await this.SaveChangeAsync(change);

                    this.UpdateIndex(key.ID);                    
                }
            });
        }
        
        // Remove changes for the given item ID
        public IAsyncAction RemoveChangeAsync(string itemID)
        {
            if (string.IsNullOrEmpty(itemID))
            {
                throw new ArgumentException("itemID");
            }
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.DeleteChangeAsync(itemID);                    
                }
            });
        }

        // Remove all changes
        public IAsyncAction RemoveAllChangesAsync()
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.EnsureIndexAsync();
                    string[] allIDs = m_itemIDIndex.ToArray();
                    foreach (string itemID in allIDs)
                    {
                        await this.DeleteChangeAsync(itemID);
                    }
                }
            });
        }

        // Remove all pending changes for a given type ID
        public IAsyncAction RemoveAllChangesForTypeAsync(string typeID)
        {
            if (string.IsNullOrEmpty(typeID))
            {
                throw new ArgumentException("typeID");
            }

            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.EnsureIndexAsync();
                    string[] allIDs = m_itemIDIndex.ToArray();
                    foreach (string itemID in allIDs)
                    {
                        RecordItemChange change = await this.GetChangeAsync(itemID);
                        if (change != null && change.IsChangeForType(typeID))
                        {
                            await this.DeleteChangeAsync(itemID);
                        }
                    }
                }
            });
        }

        async Task EnsureIndexAsync()
        {
            if (m_itemIDIndex == null)
            {
                await this.LoadIndexAsync();
                if (m_itemIDIndex == null)
                {
                    m_itemIDIndex = new HashSet<string>();
                }
            }
        }

        async Task LoadIndexAsync()
        {
            m_itemIDIndex = null;

            IList<string> keys = await m_changeStore.GetAllKeysAsync();
            if (!keys.IsNullOrEmpty())
            {
                m_itemIDIndex = new HashSet<string>(keys);
            }
        }

        void UpdateIndex(string itemID)
        {
            if (m_itemIDIndex != null)
            {
                m_itemIDIndex.Add(itemID); // Only adds to the HashSet if the item is not already present
            }
        }

        void RemoveFromIndex(string itemID)
        {
            if (m_itemIDIndex != null)
            {
                m_itemIDIndex.Remove(itemID);
            }    
        }

        async Task<RecordItemChange> GetChangeAsync(string itemID)
        {
            return (RecordItemChange) await m_changeStore.GetAsync(itemID, typeof(RecordItemChange));
        }
        
        internal async Task<List<RecordItemChange>> GetAllChangesAsync()
        {
            List<RecordItemChange> changes = new List<RecordItemChange>();
            await this.EnsureIndexAsync();
            foreach (string itemID in m_itemIDIndex)
            {
                RecordItemChange change = await this.GetChangeAsync(itemID);
                if (change != null)
                {
                    changes.Add(change);
                }
            }

            return changes;
        }

        internal async Task SaveChangeAsync(RecordItemChange change)
        {
            await m_changeStore.PutAsync(change.ItemID, change);
        }

        internal async Task DeleteChangeAsync(string itemID)
        {
            await m_changeStore.DeleteAsync(itemID);
            this.RemoveFromIndex(itemID);
        }
    }
}
