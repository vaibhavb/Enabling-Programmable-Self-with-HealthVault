using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.Store
{
    /// <summary>
    /// Use this class to Synchronize multiple views efficiently
    /// </summary>
    public sealed class SynchronizedViewSynchronizer
    {        
        IRecord m_record;
        int m_maxAgeInSeconds;
        List<ItemQuery> m_syncQueries;
        List<ItemQuery> m_queriesToRun;

        public SynchronizedViewSynchronizer(IRecord record, int maxAgeInSeconds)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            m_record = record;
            this.MaxAgeInSeconds = maxAgeInSeconds;
            m_syncQueries = new List<ItemQuery>();
            m_queriesToRun = new List<ItemQuery>();
        }
        
        public int MaxAgeInSeconds
        {
            get { return m_maxAgeInSeconds;}
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("MaxAgeInSeconds");
                }
                m_maxAgeInSeconds = value;
            }
        }
        
        /// <summary>
        /// 
        /// </summary>
        /// <param name="views">Views to synchronize</param>
        /// <returns>Subset of views acutually refreshed OR null if none</returns>
        public IAsyncOperation<IList<ISynchronizedView>> SynchronizeAsync(IList<ISynchronizedView> views)
        {
            if (views == null)
            {
                throw new ArgumentNullException("views");
            }

            return AsyncInfo.Run<IList<ISynchronizedView>>(async cancelToken => {                
                
                this.Reset();
                           
                try
                {  
                    IList<ISynchronizedView> synchronizableViews = await this.CollectSynchronizableViews(views);                       
                    if (!synchronizableViews.IsNullOrEmpty())
                    {
                        IList<ItemQueryResult> results = await this.ExecuteSyncQueries();
                        await this.UpdateViews(views, results);
                    }

                    return synchronizableViews;
                }
                finally
                {
                    this.Reset();
                }
            });
        }
        
        void Reset()
        {
            m_syncQueries.Clear();
            m_queriesToRun.Clear();
        }

        async Task UpdateViews(IList<ISynchronizedView> views, IList<ItemQueryResult> queryResults)
        {
            int iResult = 0;
            for (int i = 0, count = views.Count; i < count; ++i)
            {
                ItemQuery query = m_syncQueries[i];
                if (query != null)
                {
                    ItemQueryResult queryResult = null;
                    if (!queryResults.IsNullOrEmpty())
                    {
                        queryResult = queryResults[iResult++];
                    }

                    await this.UpdateKeys(views[i], queryResult);
                }                
            }
        }

        async Task<IList<ItemQueryResult>> ExecuteSyncQueries()
        {
            return await m_record.ExecuteQueriesAsync(m_queriesToRun);  
        }

        // Collect queries for those views that can be synchronized. Return the list of views
        async Task<IList<ISynchronizedView>> CollectSynchronizableViews(IList<ISynchronizedView> views)
        {
            LazyList<ISynchronizedView> synchronizableViews = new LazyList<ISynchronizedView>();
            foreach (ISynchronizedView view in views)
            {
                ItemQuery query = await this.GetSyncQuery(view);
                m_syncQueries.Add(query);
                if (query != null)
                {
                    m_queriesToRun.Add(query);
                    synchronizableViews.Add(view);
                }
            }

            return synchronizableViews.HasValue ? synchronizableViews.Value : null;
        }

        async Task<ItemQuery> GetSyncQuery(ISynchronizedView view)
        {
            if (!view.IsStale(m_maxAgeInSeconds))
            {
                return null;
            }

            SynchronizedType sType = view as SynchronizedType;
            if (sType != null && await sType.HasPendingChangesAsync())
            {
                return null;
            }

            return view.GetSynchronizationQuery();
        }

        async Task UpdateKeys(ISynchronizedView view, ItemQueryResult queryResult)
        {
            ViewKeyCollection keys = null;
            if (queryResult != null)
            {
                keys = ViewKeyCollection.FromQueryResult(queryResult);
            }
            else
            {
                keys = new ViewKeyCollection();
            }
            view.UpdateKeys(keys);

            SynchronizedType sType = view as SynchronizedType;
            if (sType != null)
            {
                await sType.SaveAsync();
            }
        }
    }
}
