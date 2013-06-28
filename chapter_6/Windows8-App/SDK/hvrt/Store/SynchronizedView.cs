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
using DateTime = HealthVault.Types.DateTime;

namespace HealthVault.Store
{
    public sealed class SynchronizedView : ISynchronizedView
    {
        internal const int DefaultReadAheadChunkSize = 100;        

        private readonly ViewData m_data;
        private readonly SynchronizedStore m_store;
        private int m_readAheadChunkSize = DefaultReadAheadChunkSize;

        public SynchronizedView(SynchronizedStore store, ViewData data)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (data == null)
            {
                throw new ArgumentNullException("data");
            }

            m_store = store;
            m_data = data;
            this.ReadAheadMode = SynchronizedViewReadAheadMode.Page;
        }

        public SynchronizedView(SynchronizedStore store, ItemQuery query, string name)
            : this(store, new ViewData(query, name))
        {
        }

        /// <summary>
        /// Record over which this is a view
        /// </summary>
        public IRecord Record
        {
            get { return m_store.Record; }
        }

        /// <summary>
        /// Local synchronized store this view is working with
        /// </summary>
        public SynchronizedStore Store
        {
            get { return m_store; }
        }

        public string Name
        {
            get { return m_data.Name; }
            set { m_data.Name = value; }
        }

        public int KeyCount
        {
            get { return m_data.KeyCount; }
        }

        public ViewKeyCollection Keys
        {
            get { return m_data.Keys;}
        }

        public DateTime LastUpdated 
        { 
            get { return m_data.LastUpdated;}
            set { m_data.LastUpdated = value;}
        }

        /// <summary>
        /// This view's data!
        /// </summary>
        public ViewData Data
        {
            get { return m_data; }
        }

