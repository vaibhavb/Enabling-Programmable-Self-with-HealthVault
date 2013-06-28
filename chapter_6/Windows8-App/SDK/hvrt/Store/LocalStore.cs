// (c) Microsoft. All rights reserved

using System;
using System.IO;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Storage;
using Windows.Storage.Streams;

namespace HealthVault.Store
{
    public sealed class LocalStore
    {
        private readonly IObjectStore m_objectStore;

        internal LocalStore(IObjectStore store)
        {
            m_objectStore = store;
        }

        internal LocalStore(LocalStore parentStore, string childName)
            : this(parentStore.Store, childName)
        {
        }

        internal LocalStore(IObjectStore parentStore, string childName)
        {
            Task<IObjectStore> task = Task.Run(() => parentStore.CreateChildStoreAsync(childName));
            task.Wait();
            m_objectStore = task.Result;
        }

        internal IObjectStore Store
        {
            get { return m_objectStore; }
        }

        public IAsyncOperation<IStorageFile> GetStorageFileAsync(string key)
        {
            return m_objectStore.GetStorageFileAsync(key).AsAsyncOperation();
        }

        public IAsyncOperation<IInputStream> OpenReadStreamAsync(string key)
        {
            return OpenReadAsync(key).AsAsyncOperation();
        }

        public IAsyncOperation<IOutputStream> OpenWriteStreamAsync(string key)
        {
            return OpenWriteAsync(key).AsAsyncOperation();
        }

        public IAsyncOperation<IRandomAccessStreamWithContentType> OpenContentStreamAsync(string key)
        {
            return OpenContentAsync(key).AsAsyncOperation();
        }

        public IAsyncAction DeleteAsync(string key)
        {
            return m_objectStore.DeleteAsync(key).AsAsyncAction();
        }

        public IAsyncOperation<object> GetAsync(string key, string objectTypeName)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key");
            }
            if (string.IsNullOrEmpty(objectTypeName))
            {
                throw new ArgumentException("objectTypeName");
            }
            Type type = Type.GetType(objectTypeName);
            if (type == null)
            {
                throw new ArgumentException("objectTypeName");
            }

            return m_objectStore.GetAsync(key, type).AsAsyncOperation();
        }

        public IAsyncAction PutAsync(string key, object item)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new ArgumentException("key");
            }
            if (item == null)
            {
                throw new ArgumentException("item");
            }

            return m_objectStore.PutAsync(key, item).AsAsyncAction();
        }

        public IAsyncOperation<DateTimeOffset> UpdateDateForKeyAsync(string key)
        {
            return m_objectStore.GetUpdateDateAsync(key).AsAsyncOperation();
        }

        private async Task<IInputStream> OpenReadAsync(string key)
        {
            Stream stream = await m_objectStore.OpenReadStreamAsync(key);
            return stream.AsInputStream();
        }

        private async Task<IOutputStream> OpenWriteAsync(string key)
        {
            Stream stream = await m_objectStore.OpenWriteStreamAsync(key);
            return stream.AsOutputStream();
        }

        private async Task<IRandomAccessStreamWithContentType> OpenContentAsync(string key)
        {
            IRandomAccessStreamWithContentType content = await m_objectStore.OpenContentStreamAsync(key);
            return content;
        }
    }
}