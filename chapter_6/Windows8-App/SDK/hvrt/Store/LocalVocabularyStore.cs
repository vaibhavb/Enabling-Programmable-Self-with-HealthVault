// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using HealthVault.Foundation;
using HealthVault.Types;
using Windows.Foundation;

namespace HealthVault.Store
{
    public sealed class LocalVocabularyStore
    {
        private readonly HealthVaultApp m_app;
        private readonly LocalStore m_root;

        internal LocalVocabularyStore(HealthVaultApp app, IObjectStore parentStore)
        {
            m_app = app;
            m_root = new LocalStore(parentStore, "Vocab");
        }

        public IAsyncOperation<VocabCodeSet> GetAsync(VocabIdentifier vocabID)
        {
            if (vocabID == null)
            {
                throw new ArgumentNullException("vocabID");
            }

            return GetVocabAsync(vocabID).AsAsyncOperation();
        }

        public IAsyncAction PutAsync(VocabIdentifier vocabID, VocabCodeSet vocab)
        {
            if (vocabID == null)
            {
                throw new ArgumentNullException("vocabID");
            }
            if (vocab == null)
            {
                throw new ArgumentNullException("vocab");
            }
            string key = vocabID.GetKey();
            return m_root.Store.PutAsync(key, vocab).AsAsyncAction();
        }

        public IAsyncAction RemoveAsync(VocabIdentifier vocabID)
        {
            if (vocabID == null)
            {
                throw new ArgumentNullException("vocabID");
            }

            return m_root.Store.DeleteAsync(vocabID.GetKey()).AsAsyncAction();
        }

        /// <summary>
        /// If vocabs are not available, will kick off a fetch in the background
        /// </summary>
        public IAsyncAction EnsureVocabAsync(VocabIdentifier vocabID, int maxAgeSeconds)
        {
            return Task.Run(() => EnsureVocabImplAsync(vocabID, TimeSpan.FromSeconds(maxAgeSeconds))).AsAsyncAction();
        }

        /// <summary>
        /// If vocabs are not available, will kick off a fetch in the background
        /// </summary>
        public IAsyncAction EnsureVocabsAsync(IList<VocabIdentifier> vocabIDs, int maxAgeSeconds)
        {
            if (vocabIDs == null)
            {
                throw new ArgumentNullException("vocabIDs");
            }

            // make .net copy of COM object which can safely be accessed on background thread
            var ids = vocabIDs.ToArray();

            return Task.Run(() => EnsureVocabsImplAsync(ids, TimeSpan.FromSeconds(maxAgeSeconds))).AsAsyncAction();
        }

        private async Task<VocabCodeSet> GetVocabAsync(VocabIdentifier vocabID)
        {
            string key = vocabID.GetKey();
            return (VocabCodeSet) await m_root.Store.GetAsync(key, typeof (VocabCodeSet));
        }

        private async Task<bool> IsStale(VocabIdentifier vocabID, TimeSpan maxAge)
        {
            DateTimeOffset dt = await m_root.Store.GetUpdateDateAsync(vocabID.GetKey());
            TimeSpan offset = DateTimeOffset.Now.Subtract(dt);

            return (offset >= maxAge);
        }

        private async Task EnsureVocabImplAsync(VocabIdentifier vocabID, TimeSpan maxAge)
        {
            bool isStale = await IsStale(vocabID, maxAge);
            if (isStale)
            {
                await DownloadVocabs(new[] {vocabID});
            }
        }

        private async Task EnsureVocabsImplAsync(IList<VocabIdentifier> vocabIDs, TimeSpan maxAge)
        {
            var staleVocabs = new LazyList<VocabIdentifier>();

            for (int i = 0; i < vocabIDs.Count; ++i)
            {
                VocabIdentifier vocabID = vocabIDs[i];
                if (await IsStale(vocabID, maxAge))
                {
                    staleVocabs.Add(vocabID);
                }
            }
            
            if (staleVocabs.Count > 0)
            {
                await DownloadVocabs(staleVocabs.Value);
            }
        }

        private async Task DownloadVocabs(IList<VocabIdentifier> vocabIDs)
        {
            try
            {
                IList<VocabCodeSet> vocabs = await m_app.Vocabs.GetAsync(vocabIDs);
                if (vocabs.IsNullOrEmpty())
                {
                    return;
                }

                for (int i = 0; i < vocabIDs.Count; ++i)
                {
                    await PutAsync(vocabIDs[i], vocabs[i]);
                }
            }
            catch
            {
            }
        }
    }
}