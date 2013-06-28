// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation;
using HealthVault.ItemTypes;
using HealthVault.Types;
using Windows.Foundation;

namespace HealthVault.Store
{
    /// <summary>
    /// Thread safe, 2 way sync
    /// </summary>
    public sealed class SynchronizedStore
    {
        //private IRecord m_record;
        private readonly LocalItemStore m_localStore;
        IRemoteItemStore m_remoteStore;
        RecordItemChangeManager m_changeManager;
        RecordItemLockTable m_itemLocks;

        internal SynchronizedStore(IRecord record, LocalItemStore localStore, RecordItemChangeTable changeTable)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (localStore == null)
            {
                throw new ArgumentNullException("itemStore");
            }
            if (changeTable == null)
            {
                throw new ArgumentNullException("changeTable");
            }

            //m_record = record;
            SectionsToFetch = ItemSectionType.Standard;

            m_localStore = localStore;
            m_remoteStore = new RemoteItemStore(record);
            m_changeManager = new RecordItemChangeManager(this, changeTable);
            m_itemLocks = new RecordItemLockTable();
        }
       
        public IRecord Record
        {
            get 
            { 
              //  return m_record; 
              return m_remoteStore.Record;
            }
            set 
            { 
                //m_record = value; 
                m_remoteStore.Record = value;
            }
        }
        
        // Local storage of items. 
        // Downloaded items are cached here, as are all LOCAL edits
        public LocalItemStore Local
        {
            get { return m_localStore; }
        }

