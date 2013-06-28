// (c) Microsoft. All rights reserved

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation;
using Windows.Foundation;
using Windows.Storage;

namespace HealthVault.Store
{
    /// <summary>
    /// Thread safe
    /// </summary>
    public sealed class LocalRecordStoreTable
    {
        private readonly ICache<string, object> m_itemCache;
        private readonly Dictionary<string, LocalRecordStore> m_recordStores;
        private readonly IObjectStore m_root;
        private readonly CrossThreadLock m_storageLock;
        private readonly RecordItemCommitScheduler m_commitScheduler;

        internal LocalRecordStoreTable(IObjectStore root)
        {
            m_root = root;
            m_storageLock = new CrossThreadLock(false);
            m_itemCache = new LRUCache<string, object>(0);
            m_recordStores = new Dictionary<string, LocalRecordStore>();
            m_commitScheduler = new RecordItemCommitScheduler(this);
        }

        public LocalRecordStoreTable(StorageFolder root)
            : this(new FolderObjectStore(root))
        {
        }

        public int MaxCachedItems
        {
            get { return m_itemCache.MaxCount; }
            set { m_itemCache.MaxCount = value; }
        }

        /// <summary>
        /// Background commits are disabled by default. See notes on RecordItemCommitScheduler
        /// </summary>
        public RecordItemCommitScheduler BackgroundCommitScheduler
        {
            get { return m_commitScheduler;}
        }

        internal ICache<string, object> ItemCache
        {
            get { return m_itemCache; }
        }

        public LocalRecordStore GetStoreForRecord(IRecord record)
        {
            if (record == null)
            {
                throw new ArgumentException(null);
            }
            
            return this.EnsureRecordStoreObject(record);
        }

        public void RemoveAllStores()
        {
            Task.Run(() => this.RemoveAllStoresAsync()).Wait();
        }

        public IAsyncAction RemoveAllStoresAsync()
        {
            return AsyncInfo.Run(async cancelToken => {
                IRecord[] records = this.GetAllRecords();
                foreach (IRecord record in records)
                {
                    await this.RemoveStoreForRecordIDAsync(record.ID);
                }
            });

        }

        public void RemoveStoreForRecord(IRecord record)
        {
            if (record != null)
            {
                this.RemoveStoreForRecordID(record.ID);
            }
        }

        public void RemoveStoreForRecordID(string recordID)
        {
            Task.Run(() => this.RemoveStoreForRecordIDAsync(recordID)).Wait();
        }

        public IAsyncAction RemoveStoreForRecordIDAsync(string recordID)
        {
            if (string.IsNullOrEmpty(recordID))
            {
                throw new ArgumentException("recordID");
            }
            
            return AsyncInfo.Run(async cancelToken => {
                m_itemCache.Clear();
                bool deleteStorage = this.RemoveRecordStoreObject(recordID);
                if (deleteStorage)
                {
                    await this.DeleteStorageForRecordAsync(recordID);
                }                
            });
        }

        public IAsyncOperation<bool> HasChangesAsync()
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                IRecord[] records = this.GetAllRecords();

                using(await CrossThreadLockScope.Enter(m_storageLock))
                {
                    foreach (IRecord record in records)
                    {
                        LocalRecordStore store = this.GetRecordStoreObjectIfExists(record);
                        if (store != null && await store.Data.HasChangesAsync())
                        {
                            return true;
                        }
                    }
                }
                        
                return false;
            });
        }

        public IAsyncAction CommitChangesAsync()
        {
            return AsyncInfo.Run(async cancelToken => 
            {               
                IRecord[] records = this.GetAllRecords();

                using(await CrossThreadLockScope.Enter(m_storageLock))
                {
                    foreach (IRecord record in records)
                    {
                        LocalRecordStore store = this.GetRecordStoreObjectIfExists(record);
                        if (store != null)
                        {
                            await store.Data.CommitChangesAsync(cancelToken);
                        }
                    }
                }
            });
        }

        internal void ResetRecordObjects(UserInfo userInfo)
        {
            lock (m_recordStores)
            {
                if (userInfo == null)
                {
                    throw new ArgumentNullException("userInfo");
                }

                foreach (IRecord record in userInfo.AuthorizedRecords)
                {
                    LocalRecordStore recordStore = null;
                    if (m_recordStores.TryGetValue(record.ID, out recordStore))
                    {
                        recordStore.Record = record;
                    }
                }
            }
        }

        async Task DeleteStorageForRecordAsync(string recordID)
        {
            using(await CrossThreadLockScope.Enter(m_storageLock))
            {
                System.Diagnostics.Debug.WriteLine("Deleting storage for {0}", recordID);
                await m_root.DeleteChildStoreAsync(recordID);
            }
        }

        LocalRecordStore EnsureRecordStoreObject(IRecord record)
        {
            lock (m_recordStores)
            {
                LocalRecordStore recordStore = null;
                if (!m_recordStores.TryGetValue(record.ID, out recordStore))
                {
                    recordStore = new LocalRecordStore(record, m_root, this);
                    m_recordStores[record.ID] = recordStore;
                }
                return recordStore;
            }
        }
        
        LocalRecordStore GetRecordStoreObjectIfExists(IRecord record)
        {
            lock (m_recordStores)
            {
                LocalRecordStore recordStore = null;
                if (record != null && m_recordStores.TryGetValue(record.ID, out recordStore))
                {
                    return recordStore;
                }

                return null;
            }
        }

        IRecord[] GetAllRecords()
        {
            lock (m_recordStores)
            {
                return (
                    from recordStore in m_recordStores.Values
                    select recordStore.Record
                ).ToArray();
            }
        }

        bool RemoveRecordStoreObject(string recordID)
        {
            lock (m_recordStores)
            {
                LocalRecordStore store = null;
                if (m_recordStores.TryGetValue(recordID, out store) && store != null)
                {
                    store.Data.Changes.IsCommitEnabled = false;
                    m_recordStores.Remove(recordID);
                    return true;
                }

                return false;
            }
        }
    }

}