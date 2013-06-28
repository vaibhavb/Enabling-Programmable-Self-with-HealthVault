using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Diagnostics;
using Windows.Foundation;
using HealthVault.Foundation;
using HealthVault.Types;
using HealthVault.ItemTypes;
using DateTime = HealthVault.Types.DateTime;

namespace HealthVault.Store
{
    /// <summary>
    /// A SynchronizedView with WRITE/REMOVE functions
    /// Maintains a 2-way synchronized copy of the RecordItems in a specific HealthVault data type. 
    /// </summary>
    public sealed class SynchronizedType : ISynchronizedView
    {
        string m_typeID;
        SynchronizedTypeManager m_typeManager;
        SynchronizedView m_items;
        CrossThreadLock m_lock;

        internal SynchronizedType(SynchronizedTypeManager typeManager, string typeID)
        {
            m_typeID = typeID;
            m_typeManager = typeManager;
            m_lock = new CrossThreadLock(false);
        }

        public string TypeID
        {
            get { return m_typeID;}
        }

        public IRecord Record 
        {
            get { return m_items.Record;}
        }
        
        public int KeyCount
        {
            get { return m_items.KeyCount;}
        }

        public ViewKeyCollection Keys
        {
            get { return m_items.Keys;}
        }

        public HealthVault.Types.DateTime LastUpdated
        {
            get { return m_items.LastUpdated;}
        }

        public int ReadAheadChunkSize
        {
            get { return m_items.ReadAheadChunkSize;}
            set { m_items.ReadAheadChunkSize = value;}
        }

        public SynchronizedViewReadAheadMode ReadAheadMode
        {
            get { return m_items.ReadAheadMode;}
            set { m_items.ReadAheadMode = value;}
        }

        public SynchronizedStore Data
        {
            get { return m_typeManager.RecordStore.Data; }
        }
                
        public int MaxItems
        {
            get 
            { 
                NonNegativeInt maxItems = m_items.Data.Query.MaxResults;
                return (maxItems != null) ? maxItems.Value : -1;
            }
            set
            {
                if (value == 0)
                {
                    throw new ArgumentException("value");
                }
                NonNegativeInt maxValue = (value > 0) ? new NonNegativeInt(value) : null;
                m_items.Data.Query.MaxResults = maxValue;
            }
        }

        public DateTime EffectiveDateMin
        {
            get { return this.Filter.EffectiveDateMin;}
            set { this.Filter.EffectiveDateMin = value;}
        }

        public DateTime EffectiveDateMax
        {
            get { return this.Filter.EffectiveDateMax; }
            set { this.Filter.EffectiveDateMax = value; }
        }

        internal LocalRecordStore Store
        {
            get { return m_typeManager.RecordStore;}
        }

        internal ItemFilter Filter
        {
            get { return m_items.Data.Query.FirstFilter; }
            set { m_items.Data.Query.FirstFilter = value; }
        }

        internal SynchronizedView Items
        {
            get { return m_items;}
        }

        public event EventHandler<IList<ItemKey>> ItemsAvailable;
        /// <summary>
        /// Passes the keys which were not found
        /// </summary>
        public event EventHandler<IList<ItemKey>> ItemsNotFound;
        /// <summary>
        /// Exception!
        /// </summary>
        public event EventHandler<Exception> Error;
        
        public bool IsStale(int maxAgeInSeconds)
        {
            return m_items.IsStale(maxAgeInSeconds);
        }

        public ItemKey KeyAtIndex(int index)
        {
            return m_items.KeyAtIndex(index);
        }

