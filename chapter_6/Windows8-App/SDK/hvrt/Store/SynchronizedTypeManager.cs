using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.Store
{
    /// <summary>
    /// Maintains a SynchronizedType instance for each HealthVault data type that the application decides to work with
    ///  - Objects are created opportunistically -- i.e. only when the application asks for them
    ///  - There is always ONE and only ONE SynchronizedType object per data type. 
    ///  - Each SynchronizedType object is backed by a persisted object on physical storage
    ///  - The SDK keeps the keys in this SynchronizedType to date (as background commits succeed and keys are updated, etc. )
    ///  
    ///  - The SynchronizedType manager uses WeakReferences to manage memory efficiently and purge objects no longer in use. 
    /// </summary>
    public sealed class SynchronizedTypeManager
    {
        CrossThreadLock m_lock;
        LocalRecordStore m_recordStore;
        Dictionary<string, WeakReference<SynchronizedType>> m_views;

        internal SynchronizedTypeManager(LocalRecordStore recordStore)
        {
            if (recordStore == null)
            {
                throw new ArgumentNullException("recordStore");
            }
            
            m_lock = new CrossThreadLock(false);
            m_recordStore = recordStore;
            m_views = new Dictionary<string,WeakReference<SynchronizedType>>();

            m_recordStore.Data.Changes.SynchronizedTypes = this;
            this.ImmediateCommitEnabled = true;
        }

        internal LocalRecordStore RecordStore
        {
            get { return m_recordStore;}
        }

        public bool ImmediateCommitEnabled
        {
            get; set;
        }
        //
        // Fires an event with the typeID of the Type that was updated
        // 
        public event EventHandler<string> TypeUpdated;
        
        public IAsyncOperation<SynchronizedType> GetAsync(string typeID)
        {
            return AsyncInfo.Run(async cancelToken =>
            {
                return await this.Ensure(typeID, cancelToken);
            });
        }    
        
        public IAsyncOperation<IList<SynchronizedType>> GetMultipleAsync(IList<string> typeIDs)
        {
            if (typeIDs.IsNullOrEmpty())
            {
                throw new ArgumentException("typeIDs");
            }

            return AsyncInfo.Run<IList<SynchronizedType>>(async cancelToken =>
            {
                List<SynchronizedType> sTypes = new List<SynchronizedType>();
                foreach(string typeID in typeIDs)
                {
                    SynchronizedType type = await this.Ensure(typeID, cancelToken);
                    sTypes.Add(type);
                }

                return sTypes;
            });
        }
        
        /// <summary>
        /// Synchronize multiple types with a single roundtrip
        /// </summary>
        /// <param name="typeIDs">TypeIds to sync</param>
        /// <param name="maxAgeInSeconds">View age</param>
        /// <returns>A list of type ids actually synchronized</returns>
        public IAsyncOperation<IList<string>> SynchronizeTypesAsync(IList<string> typeIDs, int maxAgeInSeconds)
        {
            if (typeIDs.IsNullOrEmpty())
            {
                throw new ArgumentException("typeIDs");
            }
            if (maxAgeInSeconds < 0)
            {
                throw new ArgumentException("maxAgeInSeconds");
            }

            return AsyncInfo.Run<IList<string>>(async cancelToken =>
            {
                IList<SynchronizedType> sTypes = await this.GetMultipleAsync(typeIDs);
                SynchronizedViewSynchronizer synchronizer = new SynchronizedViewSynchronizer(m_recordStore.Record, maxAgeInSeconds);
                synchronizer.MaxAgeInSeconds = maxAgeInSeconds;

                IList<ISynchronizedView> synchronized = await synchronizer.SynchronizeAsync(sTypes.Cast<ISynchronizedView>().ToList());
                if (synchronized.IsNullOrEmpty())
                {
                    return null;
                }
                
                string[] syncedIDs = (from view in synchronized
                        select ((SynchronizedType) view).TypeID).ToArray();

                return syncedIDs;
            });
        }

        async Task<SynchronizedType> Ensure(string typeID, CancellationToken cancelToken)
        {
            using(await CrossThreadLockScope.Enter(m_lock))
            {
                WeakReference<SynchronizedType> viewRef = null;
                SynchronizedType view = null;
                if (m_views.TryGetValue(typeID, out viewRef))
                {
                    viewRef.TryGetTarget(out view);
                }
                if (view != null)
                {
                    return view;
                }

                view = new SynchronizedType(this, typeID);
                await view.Load();
                if (viewRef == null)
                {
                    viewRef = new WeakReference<SynchronizedType>(view);
                }
                else
                {
                    viewRef.SetTarget(view);
                }
                m_views[typeID] = viewRef;
                return view;
            }    
        }

        internal void StartCommitChanges()
        {
            if (this.ImmediateCommitEnabled)
            {
                m_recordStore.Data.Changes.StartCommit();
            }
        }

        internal async Task OnChangeCommittedAsync(RecordItemChange change)
        {
            if (change.ChangeType != RecordItemChangeType.Put)
            {
                return;
            }          
              
            try
            {
                SynchronizedType sType = await this.GetAsync(change.TypeID);

                await sType.OnPutCommittedAsync(change);
                
                this.TypeUpdated.SafeInvokeInUIThread(this, change.TypeID);
            }
            catch
            {
            }
        }
    }
}