        /// <summary>
        /// The server currently limits max "Full" items to 250. 
        /// Setting the chunkSize larger than 250 will not help
        /// </summary>
        public int ReadAheadChunkSize
        {
            get { return m_readAheadChunkSize; }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentException("ReadAheadChunkSize");
                }
                m_readAheadChunkSize = value;
            }
        }

        public SynchronizedViewReadAheadMode ReadAheadMode
        {
            get; set;
        }

        internal bool PublicSyncDisabled
        {
            get; set;
        }

        /// <summary>
        /// The keys for which items are now available
        /// </summary>
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
            return (!m_data.HasKeys || m_data.IsStale(maxAgeInSeconds));
        }

        public IAsyncOperation<bool> SynchronizeAsync()
        {
            if (this.PublicSyncDisabled)
            {
                throw new NotSupportedException();
            }

            return AsyncInfo.Run(async cancelToken => 
            { 
                await SynchronizeAsyncImpl(cancelToken); 
                return true;
            });
        }

        public ItemQuery GetSynchronizationQuery()
        {
            ItemQuery query = m_data.Query;
            int maxResults =  (query.MaxResults) != null ? m_data.Query.MaxResults.Value : 0;
            return ItemQuery.QueryForKeys(query.Filters, maxResults);
        }

        public IList<string> GetTypeVersions()
        {
            return m_data.TypeVersions;
        }

        public void UpdateKeys(ViewKeyCollection keys)
        {
            if (keys == null)
            {
                throw new ArgumentNullException("keys");
            }

            m_data.Keys = keys;
            m_data.LastUpdated = DateTime.Now();
        }

        public ItemKey KeyAtIndex(int index)
        {
            return m_data.Keys[index].Key;
        }

        public int IndexOfItemKey(ItemKey key)
        {
            return m_data.Keys.IndexOfItemKey(key);
        }

        /// <summary>
        /// 1. If the item for the key at index is available in the local cache, returns the item
        /// 2. If not, OR the item was stale (version stamp mismatch), returns NULL
        /// 3. If returns NULL, then also triggers a refresh (with readahead) in the background
        /// 4. Does NOT await the background refresh
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IAsyncOperation<IItemDataTyped> GetItemAsync(int index)
        {
            return AsyncInfo.Run(async cancelToken => 
            { 
                return await GetItemAsync(index, false, cancelToken); 
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
                return await this.GetItemByKeyAsync(key, false, cancelToken);
            });
        }

        public IAsyncOperation<IList<IItemDataTyped>> GetItemsAsync(int startAt, int count)
        {
            return AsyncInfo.Run<IList<IItemDataTyped>>(async cancelToken =>
            {
                return await this.GetItemsAsync(startAt, count, false, cancelToken);
            });
        }

        /// <summary>
        /// Call this WARILY. Will block
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IItemDataTyped GetItemSync(int index)
        {
            Task<IItemDataTyped> task = Task.Run(() => GetItemAsync(index, false, CancellationToken.None));
            task.Wait();

            return task.Result;
        }
        
        public IAsyncOperation<IItemDataTyped> GetLocalItemAsync(int index)
        {
            ViewKey viewKey = m_data.Keys[index];
            return this.GetLocalItemByKeyAsync(viewKey.Key);
        }

        public IAsyncOperation<IItemDataTyped> GetLocalItemByKeyAsync(ItemKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            return AsyncInfo.Run(async cancelToken =>
            {
                return await this.LoadLocalItemAsync(key);
            }); 
        }

        // Returns null if no matches
        public IAsyncOperation<IList<ItemKey>> GetKeysForItemsNeedingDownload(int startAt, int count)
        {
            return AsyncInfo.Run<IList<ItemKey>>(async cancelToken =>
            {
                IList<ItemKey> keys = this.Keys.SelectItemKeys(startAt, count);
                LazyList<ItemKey> candidates = new LazyList<ItemKey>();

                foreach(ItemKey key in keys)
                {
                    if (await this.LoadLocalItemAsync(key) == null)
                    {
                        candidates.Add(key);
                    }
                }

                return candidates.HasValue ? candidates.Value : null;
            });
        }


        /// <summary>
        /// 1. If the item for the key at index is available in the local cache, returns the item
        /// 2. If not, OR the item was stale (version stamp mismatch), triggers a refresh (with readahead)
        /// 3. AWAITS the refresh completion
        /// 4. If the item was not found, returns NULL
        /// 5. If you called this method and did NOT await its result before calling it again, MAY return NULL if the 
        /// item you requested already has a load pending
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IAsyncOperation<IItemDataTyped> EnsureItemAvailableAndGetAsync(int index)
        {
            return AsyncInfo.Run(async cancelToken => 
            { 
                return await GetItemAsync(index, true, cancelToken); 
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
                return await this.GetItemByKeyAsync(key, true, cancelToken);
            });
        }

        public IAsyncOperation<IList<IItemDataTyped>> EnsureItemsAvailableAndGetAsync(int startAt, int count)
        {
            return AsyncInfo.Run<IList<IItemDataTyped>>(async cancelToken =>
            {
                return await this.GetItemsAsync(startAt, count, true, cancelToken);
            });
        }

        /// <summary>
        /// Call this warily. Will block.
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IItemDataTyped EnsureItemAvailableAndGetSync(int index)
        {
            Task<IItemDataTyped> task = Task.Run(() => GetItemAsync(index, true, CancellationToken.None));
            task.Wait();

            return task.Result;
        }

        // PredicateDelegate is passed IItemDataTyped objects
        public IAsyncOperation<IList<IItemDataTyped>> SelectAsync(PredicateDelegate predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return AsyncInfo.Run(async cancelToken =>
            {
                return await this.SelectAsync(predicate, cancelToken);
            });
        }

        // PredicateDelegate is passed IItemDataTyped objects
        public IAsyncOperation<IList<ItemKey>> SelectKeysAsync(PredicateDelegate predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            return AsyncInfo.Run(async cancelToken =>
            {
                return await this.SelectKeysAsync(predicate, cancelToken);
            });
        }
                        
        internal async Task<IList<IItemDataTyped>> EnsureItemsAvailableAndGetAsync(int startAt, int count, CancellationToken cancelToken)
        {
            if (!m_data.HasKeys)
            {
                return null;
            }

            m_data.ValidateIndex(startAt);
            count = m_data.Keys.GetCorrectedCount(startAt, count);

            var items = new LazyList<IItemDataTyped>();
            for (int i = startAt, max = startAt + count; i < max; ++i)
            {
                IItemDataTyped item = await EnsureItemAvailableAndGetAsync(i).AsTask(cancelToken);
                items.Add(item);
            }

            return items.HasValue ? items.Value : null;
        }

        internal async Task<IList<IItemDataTyped>> GetItemsAsync(int startAt, int count, bool shouldAwaitRefresh, CancellationToken cancelToken)
        {
            if (!m_data.HasKeys)
            {
                return null;
            }

            m_data.ValidateIndex(startAt);
            count = m_data.Keys.GetCorrectedCount(startAt, count);

            var items = new LazyList<IItemDataTyped>();
            for (int i = startAt, max = startAt + count; i < max; ++i)
            {
                IItemDataTyped item = await GetItemAsync(i, shouldAwaitRefresh, cancelToken);
                items.Add(item);
            }

            return items.HasValue ? items.Value : null;
        }

        internal async Task<IItemDataTyped> GetItemByKeyAsync(ItemKey key, bool shouldAwaitRefresh, CancellationToken cancelToken)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }
            int index = m_data.Keys.IndexOfItemKey(key);
            if (index < 0)
            {
                return null;
            }

            return await this.GetItemAsync(index, shouldAwaitRefresh, cancelToken);
        }

        internal async Task<IItemDataTyped> GetItemAsync(int index, bool shouldAwaitRefresh, CancellationToken cancelToken)
        {
            if (!m_data.HasKeys)
            {
                return null;
            }

            m_data.ValidateIndex(index);

            ViewKey viewKey = m_data.Keys[index];
            //
            // Try to load the item from the local store
            //
            IItemDataTyped item = await this.LoadLocalItemAsync(viewKey.Key);
            if (item != null)
            {
                return item;
            }

            //
            // Don't have the item locally available. Will need to fetch it. 
            // While we do this, might as well read ahead
            //
            int startAt;
            if (this.ReadAheadMode == SynchronizedViewReadAheadMode.Page)
            {
                startAt = this.GetStartAtPositionForPage(index, this.ReadAheadChunkSize);
            } 
            else
            {
                startAt = index;
            }

            await BeginRefreshAsync(startAt, shouldAwaitRefresh, cancelToken);

            if (!shouldAwaitRefresh)
            {
                return null;
            }
            //
            // Reload the item
            //
            return await Store.Local.GetAsyncImpl(viewKey.Key);
        }

        internal async Task<IList<IItemDataTyped>> SelectAsync(PredicateDelegate predicate, CancellationToken cancelToken)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            List<IItemDataTyped> matches = new List<IItemDataTyped>();
            for (int i = 0, max = this.KeyCount; i < max; ++i)
            {
                IItemDataTyped item = await this.GetItemAsync(i, true, cancelToken);
                if (item != null && predicate(item))
                {
                    matches.Add(item);
                }
            }

            return matches;
        }

        internal async Task<IList<ItemKey>> SelectKeysAsync(PredicateDelegate predicate, CancellationToken cancelToken)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException("predicate");
            }
            List<ItemKey> matches = new List<ItemKey>();
            for (int i = 0, max = this.KeyCount; i < max; ++i)
            {
                IItemDataTyped item = await this.GetItemAsync(i, true, cancelToken);
                if (item != null && predicate(item))
                {
                    matches.Add(item.Key);
                }
            }

            return matches;
        }
                
        private async Task BeginRefreshAsync(int startAt, bool shouldAwait, CancellationToken cancelToken)
        {
            IList<ItemKey> keysToDownload = m_data.Keys.CollectKeysNeedingDownload(startAt, ReadAheadChunkSize);
            if (keysToDownload.IsNullOrEmpty())
            {
                return;
            }
            //
            // Refresh happens in the background
            // This will return as soon as the task is launched
            //
            PendingGetCompletionDelegate completionCallback = null;
            if (!shouldAwait)
            {
                completionCallback = PendingGetCompletion; // Callback => download items in background
            }

            PendingGetResult result = await Store.RefreshAsyncImpl(
                keysToDownload, 
                m_data.TypeVersions,
                completionCallback, 
                cancelToken);
            if (result == null)
            {
                return; // NO pending work
            }

            PendingGetCompletion(this, result);
        }

        /// <summary>
        /// Returns the start index of the page the given item index is in
        /// </summary>
        int GetStartAtPositionForPage(int positionInPage, int pageSize)
        {
            int page = positionInPage / pageSize;
            return (page * pageSize);
        }

        private void PendingGetCompletion(object sender, PendingGetResult result)
        {
            try
            {
                result.EnsureSuccess();

                ProcessFoundItems(result, true);

                ProcessNotFoundItems(result);
            }
            catch (Exception ex)
            {
                ProcessError(result, ex);
            }
        }

        private void ProcessFoundItems(PendingGetResult pendingResult, bool fireEvent)
        {
            if (pendingResult.KeysFound.IsNullOrEmpty() || !m_data.HasKeys)
            {
                return;
            }

            m_data.Keys.SetLoadingStateForKeys(pendingResult.KeysFound, false);

            if (fireEvent)
            {
                ItemsAvailable.SafeInvokeInUIThread(this, pendingResult.KeysFound);
            }
        }

        private void ProcessNotFoundItems(PendingGetResult pendingResult)
        {
            if (pendingResult.KeysNotFound.IsNullOrEmpty() || !m_data.HasKeys)
            {
                return;
            }

            m_data.Keys.SetLoadingStateForKeys(pendingResult.KeysNotFound, false);
            ItemsNotFound.SafeInvokeInUIThread(this, pendingResult.KeysNotFound);
        }

        private void ProcessError(PendingGetResult pendingResult, Exception ex)
        {
            try
            {
                m_data.Keys.SetLoadingStateForKeys(pendingResult.KeysRequested, false);
                Error.SafeInvokeInUIThread(this, ex);
            }
            catch
            {
            }
        }

        internal async Task SynchronizeAsyncImpl(CancellationToken cancelToken)
        {
            if (!m_data.HasQuery)
            {
                throw new ArgumentException("Query");
            }

            ItemQuery query = m_data.Query;
            int maxResults =  (query.MaxResults) != null ? m_data.Query.MaxResults.Value : 0;
            IList<PendingItem> pendingItems = await Record.GetKeysAndDateAsync(query.Filters, maxResults).AsTask(cancelToken);

            ViewKeyCollection newKeys = new ViewKeyCollection();
            if (!pendingItems.IsNullOrEmpty())
            {
                newKeys.AddFromPendingItems(pendingItems);
            }

            this.UpdateKeys(newKeys);
        }
        
        internal async Task<IItemDataTyped> LoadLocalItemAsync(ItemKey key)
        {
            IItemDataTyped item = await m_store.Local.GetAsyncImpl(key);
            if (item != null && m_data.TypeVersions.Contains(item.Type.ID))
            {
                return item;
            }

            return null;    
        }

        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }
    }
}