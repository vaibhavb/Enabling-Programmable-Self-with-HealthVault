using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Diagnostics;
using HealthVault.Foundation;
using HealthVault.ItemTypes;
using HealthVault.Types;
using Windows.Foundation;

namespace HealthVault.Store
{
    /// <summary>
    /// The Change Manager:
    ///     - Tracks all changes made to a local SynchronizedStore
    ///     - When asked, commits changes from SynchronizedStore.Local TO SynchronizedStore.Remote
    ///   
    /// To commit pending changes, somebody must call CommitAsync*. 
    /// 
    /// You typically don't need to do this yourself, as a higher level object will do the needful. 
    /// 
    ///     - A SynchronizedType will attempt to immediately commit changes 
    ///     - To handle left over changes (such as those made during a network outage), you can enable 
    ///       localVault.RecordStores.BackgroundCommitScheduler (disabled by default). OR you can do your
    ///       own thing and call Commit when you see fit.. by calling localVault.RecordStores.CommitChangesAsync
    ///     
    ///     - Any pending changes, such as those collected when you were offline - will get committed
    ///     
    /// Fires events as changes are committed. 
    /// </summary>
    public sealed class RecordItemChangeManager
    {
        private SynchronizedStore m_store;
        private RecordItemChangeTable m_changeTable;
        private RecordItemCommitErrorHandler m_errorHandler;
        private WorkerController m_workerController;

