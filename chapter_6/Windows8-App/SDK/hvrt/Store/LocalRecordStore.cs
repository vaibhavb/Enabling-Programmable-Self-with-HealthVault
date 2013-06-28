// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using System.Threading;
using HealthVault.Foundation;
using HealthVault.Types;
using Windows.Foundation;
using Windows.Storage;

namespace HealthVault.Store
{
    public sealed class LocalRecordStore
    {
        private IObjectStore m_root;
        private LocalStore m_blobs;
        private SynchronizedStore m_dataStore;
        private LocalStore m_metadataStore;
        private IRecord m_record;
        private CrossThreadLock m_metadataLock;
        private SynchronizedTypeManager m_synchronizedTypes;

        internal LocalRecordStore(IRecord record, IObjectStore parentStore, LocalRecordStoreTable recordStoreTable)
        {
            Initialize(record, parentStore, recordStoreTable);
        }

        public LocalRecordStore(IRecord record, StorageFolder folder)
            : this(record, new FolderObjectStore(folder), null)
        {
        }

        public LocalRecordStore(IRecord record, StorageFolder folder, string encryptionKey)
        {
            IObjectStore store = new FolderObjectStore(folder);
            if (!String.IsNullOrEmpty(encryptionKey))
            {
                store = new EncryptedObjectStore(store, new Cryptographer(), encryptionKey);
            }

            Initialize(record, store, null);
        }

        private void Initialize(IRecord record, IObjectStore parentStore, LocalRecordStoreTable recordStoreTable)
        {
            m_record = record;
            m_metadataLock = new CrossThreadLock(false);
            Task.Run(() => EnsureStores(parentStore, recordStoreTable)).Wait();
        }

        public IRecord Record
        {
            get { return m_record; }
            set
            {
                // Update local store's record reference
                m_record = value;
                if (m_dataStore != null)
                {
                    m_dataStore.Record = m_record;
                }
            }
        }

        public SynchronizedStore Data
        {
            get { return m_dataStore; }
        }

        public LocalStore Blobs
        {
            get { return m_blobs; }
        }
        
        internal IObjectStore Root
        {
            get { return m_root;}
        }

        public SynchronizedTypeManager Types
        {
            get { return m_synchronizedTypes;}
        }

        public SynchronizedView CreateView(ItemQuery query)
        {
            return new SynchronizedView(m_dataStore, query, query.Name);
        }

        public SynchronizedView CreateView(string viewName, ItemQuery query)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentException("viewName");
            }

            return new SynchronizedView(m_dataStore, query, viewName);
        }

        /// <summary>
        /// Loads a view with the given name. If not found, returns null
        /// </summary>
        public IAsyncOperation<SynchronizedView> GetViewAsync(string viewName)
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                return await this.GetViewAsync(viewName, cancelToken);
            });
        }

        public IAsyncOperation<IList<SynchronizedView>> GetViewsAsync(IList<string> viewNames)
        {
            if (viewNames.IsNullOrEmpty())
            {
                throw new ArgumentException("viewNames");
            }

            return AsyncInfo.Run<IList<SynchronizedView>>(async cancelToken => 
            {
                LazyList<SynchronizedView> views = new LazyList<SynchronizedView>();
                foreach(string viewName in viewNames)
                {
                    SynchronizedView view = await this.GetViewAsync(viewName, cancelToken);
                    if (view != null)
                    {
                        views.Add(view);
                    }
                }

                return views.HasValue ? views.Value : null;
            });
        }

        public IAsyncAction PutViewAsync(SynchronizedView view)
        {
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }

            view.Name.ValidateRequired("view");

            return AsyncInfo.Run(async cancelToken =>
            {
                await this.PutViewAsync(view, cancelToken);
            });
        }

        public IAsyncAction PutViewsAsync(IList<SynchronizedView> views)
        {
            if (views.IsNullOrEmpty())
            {
                throw new ArgumentException("views");
            }

            return AsyncInfo.Run(async cancelToken => 
            {
                foreach(SynchronizedView view in views)
                {
                    view.ValidateRequired("view");
                    await this.PutViewAsync(view, cancelToken);
                }
            });
        }

        public IAsyncAction DeleteViewAsync(string viewName)
        {
            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          using (await CrossThreadLockScope.Enter(m_metadataLock))
                          {
                              await m_metadataStore.DeleteAsync(MakeViewKey(viewName));
                          }
                      });
        }

        public IAsyncOperation<StoredQuery> GetStoredQueryAsync(string name)
        {
            return GetStoredQueryAsyncImpl(MakeStoredQueryKey(name)).AsAsyncOperation();
        }

        public IAsyncAction PutStoredQueryAsync(string name, StoredQuery query)
        {
            if (query == null)
            {
                throw new ArgumentNullException("value");
            }

            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          using (await CrossThreadLockScope.Enter(m_metadataLock))
                          {
                              await m_metadataStore.PutAsync(MakeStoredQueryKey(name), query);
                          }
                      });
        }

        public IAsyncAction DeleteStoredQueryAsync(string name)
        {
            return AsyncInfo.Run(
                async cancelToken =>
                      {
                          using (await CrossThreadLockScope.Enter(m_metadataLock))
                          {
                              await m_metadataStore.DeleteAsync(MakeStoredQueryKey(name));
                          }
                      });
        }

        internal async Task<SynchronizedView> GetViewAsync(string viewName, CancellationToken cancelToken)
        {
            using (await CrossThreadLockScope.Enter(m_metadataLock))
            {
                var viewData = (ViewData) await m_metadataStore.Store.GetAsync(MakeViewKey(viewName), typeof (ViewData));
                if (viewData == null)
                {
                    return null;
                }
                if (!string.Equals(viewData.Name, viewName))
                {
                    return null;
                }

                return new SynchronizedView(m_dataStore, viewData);
            }
        }

        internal async Task PutViewAsync(SynchronizedView view, CancellationToken cancelToken)
        {
            using (await CrossThreadLockScope.Enter(m_metadataLock))
            {
                await m_metadataStore.PutAsync(MakeViewKey(view.Name), view.Data);
            }
        }

        internal async Task<StoredQuery> GetStoredQueryAsyncImpl(string queryKey)
        {
            using (await CrossThreadLockScope.Enter(m_metadataLock))
            {
                return (StoredQuery) await m_metadataStore.Store.GetAsync(queryKey, typeof (StoredQuery));
            }
        }

        
        internal async Task EnsureStores(IObjectStore parentStore, LocalRecordStoreTable recordStoreTable)
        {
            m_root = await parentStore.CreateChildStoreAsync(m_record.ID);

            IObjectStore child;

            child = await m_root.CreateChildStoreAsync("Data");
            LocalItemStore itemStore = new LocalItemStore(child, (recordStoreTable != null) ? recordStoreTable.ItemCache : null);

            child = await m_root.CreateChildStoreAsync("Changes");
            RecordItemChangeTable changeTable = new RecordItemChangeTable(child, null);

            m_dataStore = new SynchronizedStore(m_record, itemStore, changeTable);

            child = await m_root.CreateChildStoreAsync("Metadata");
            m_metadataStore = new LocalStore(child);

            child = await m_root.CreateChildStoreAsync("Blobs");
            m_blobs = new LocalStore(child);

            m_synchronizedTypes = new SynchronizedTypeManager(this);
        }

        private string MakeViewKey(string name)
        {
            return name + "_View";
        }

        private string MakeStoredQueryKey(string name)
        {
            return name + "_StoredQuery";
        }
    }
}