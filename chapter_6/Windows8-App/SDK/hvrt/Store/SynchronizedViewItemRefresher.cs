using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Runtime.InteropServices.WindowsRuntime;
using HealthVault.Foundation;
using HealthVault.Types;
using HealthVault.ItemTypes;
using Windows.Foundation;

namespace HealthVault.Store
{
    /// <summary>
    /// Use this object to refresh/ensure items in multiple views simultaneously.
    /// The object will include ItemKeys from multiple views as efficiently as possible
    /// </summary>
    public sealed class SynchronizedViewItemRefresher
    {
        LocalRecordStore m_store;
        List<UpdateChunk> m_updateChunks;
        int m_maxBatchSize;
        IEnumerator<UpdateCandidate> m_updateCandidates;
        List<ViewKey> m_keyBatch;
        HashSet<string> m_typeVersions;
        IList<string> m_typeVersionsList;

        public SynchronizedViewItemRefresher(LocalRecordStore store)
        {
            if (store == null)
            {
                throw new ArgumentNullException("store");
            }

            m_store = store;
            m_updateChunks = new List<UpdateChunk>();
            m_maxBatchSize = SynchronizedView.DefaultReadAheadChunkSize;
            m_keyBatch = new List<ViewKey>();
            m_typeVersions = new HashSet<string>();
        }

        /// <summary>
        /// The server currently limits max "Full" items to 250. 
        /// Setting the chunkSize larger than 250 will not help
        /// </summary>
        public int BatchSize
        {
            get { return m_maxBatchSize;}
            set 
            {
                if (value <= 0)
                {
                    throw new ArgumentException("BatchSize");
                }
                m_maxBatchSize = value;
            }
        }
        
        public void Reset()
        {
            m_updateChunks.Clear();
            m_typeVersions.Clear();
            m_updateCandidates = null;
            m_typeVersionsList = null;
        }

        public void AddChunk(ISynchronizedView view, int startAt, int count)
        {
            if (view == null || view.Record != m_store.Record)
            {
                throw new ArgumentException("view");
            }

            IList<ViewKey> keys = view.Keys.CollectViewKeysNeedingDownload(startAt, count);
            m_updateChunks.Add(new UpdateChunk { View = view, Keys = keys });
            m_typeVersions.AddRange(view.GetTypeVersions());
        }

        // Refresh the registered batches and return the count of keys for which we actually tried to download updated data
        public IAsyncOperation<int> RefreshAsync()
        {
            return AsyncInfo.Run(async cancelToken => {            
                try
                {
                    m_typeVersionsList = m_typeVersions.ToList();

                    this.CollectCandidatesForUpdate();    

                    int downloadCount = 0;
                    while (await this.CollectNextUpdateBatchAsync())
                    {
                        downloadCount += await this.DownloadNextBatchAsync(cancelToken);
                    }

                    return downloadCount;
                }
                finally
                {
                    this.Reset();
                }
            });
        }
        
        async Task<bool> CollectNextUpdateBatchAsync()
        {
            m_keyBatch.Clear();
            try
            {
                while (m_updateCandidates.MoveNext())
                {
                    UpdateCandidate candidate = m_updateCandidates.Current;
                    IItemDataTyped item = await candidate.Chunk.View.GetLocalItemByKeyAsync(candidate.ViewKey.Key);
                    if (item == null)
                    {
                        m_keyBatch.Add(candidate.ViewKey);
                    }
                    else
                    {
                        candidate.ViewKey.IsLoadPending = false;
                    }

                    if (m_keyBatch.Count >= m_maxBatchSize)
                    {
                        break;
                    }
                }
            
                return (m_keyBatch.Count > 0);
            }
            catch
            {
                this.ClearUpdateStatusForBatch();
                throw;
            }
        }

        // Return the key count for which we tried a download
        async Task<int> DownloadNextBatchAsync(CancellationToken cancelToken)
        {
            var keys = from key in m_keyBatch
                       select key.Key;
            try
            {
                IList<ItemKey> keyList = keys.ToList();
                await m_store.Data.DownloadAsyncImpl(keyList, m_typeVersionsList, null, cancelToken);
                return keyList.Count;
            }
            finally
            {
                this.ClearUpdateStatusForBatch();
            }
        }

        void CollectCandidatesForUpdate()
        {
            var candidates = from chunk in m_updateChunks
                   from candidate in chunk.GetCandidatesForUpdate()
                   select candidate;
            m_updateCandidates = candidates.GetEnumerator();
        }

        void ClearUpdateStatusForBatch()
        {
            foreach(ViewKey key in m_keyBatch)
            {
                key.IsLoadPending = false;
            }
        }

        internal class UpdateChunk
        {
            internal ISynchronizedView View { get; set;}
            internal IList<ViewKey> Keys;

            internal IEnumerable<UpdateCandidate> GetCandidatesForUpdate()
            {
                return from key in this.Keys
                select new UpdateCandidate {
                    ViewKey = key,
                    Chunk = this
                };
            }
        }

        internal struct UpdateCandidate
        {
            internal ViewKey ViewKey { get; set; }
            internal UpdateChunk Chunk { get; set; }
        }
    }
}