        public IAsyncOperation<IItemDataTyped> GetItemAsync(int index)
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using(await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.GetItemAsync(index, false, cancelToken);
                }
            });
        }

        public IAsyncOperation<IItemDataTyped> GetItemByKeyAsync(ItemKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return AsyncInfo.Run(async cancelToken =>
            {
                using(await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.GetItemByKeyAsync(key, false, cancelToken);
                }
            });
        }

        public IAsyncOperation<IItemDataTyped> GetLocalItemAsync(int index)
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.GetLocalItemAsync(index);
                }
            });
        }

        public IAsyncOperation<IItemDataTyped> GetLocalItemByKeyAsync(ItemKey key)
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.GetLocalItemByKeyAsync(key);
                }
            });
        }

        public IAsyncOperation<IList<ItemKey>> GetKeysForItemsNeedingDownload(int startAt, int count)
        {
            return AsyncInfo.Run<IList<ItemKey>>(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.GetKeysForItemsNeedingDownload(startAt, count);
                }
            });
        }

        public IAsyncOperation<IList<IItemDataTyped>> GetItemsAsync(int startAt, int count)
        {
            return AsyncInfo.Run(async cancelToken => 
            {
                using(await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.GetItemsAsync(startAt, count, false, cancelToken);
                }    
            });
        }

        public IAsyncOperation<IItemDataTyped> EnsureItemAvailableAndGetAsync(int index)
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.GetItemAsync(index, true, cancelToken);
                }
            });
        }
        
        public IAsyncOperation<IItemDataTyped> EnsureItemAvailableAndGetByKeyAsync(ItemKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.GetItemByKeyAsync(key, true, cancelToken);
                }
            });
        }

        public IAsyncOperation<IList<IItemDataTyped>> EnsureItemsAvailableAndGetAsync(int startAt, int count)
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.GetItemsAsync(startAt, count, true, cancelToken);
                }
            });
        }
        
        public IAsyncOperation<bool> HasPendingChangesAsync()
        {
            return AsyncInfo.Run<bool>(async cancelToken => {
                return await this.Data.Changes.HasChangesForTypeAsync(m_typeID);
            });
        }

        /// <summary>
        /// Synchronize --> update the list of known Items by fetching a fresh list of ItemKeys from HealthVault
        /// 
        /// If there are pending changes (writes, removes) that have not been committed to HealthVault yet, synchronize will do nothing 
        /// and return false. 
        /// 
        /// </summary>
        /// <returns>FALSE if there are pending changes that have not yet been committed to HealthVault</returns>
        public IAsyncOperation<bool> SynchronizeAsync()
        {
            return AsyncInfo.Run<bool>(async cancelToken => {
                if (await this.Data.Changes.HasChangesForTypeAsync(m_typeID))
                {
                    return false;
                }

                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await m_items.SynchronizeAsyncImpl(cancelToken);
                    await this.SaveView();
                    return true;
                }
            });
        }

        // Returns true if a sync was actually triggered
        public IAsyncOperation<bool> SynchronizeIfStaleAsync(int maxAgeInSeconds)
        {
            return AsyncInfo.Run<bool>(async cancelToken =>
            {
                if (!this.IsStale(maxAgeInSeconds))
                {
                    return false;
                }
                
                return await this.SynchronizeAsync();
            });
        }
        
        public ItemQuery GetSynchronizationQuery()
        {
            return m_items.GetSynchronizationQuery();
        }

        public IList<string> GetTypeVersions()
        {
            return m_items.GetTypeVersions();
        }

        public void UpdateKeys(ViewKeyCollection keys)
        {
            m_items.UpdateKeys(keys);   
        }

        /// <summary>
        /// Add a new item. The item is saved in the local store immediately, and a pending commit to the remote store is put in 
        /// the synchronized store's change queue
        /// </summary>
        public IAsyncAction AddNewAsync(IItemDataTyped item)
        {
            this.ValidateItem(item);

            RecordItem recordItem = item.Item;
            SynchronizedStore.PrepareForNew(recordItem);

            return AsyncInfo.Run(async cancelToken => {                
                RecordItemLock rlock = this.AcquireItemLock(recordItem.Key);
                if (rlock == null)
                {
                    return;
                }

                using(rlock)
                {
                    await this.Data.PutItemAsync(recordItem, rlock);
                    await this.AddKeyAsync(recordItem);
                }

                this.StartCommitChanges();
            });
        }
        
        /// <summary>
        /// Returns null if:
        /// 1. Could not take a lock on the item
        /// 2. Item was not available locally
        /// 
        /// When you are done editing, call CommitAsync() on the RecordItemEditOperation
        /// To abort, call RecordItemEditOperation::Cancel() 
        /// 
        /// </summary>
        public IAsyncOperation<RecordItemEditOperation> OpenForEditAsync(ItemKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return AsyncInfo.Run<RecordItemEditOperation>(async cancelToken =>
            {
                RecordItemLock rLock = this.AcquireItemLock(key);
                if (rLock == null)
                {
                    return null;
                }

                RecordItemEditOperation editOp = null;
                try
                {
                    editOp = await this.OpenForEditAsync(key, rLock);
                    return editOp;
                }
                finally
                {
                    if (editOp == null)
                    {
                        rLock.Release();
                    }
                }
            });            
        }
        
        /// <summary>
        /// Before removing the item, will try to take a lock on the item in question. 
        /// If it can't, it will return FALSE
        /// </summary>
        public IAsyncOperation<bool> RemoveAsync(ItemKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return AsyncInfo.Run(async cancelToken =>
            {
                RecordItemLock rLock = this.AcquireItemLock(key);
                if (rLock == null)
                {
                    return false;
                }
                using (rLock)
                {
                    await this.RemoveAsync(key, rLock);
                }
                                
                this.StartCommitChanges();
                return true;
            });
        }

        public IAsyncAction SaveAsync()
        {
            return AsyncInfo.Run(async cancelToken => {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    await this.SaveView();
                }
            });
        }

        // PredicateDelegate is passed IItemDataTyped objects
        public IAsyncOperation<IList<IItemDataTyped>> SelectAsync(PredicateDelegate predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return AsyncInfo.Run(async cancelToken => {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.SelectAsync(predicate, cancelToken);
                }
            });
        }

        // PredicateDelegate is passed IItemDataTyped objects
        public IAsyncOperation<IList<ItemKey>> SelectKeysAsync(PredicateDelegate predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return AsyncInfo.Run<IList<ItemKey>>(async cancelToken =>
            {
                using (await CrossThreadLockScope.Enter(m_lock))
                {
                    return await m_items.SelectKeysAsync(predicate, cancelToken);
                }
            });
        }

        // Returns null if no lock acquired (somebody else owns it)
        internal RecordItemLock AcquireItemLock(ItemKey key)
        {
            return this.Data.Locks.AcquireItemLock(key.ID);
        }
        
        internal async Task<RecordItemEditOperation> OpenForEditAsync(ItemKey key, RecordItemLock itemLock)
        {
            IItemDataTyped data = await this.EnsureItemAvailableAndGetByKeyAsync(key);
            if (data == null)
            {
                return null;
            }

            return new RecordItemEditOperation(this, data, itemLock);
        }

        internal IAsyncAction PutAsync(IItemDataTyped item, RecordItemLock itemLock)
        {
            this.ValidateItem(item);

            return AsyncInfo.Run(async cancelToken => {            
                await this.Data.PutAsync(item, itemLock);
                await this.UpdateKeyAsync(item.Item);
            });
        }

        internal IAsyncAction RemoveAsync(ItemKey key, RecordItemLock itemLock)
        {
            key.ValidateRequired("key");

            return AsyncInfo.Run(async cancelToken =>
            {
                await this.Data.RemoveItemAsync(m_typeID, key, itemLock);
                await this.RemoveKeyAsync(key);
            });
        }

        internal void StartCommitChanges()
        {
            m_typeManager.StartCommitChanges();
        }
        
        async Task AddKeyAsync(RecordItem addedItem)
        {
            ViewKey viewKey = ViewKey.FromItem(addedItem);
            using (await CrossThreadLockScope.Enter(m_lock))
            {
                m_items.Keys.Add(viewKey);
                await this.SaveView();
            }
        }
                
        async Task UpdateKeyAsync(RecordItem updatedItem)
        {
            ViewKey viewKey = ViewKey.FromItem(updatedItem);
            using (await CrossThreadLockScope.Enter(m_lock))
            {
                m_items.Keys.UpdateKey(viewKey);
                await this.SaveView();
            }
        }
        
        async Task UpdateKeyAsync(string itemID, RecordItem updatedItem)
        {
            ViewKey viewKey = ViewKey.FromItem(updatedItem);
            using (await CrossThreadLockScope.Enter(m_lock))
            {
                m_items.Keys.UpdateKey(itemID, viewKey);
                await this.SaveView();
            }
        }
                
        async Task RemoveKeyAsync(ItemKey key)
        {
            using (await CrossThreadLockScope.Enter(m_lock))
            {
                m_items.Keys.RemoveByItemKey(key);
                await this.SaveView();
            }
        }

        internal async Task OnPutCommittedAsync(RecordItemChange change)
        {
            Debug.Assert(change.HasUpdatedItem || change.HasLocalData);

            RecordItem updatedItem = change.UpdatedItem;
            if (updatedItem == null)
            {
                updatedItem = change.LocalData.Item;
                updatedItem.Key = change.UpdatedKey;
            }

            await this.Data.Local.PutItemAsync(updatedItem);
            await this.UpdateKeyAsync(change.ItemID, updatedItem);
        }

        internal async Task Load()
        {
            string viewName = SynchronizedType.MakeViewName(m_typeID);
            m_items = await this.Store.GetViewAsync(viewName);
            if (m_items == null)
            {
                m_items = this.Store.CreateView(viewName, ItemQuery.QueryForTypeID(m_typeID));
                await this.SaveAsync();
            }

            m_items.PublicSyncDisabled = true;
            m_items.ItemsAvailable += OnItemsAvailable;
            m_items.ItemsNotFound += OnItemsNotFound;
            m_items.Error += OnError;
        }

        internal async Task SaveView()
        {
            await this.Store.PutViewAsync(m_items);
        }

        internal static string MakeViewName(string typeID)
        {
            return "Type_" + typeID;
        }

        /*
         * SynchronizedView fires events in the UI thread
         */
        void OnItemsAvailable(object sender, IList<ItemKey> e)
        {
            this.ItemsAvailable.SafeInvoke(sender, e);
        }

        void OnItemsNotFound(object sender, IList<ItemKey> e)
        {
            this.ItemsNotFound.SafeInvoke(sender, e);
        }
        
        void OnError(object sender, Exception e)
        {
            this.Error.SafeInvoke(sender, e);
        }

        ViewKey GetViewKey(int index)
        {
            return m_items.Data.KeyAtIndex(index);
        }

        void ValidateItem(IItemDataTyped item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            item.Type.ValidateRequired("Type");
            if (item.Type.ID != m_typeID)
            {
                throw new ArgumentException("TypeID mismatch");
            }
        }
    }
}