        public RecordItemChangeManager(SynchronizedStore store, RecordItemChangeTable changeTable)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }
            if (changeTable == null)
            {
                throw new ArgumentNullException("changeTable");
            }
            m_store = store;
            m_changeTable = changeTable;
            m_errorHandler = new RecordItemCommitErrorHandler();
            m_workerController = new WorkerController();
        }

        /// <summary>
        /// Known changes
        /// </summary>
        public RecordItemChangeTable ChangeTable
        {
            get { return m_changeTable;}
        }
        
        // Enable/Disable commits...
        public bool IsCommitEnabled
        {
            get { return m_workerController.IsEnabled;}
            set { m_workerController.IsEnabled = value;}
        }

        public RecordItemCommitErrorHandler ErrorHandler
        {
            get { return m_errorHandler;}
        }
        
        internal SynchronizedTypeManager SynchronizedTypes
        {
            get; set;
        }
        
        /*
          Status Events. These are NOT invoked in the UI thread
         */
        public event EventHandler<RecordItemChangeManager> Starting;
        public event EventHandler<RecordItemChangeManager> Finished;
        public event EventHandler<RecordItemChange> CommitSuccess;
        public event EventHandler<RecordItemChange> CommitFailed;
        public event EventHandler<Exception> Error;
        
        /// <summary>
        /// Are there pending changes?
        /// </summary>
        public IAsyncOperation<bool> HasChangesAsync()
        {
            return m_changeTable.HasChangesAsync();
        }
        
        /// <summary>
        /// Are there pending changes for a given type?
        /// </summary>
        public IAsyncOperation<bool> HasChangesForTypeAsync(string typeID)
        {
            return m_changeTable.HasChangesForTypeAsync(typeID);
        }
        
        /// <summary>
        /// Are there pending changes for an item? 
        /// </summary>
        public IAsyncOperation<bool> HasChangesForItemAsync(string itemID)
        {
            return m_changeTable.HasChangesForItemAsync(itemID);
        }
        
        /// <summary>
        /// Get a count of changes
        /// </summary>
        public IAsyncOperation<int> GetChangeCountAsync()
        {
            return m_changeTable.GetChangeCountAsync();
        }
        
        /// <summary>
        /// Starts committing and and returns
        /// </summary>
        public void StartCommit()
        {
            if (m_workerController.ShouldScheduleWork())
            {
                Task.Run(() => this.CommitAsync());
            }
        }

        /// <summary>
        /// Starts committing a batch of pending changes. 
        /// </summary>
        /// <returns></returns>
        public IAsyncAction CommitAsync()
        {
            return AsyncInfo.Run(async cancelToken => {
                await this.CommitAsync(cancelToken);
            });
        }

        internal async Task CommitAsync(CancellationToken cancelToken)
        {
            bool hasPendingWork = true;
            while(hasPendingWork)
            {                
                if (!m_workerController.BeginWork())
                {
                    return;
                }
                                    
                hasPendingWork = false;
                this.NotifyStarting();
                try
                {
                    IList<string> changedItems = await m_changeTable.GetChangeQueueAsync();
                    if (!changedItems.IsNullOrEmpty())
                    {
                        await this.CommitChangesAsync(changedItems);
                    }
                }
                catch(Exception ex)
                {
                    this.NotifyError(ex);
                    throw;
                }
                finally
                {
                    this.NotifyFinished();
                    hasPendingWork = m_workerController.CompleteWork();
                }
            }
        }

        // SynchronizedStore calls this to track "Put"s
        internal async Task TrackPutAsync(RecordItem item)
        {
            this.ValidateItem(item);
            await m_changeTable.TrackChangeAsync(item.Type.ID, item.Key, RecordItemChangeType.Put);
        }
        
        // SynchronizedStore calls this to track "Removes"
        internal async Task TrackRemoveAsync(string typeID, ItemKey key)
        {
            await m_changeTable.TrackChangeAsync(typeID, key, RecordItemChangeType.Remove);
        }
        
        /// <summary>
        /// Commit the given change batch. 
        /// 
        /// For simplicity and robustness, the current implementation commits each each change individually. 
        /// 
        /// This should be fine as the # of pending changes at any time should be moderate.
        /// And and most changes will be committed as they are made - ONLINE. In fact, the majority of batches will contain
        /// exactly 1 item.
        ///
        /// If necessary, we can optimize this behavior later by reducing round trips:
        ///     - Dupe Detection in batches
        ///     - New Items in a batch
        ///  However, since a single bad egg can ruin the entire batch, the error handling will be more complex. 
        /// 
        /// We can also consider introducing new platform methods that can essentially do all of this in a single call. 
        /// 
        /// </summary>
        async Task CommitChangesAsync(IList<string> changedItems)
        {
            foreach (string itemID in changedItems)
            {
                if (!this.IsCommitEnabled)
                {
                    break;
                }

                long itemLockID = 0;
                if ((itemLockID = this.AcquireItemLock(itemID)) == 0)
                {
                    // Item is being edited.. we'll wait until we try to commit the next batch
                    continue;
                }

                try
                {
                    bool shouldContinue = true;

                    RecordItemChange change = await m_changeTable.GetChangeForItemAsync(itemID);
                    if (change != null)
                    {
                        shouldContinue = await this.CommitChange(change, itemLockID);
                    }
                    if (!shouldContinue) // Was there a halting error? 
                    {
                        return;
                    }
                }
                finally
                {
                    this.ReleaseItemLock(itemID, itemLockID);
                }
            }
        }

        // Assumes that you've acquire the item lock earlier! 
        // Critical that the item lock be acquired
        async Task<bool> CommitChange(RecordItemChange change, long itemLockID)
        { 
            bool dequeue = false;
            try
            {
                await this.UpdateChangeAttemptCount(change);

                if (change.ChangeType == RecordItemChangeType.Remove)
                {
                    await this.CommitRemoveAsync(change, itemLockID);
                }
                else
                {
                    if (await this.CommitPutAsync(change, itemLockID))
                    {
                        await this.NotifySynchronizedTypesAsync(change);
                    }
                }
                
                this.NotifyCommitSuccess(change);

                dequeue = true;
            }
            catch (Exception ex)
            {
                if (m_errorHandler.IsHaltingError(ex))
                {
                    this.NotifyError(ex);
                    return false; // Stop processing items right away because of major Halting errors, such as Network
                }

                if (m_errorHandler.ShouldRetryCommit(change, ex))
                {
                    // Leave the change in the commit queue. try again later
                    this.NotifyError(ex);
                }
                else
                {
                    this.NotifyCommitFailed(change);
                    dequeue = true;
                }
            }

            if (dequeue)
            {
                await m_changeTable.RemoveChangeAsync(change.ItemID);
            }

            return true;
        }

        async Task<bool> CommitPutAsync(RecordItemChange change, long itemLockID)
        {
            IItemDataTyped item = await m_store.Local.GetByIDAsync(change.ItemID);
            if (item == null)
            {
                return false;
            }

            change.LocalData = item;
            if (item.Key.IsLocal)
            {
                await this.CommitNewAsync(change);
            }
            else
            {
                await this.CommitUpdateAsync(change);
            }
            //
            // Refetch the item from HealthVault, to get updated dates etc...
            //
            await this.RefreshItemAsync(change, itemLockID);

            return true;
        }

        async Task CommitUpdateAsync(RecordItemChange change)
        {
            Debug.Assert(change.HasLocalData);

            if (await this.DetectDuplicateAsync(change))
            {
                // Already applied this change
                return;
            }                

            try
            {
                change.UpdatedKey = await m_store.RemoteStore.PutAsync(change.LocalData);
                return;
            }
            catch (Exception ex)
            {
                if (!m_errorHandler.ShouldCreateNewItemForConflict(change, ex))
                {
                    //
                    // Let the calling function decide how to handle all other scenarios
                    //
                    throw;
                }
            }
            //
            // Conflict resolution is simple. We don't want to lose data. If the data on the server changed from underneath us,
            // we will write this item as a NEW one, since the user did actually take the trouble to create the entry
            //
            await this.CommitNewAsync(change);
        }

        async Task CommitNewAsync(RecordItemChange change)
        {
            Debug.Assert(change.HasLocalData);
            change.UpdatedKey = await m_store.RemoteStore.NewAsync(change.LocalData);
        }
        
        async Task CommitRemoveAsync(RecordItemChange change, long itemLockID)
        {
            try
            {
                if (change.Key.IsLocal)
                {
                    return;
                }
                await m_store.RemoteStore.RemoveItemAsync(change.Key);
            }
            catch(ServerException se)
            {
                if (!m_errorHandler.IsItemKeyNotFound(se))
                {
                    throw;
                }
            }
        }

        async Task RefreshItemAsync(RecordItemChange change, long lockID)
        {
            change.UpdatedItem = null;
            try
            {
                change.UpdatedItem = await m_store.RemoteStore.GetItemAsync(change.UpdatedKey, m_store.SectionsToFetch);
            }
            catch(Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        async Task<bool> DetectDuplicateAsync(RecordItemChange change)
        {            
            IList<RecordItem> results = await this.LookupCommitsInRemoteStoreAsync(change.ChangeID);
            if (results.IsNullOrEmpty())
            {
                return false;
            }

            change.UpdatedKey = results[0].Key;
            return true;
        }

        async Task<IList<RecordItem>> LookupCommitsInRemoteStoreAsync(params string[] changeIDs)
        {
            ItemQuery query = new ItemQuery();
            query.View.SetSections(ItemSectionType.Core);
            query.MaxFullItems = new NonNegativeInt(0);
            query.MaxResults = new NonNegativeInt(changeIDs.Length);
            query.ClientIDs = changeIDs.ToArray();

            return await m_store.RemoteStore.GetAllItemsAsync(query);
        }

        async Task UpdateChangeAttemptCount(RecordItemChange change)
        {
            try
            {
                change.Attempt++;
                await m_changeTable.SaveChangeAsync(change);
            }
            catch(Exception ex)
            {
                this.NotifyError(ex);
            }
        }

        async Task NotifySynchronizedTypesAsync(RecordItemChange change)
        {
            try
            {
                if (this.SynchronizedTypes != null)
                {
                    await this.SynchronizedTypes.OnChangeCommittedAsync(change);
                }
            }
            catch
            {
            }
        }

        long AcquireItemLock(string itemID)
        {
            return m_store.Locks.AcquireLock(itemID);
        }

        void ReleaseItemLock(string itemID, long lockID)
        {
            m_store.Locks.SafeReleaseLock(itemID, lockID);
        }
        
        void ValidateItem(RecordItem item)
        {
            item.Type.ValidateRequired("Type");
            item.Key.ValidateRequired("Key");
        }

        void NotifyError(Exception ex)
        {
            Debug.WriteLine("Change commit error {0}", ex.Message);
            this.Error.SafeInvokeInUIThread(this, ex);
        }

        void NotifyStarting()
        {
            Debug.WriteLine("Starting change commit");
            this.Starting.SafeInvokeInUIThread(this, this);
        }

        void NotifyFinished()
        {
            Debug.WriteLine("Finished change commit");
            this.Finished.SafeInvokeInUIThread(this, this);
        }

        void NotifyCommitSuccess(RecordItemChange change)
        {
            Debug.WriteLine("Change Committed {0}", change);
            this.CommitSuccess.SafeInvokeInUIThread(this, change);
        }

        void NotifyCommitFailed(RecordItemChange change)
        {
            Debug.WriteLine("Change Commit failed {0}", change);
            this.CommitFailed.SafeInvokeInUIThread(this, change);
        }
    }

    internal class WorkerController
    {
        object m_lock;
        bool m_isBusy = false;
        bool m_hasPendingWork = false;
        bool m_isEnabled = true;

        internal WorkerController()
        {
            m_lock = new object();
        }

        public bool IsEnabled
        {
            get { return m_isEnabled;}
            set { m_isEnabled = value;}
        }

        public bool IsBusy
        {
            get { return m_isBusy; }
        }

        public bool HasPendingWork
        {
            get { return m_hasPendingWork; }
        }

        public bool BeginWork()
        {
            lock (m_lock)
            {
                if (m_isBusy || !this.IsEnabled)
                {
                    m_hasPendingWork = true;
                    return false;
                }

                m_isBusy = true;
            }

            return true;
        }

        internal bool CompleteWork()
        {
            lock (m_lock)
            {
                m_isBusy = false;
                bool isPending = m_hasPendingWork;
                m_hasPendingWork = false;
                return isPending;
            }
        }

        internal bool ShouldScheduleWork()
        {
            lock(m_lock)
            {
                if (m_isBusy || !this.IsEnabled)
                {
                    m_hasPendingWork = true;
                    return false;
                }

                return true;
            }
        }
    }
}