        // The remote store from which data is pulled and TO which local edits/removes are committed
        public IRemoteItemStore RemoteStore
        {
            get { return m_remoteStore;}
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("RemoteStore");
                }
                m_remoteStore = value;
            }
        }

        /// <summary>
        /// This Synchronized Store's Item Lock table
        /// </summary>
        public RecordItemLockTable Locks
        {
            get { return m_itemLocks; }
        }
        
        /// <summary>
        /// This store's pending changes..
        /// </summary>
        public RecordItemChangeManager Changes
        {
            get { return m_changeManager;}
        }

        public ItemSectionType SectionsToFetch { get; set; }        

        public static void PrepareForNew(RecordItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            item.Key = ItemKey.NewLocalKey(); // A special "Local only" Key
            item.UpdateEffectiveDate();
        }

        /// <summary>
        /// This completes only when all locally available AND any pending items have been downloaded
        /// See GetAsync(keys, callback) for an alternative
        /// </summary>
        /// <param name="keys"></param>
        /// <param name="typeVersions"></param>
        /// <returns></returns>
        public IAsyncOperation<IList<IItemDataTyped>> GetAsync(
            IList<ItemKey> keys,
            IList<string> typeVersions)
        {
            return GetAsync(keys, typeVersions, null);
        }

        /// <summary>
        /// Returns a list of items such that:
        ///     - If an item matching the equivalent key is available locally, returns it
        ///     - ELSE returns a NULL item at the ordinal matching the key - indicating that the item is PENDING
        ///     - Issues a background get for the pending items, and notifies you (callback) when done
        /// This technique is useful for making responsive UIs. 
        ///   - User can view locally available items quickly
        ///   - Pending items are shown as 'loading' and updated as they become available
        /// </summary>
        /// <param name="keys">keys to retrieve</param>
        /// <param name="typeVersions"></param>
        /// <returns>List with COUNT == keys.Count. If an item was not found locally, the equivalent item in the list is NULL</returns>
        public IAsyncOperation<IList<IItemDataTyped>> GetAsync(
            IList<ItemKey> keys,
            IList<string> typeVersions, 
            PendingGetCompletionDelegate callback)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            return AsyncInfo.Run(cancelToken => GetAsyncImpl(keys, typeVersions, callback, cancelToken));
        }


        /// <summary>
        /// Returns a list of items such that:
        ///     - If an item matching the equivalent key is available locally, returns it
        ///     - ELSE returns a NULL item at the ordinal matching the key - indicating that the item is PENDING
        ///     - Issues a background get for the pending items, and notifies you (callback) when done
        /// This technique is useful for making responsive UIs. 
        ///   - User can view locally available items quickly
        ///   - Pending items are shown as 'loading' and updated as they become available
        /// </summary>
        /// <param name="keys">keys to retrieve</param>
        /// <param name="typeVersions"></param>
        /// <returns>List with COUNT == keys.Count. If an item was not found locally, the equivalent item in the list is NULL</returns>
        /// 
        public IAsyncOperation<IList<RecordItem>> GetItemsAsync(
            IList<ItemKey> keys,
            IList<string> typeVersions)
        {
            return GetItemsAsync(keys, typeVersions, null);
        }

        public IAsyncOperation<IList<RecordItem>> GetItemsAsync(
            IList<ItemKey> keys,
            IList<string> typeVersions, 
            PendingGetCompletionDelegate callback)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            return AsyncInfo.Run(async cancelToken => await GetItemsAsyncImpl(keys, typeVersions, callback, cancelToken));
        }

        public IAsyncOperation<IList<IItemDataTyped>> GetByViewKeysAsync(
            IList<ViewKey> keys,
            IList<string> typeVersions)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }
            
            return GetAsync(keys.Select(k => k.Key).ToArray(), typeVersions);
        }

        /// <summary>
        /// Refresh any items that need refreshing
        /// </summary>
        public IAsyncOperation<PendingGetResult> RefreshAsync(
            IList<ItemKey> keys,
            IList<string> typeVersions)
        {
            return AsyncInfo.Run(async cancelToken => await RefreshAsyncImpl(keys, typeVersions, null, cancelToken));
        }

        public IAsyncAction RefreshAsync(
            IList<ItemKey> keys,
            IList<string> typeVersions, 
            PendingGetCompletionDelegate callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            return AsyncInfo.Run(async cancelToken => { await RefreshAsyncImpl(keys, typeVersions, callback, cancelToken); });
        }

        /// <summary>
        /// Triggers a FORCED refresh in the background and returns as soon as it can
        /// When the background fetch completes, invokes the callback
        /// </summary>
        public IAsyncAction DownloadAsync(
            IList<ItemKey> keys,
            IList<string> typeVersions, 
            PendingGetCompletionDelegate callback)
        {
            if (callback == null)
            {
                throw new ArgumentNullException("callback");
            }

            return AsyncInfo.Run(async cancelToken => { await DownloadAsyncImpl(keys, typeVersions, callback, cancelToken); });
        }

        public IAsyncOperation<PendingGetResult> DownloadAsync(
            IList<ItemKey> keys,
            IList<string> typeVersions)
        {
            return AsyncInfo.Run(async cancelToken => await DownloadAsyncImpl(keys, typeVersions, null, cancelToken));
        }

        /// <summary>
        /// Puts a new item into the synchronized store. The new item will be comitted to the remote store in the background
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        public IAsyncAction NewAsync(IItemDataTyped item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            return this.NewItemAsync(item.Item);        
        }

        public IAsyncAction NewItemAsync(RecordItem item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            SynchronizedStore.PrepareForNew(item);

            return AsyncInfo.Run(async cancelToken =>
            {
                RecordItemLock rlock = m_itemLocks.AcquireItemLock(item.ID);
                if (rlock != null)
                {
                    using (rlock)
                    {
                        await this.PutItemAsync(item, rlock);
                    }
                }
            });
        }
        
        /// <summary>
        /// Updates an existing item. To update, you must first acquire a lock on it, and prove that you own the lock. 
        /// Note: if you use higher level objects like SynchronizedStore, you won't have to acquire locks yourself. 
        /// </summary>
        public IAsyncAction PutAsync(IItemDataTyped item, RecordItemLock itemLock)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            return this.PutItemAsync(item.Item, itemLock);
        }

        public IAsyncAction PutItemAsync(RecordItem item, RecordItemLock itemLock)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
            if (itemLock == null)
            {
                throw new ArgumentNullException("itemLock");
            }

            m_itemLocks.ValidateLock(item.ID, itemLock.LockID);
            
            item.UpdateEffectiveDate();

            return AsyncInfo.Run(async cancelToken => {
                await m_localStore.PutItemAsync(item);
                await m_changeManager.TrackPutAsync(item);
            });
        }
        
        /// <summary>
        /// Remove an item. To remove, you must first acquire a lock on the item, and prove that you own the lock.
        /// If you use the higher level SychronizedType object, you won't have to acquire the lock yourself
        /// </summary>
        public IAsyncAction RemoveItemAsync(string typeID, ItemKey itemKey, RecordItemLock itemLock)
        {
            itemKey.ValidateRequired("key");
            if (itemLock == null)
            {
                throw new ArgumentNullException("itemLock");
            }

            m_itemLocks.ValidateLock(itemKey.ID, itemLock.LockID);

            return AsyncInfo.Run(async cancelToken => {                
                await m_localStore.RemoveItemAsync(itemKey);
                await m_changeManager.TrackRemoveAsync(typeID, itemKey);                
            });
        }        
        
        // Do we have pending changes? 
        public IAsyncOperation<bool> HasChangesAsync()
        {
            return m_changeManager.HasChangesAsync();
        }
        
        // Commit pending changes
        public IAsyncAction CommitChangesAsync()
        {
            return m_changeManager.CommitAsync();
        }

        internal Task CommitChangesAsync(CancellationToken cancelToken)
        {
            return m_changeManager.CommitAsync(cancelToken);
        }
                                    
        internal async Task<IList<IItemDataTyped>> GetAsyncImpl(IList<ItemKey> keys, IList<string> typeVersions, PendingGetCompletionDelegate callback, CancellationToken cancelToken)
        {
            IList<RecordItem> items = await GetItemsAsyncImpl(keys, typeVersions, callback, cancelToken);
            if (items != null)
            {
                return (
                    from item in items
                    select (item != null ? item.TypedData : null)
                    ).ToArray();
            }

            return null;
        }

        internal async Task<IList<RecordItem>> GetItemsAsyncImpl(IList<ItemKey> keys, IList<string> typeVersions,
            PendingGetCompletionDelegate callback, 
            CancellationToken cancelToken)
        {
            //
            // true: include null items - i.e. items not found in the local store
            //
            IList<RecordItem> foundItems = await m_localStore.GetItemsAsyncImpl(keys, true);

            //
            // Trigger a download of items that are not available yet...
            //
            IList<ItemKey> pendingKeys = CollectKeysNeedingDownload(keys, typeVersions, foundItems);
            if (pendingKeys.IsNullOrEmpty())
            {
                return foundItems;
            }

            PendingGetResult pendingResult = await DownloadAsyncImpl(pendingKeys, typeVersions, callback, cancelToken);
            if (pendingResult == null)
            {
                return foundItems;
            }

            //
            // Load fresh items
            //
            if (pendingResult.HasKeysFound)
            {
                await LoadNewItems(foundItems, keys, pendingResult.KeysFound);
            }

            return foundItems;
        }

        internal async Task<PendingGetResult> DownloadAsyncImpl(IList<ItemKey> keys, IList<string> typeVersions, PendingGetCompletionDelegate callback, CancellationToken cancelToken)
        {
            if (callback != null)
            {
                //
                // Run the download in the background. 
                // Return what we have right away, and notify caller when pending items arrive
                //
                Task task = DownloadItems(keys, typeVersions, callback, cancelToken);
                return null;
            }

            //
            // Wait for download to complete...
            //
            PendingGetResult result = await DownloadItems(keys, typeVersions, callback, cancelToken);
            result.EnsureSuccess();

            return result;
        }

        internal async Task<PendingGetResult> RefreshAsyncImpl(IList<ItemKey> keys, IList<string> typeVersions, PendingGetCompletionDelegate callback, CancellationToken cancelToken)
        {
            IList<ItemKey> pendingKeys = await CollectKeysNotInLocalStore(keys, typeVersions);
            if (pendingKeys.IsNullOrEmpty())
            {
                return null; // No pending work
            }

            System.Diagnostics.Debug.WriteLine("Downloading {0} items", keys.Count);

            return await DownloadAsyncImpl(pendingKeys, typeVersions, callback, cancelToken);
        }
        
        internal async Task<PendingGetResult> DownloadItems(IList<ItemKey> keys, IList<string> typeVersions, PendingGetCompletionDelegate callback, CancellationToken cancelToken)
        {
            var result = new PendingGetResult();
            try
            {
                result.KeysRequested = keys;

                ItemQuery query = this.CreateRefreshQueryForKeys(keys, typeVersions);

                IList<RecordItem> items = await m_remoteStore.GetAllItemsAsync(query);
                
                 await this.PersistDownloadedItemsAsync(items);

                result.KeysFound = (
                    from item in items
                    select item.Key
                    ).ToArray();
            }
            catch (Exception ex)
            {
                result.Exception = ex;
            }

            NotifyPendingGetComplete(callback, result);

            return result;
        }

        internal async Task PersistDownloadedItemsAsync(IList<RecordItem> items)
        {
            foreach(RecordItem item in items)
            {
                await this.SafePutItemInLocalStoreAsync(item);
            }
        }

        //        
        // Will only write the item to the local store IF:
        //  - Can take the write lock on the item (no pending edits in progress)
        //  - There are no pending changes on the item
        // 
        async Task SafePutItemInLocalStoreAsync(RecordItem item)
        {
            long lockID = m_itemLocks.AcquireLock(item.ID);
            if (!RecordItemLockTable.IsValidLockID(lockID))
            {
                return;  // Item is locked for editing. Don't overrwrite yet
            }

            try
            {
                // Make sure there are no pending updates
                if (!await m_changeManager.HasChangesForItemAsync(item.ID))
                {
                    await m_localStore.PutItemAsync(item);
                }
            }
            finally
            {
                m_itemLocks.SafeReleaseLock(item.ID, lockID);
            }
        }

        internal ItemQuery CreateRefreshQueryForKeys(IList<ItemKey> keys, IList<string> typeVersions)
        {
            ItemQuery query = ItemQuery.QueryForKeys(keys);
            query.View.SetSections(SectionsToFetch);
            if (!typeVersions.IsNullOrEmpty())
            {
                query.View.TypeVersions.AddRange(typeVersions);
            }

            return query;
        }

        internal async Task<IList<ItemKey>> CollectKeysNotInLocalStore(IList<ItemKey> keys, IList<string> typeVersions)
        {
            //
            // true: include null items - i.e. items not found in the local store
            //
            IList<RecordItem> foundItems = await m_localStore.GetItemsAsyncImpl(keys, true);
            //
            // Trigger a download of items that are not available yet...
            //
            return CollectKeysNeedingDownload(keys, typeVersions, foundItems);
        }

        internal IList<ItemKey> CollectKeysNeedingDownload(IList<ItemKey> requestedKeys,  IList<string> typeVersions, IList<RecordItem> collectedLocalItems)
        {
            var checkVersion = typeVersions != null;
            var typeVersionHash = checkVersion ? new HashSet<string>(typeVersions) : null;

            var pendingKeys = new LazyList<ItemKey>();
            for (int i = 0, count = requestedKeys.Count; i < count; ++i)
            {
                RecordItem localItem = collectedLocalItems[i];

                if (localItem == null ||
                    (checkVersion && !typeVersionHash.Contains(localItem.Type.ID)))
                {
                    pendingKeys.Add(requestedKeys[i]);
                }
            }

            return (pendingKeys.Count > 0) ? pendingKeys.Value : null;
        }

        internal async Task LoadNewItems(IList<RecordItem> itemList, IList<ItemKey> keysRequested, IList<ItemKey> newKeysFound)
        {
            if (itemList.Count != keysRequested.Count)
            {
                throw new InvalidOperationException();
            }

            int iNewKey = 0;
            for (int i = 0, count = keysRequested.Count; i < count; ++i)
            {
                ItemKey keyRequested = keysRequested[i];
                if (keyRequested.EqualsKey(newKeysFound[i]))
                {
                    itemList[i] = await m_localStore.GetItemAsyncImpl(keyRequested);
                    ++iNewKey;
                }
            }
        }

        private void NotifyPendingGetComplete(PendingGetCompletionDelegate callback, PendingGetResult result)
        {
            if (callback != null)
            {
                try
                {
                    callback(this, result);
                }
                catch
                {
                }
            }
        }
    }
}