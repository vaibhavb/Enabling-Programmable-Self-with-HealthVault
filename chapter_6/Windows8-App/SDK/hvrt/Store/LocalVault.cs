// (c) Microsoft. All rights reserved

using System;
using System.Runtime.InteropServices.WindowsRuntime;
using HealthVault.Foundation;
using Windows.Storage;
using Windows.Foundation;

namespace HealthVault.Store
{
    public sealed class LocalVault
    {
        private readonly HealthVaultApp m_app;
        private readonly LocalRecordStoreTable m_recordStores;
        private readonly IObjectStore m_vocabRoot;
        private readonly IObjectStore m_recordRoot;
        private readonly LocalVocabularyStore m_vocabStore;

        internal LocalVault(HealthVaultApp app)
            : this(app, FolderObjectStore.CreateRoot(), FolderObjectStore.CreateRoot())
        {
        }

        internal LocalVault(HealthVaultApp app, StorageFolder vocabFolder, StorageFolder recordFolder)
            : this(app, FolderObjectStore.CreateRoot(vocabFolder), FolderObjectStore.CreateRoot(recordFolder))
        {
        }

        internal LocalVault(HealthVaultApp app, IObjectStore vocabStore, IObjectStore recordStore)
        {
            m_app = app;
            m_vocabRoot = vocabStore;
            m_recordRoot = recordStore;
            m_vocabStore = new LocalVocabularyStore(m_app, m_vocabRoot);
            m_recordStores = new LocalRecordStoreTable(m_recordRoot);
        }

        internal IObjectStore VocabRoot
        {
            get { return m_vocabRoot; }
        }

        internal IObjectStore RecordRoot
        {
            get { return m_recordRoot; }
        }

        public LocalVocabularyStore VocabStore
        {
            get { return m_vocabStore; }
        }

        public LocalRecordStoreTable RecordStores
        {
            get { return m_recordStores; }
        }        
    }
}