// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Windows.Storage;
using Windows.Storage.Streams;
using HealthVault.Foundation;

namespace HealthVault.Store
{
    public class CachingObjectStore : IObjectStore
    {
        IObjectStore m_inner;
        ICache<string, object> m_cache;

        public CachingObjectStore(IObjectStore inner, ICache<string, object> cache)
        {
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            if (cache == null)
            {
                throw new ArgumentNullException("cache");
            }
            m_inner = inner;
            m_cache = cache;
        }

        public ICache<string, object> Cache
        {
            get { return m_cache;}
        }

        public async Task<bool> KeyExistsAsync(string key)
        {
            object value;
            if (m_cache.TryGet(key, out value) && value != null)
            {
                return true;
            }

            return await m_inner.KeyExistsAsync(key);
        }

        public async Task DeleteAllAsync()
        {
            m_cache.Clear();
            await m_inner.DeleteAllAsync();
        }

        public async Task DeleteAsync(string key)
        {
            m_cache.Remove(key);
            await m_inner.DeleteAsync(key);
        }

        public async Task<IList<string>> GetAllKeysAsync()
        {
            return await m_inner.GetAllKeysAsync();
        }

        public async Task<DateTimeOffset> GetUpdateDateAsync(string key)
        {
            return await m_inner.GetUpdateDateAsync(key);
        }

        public async Task<object> GetAsync(string key, Type type)
        {
            object obj = null;
            if (m_cache.TryGet(key, out obj))
            {
                return obj;
            }

            obj = await m_inner.GetAsync(key, type);
            if (obj != null)
            {
                m_cache.Put(key, obj);
            }

            return obj;
        }

        public async Task<object> RefreshAndGetAsync(string key, Type type)
        {
            m_cache.Remove(key);
            return await this.GetAsync(key, type);
        }

        public async Task PutAsync(string key, object value)
        {
            m_cache.Remove(key);
            await m_inner.PutAsync(key, value);
        }

        public async Task<Stream> OpenReadStreamAsync(string key)
        {
            return await m_inner.OpenReadStreamAsync(key);
        }

        public async Task<Stream> OpenWriteStreamAsync(string key)
        {
            return await m_inner.OpenWriteStreamAsync(key);
        }

        public async Task<IRandomAccessStreamWithContentType> OpenContentStreamAsync(string key)
        {
            return await m_inner.OpenContentStreamAsync(key);
        }

        public async Task<bool> ChildStoreExistsAsync(string childName)
        {
            return await m_inner.ChildStoreExistsAsync(childName);
        }

        public async Task<IObjectStore> CreateChildStoreAsync(string childName)
        {
            return await m_inner.CreateChildStoreAsync(childName);
        }

        public async Task DeleteChildStoreAsync(string childName)
        {
            await m_inner.DeleteChildStoreAsync(childName);
        }

        public async Task<IStorageFile> GetStorageFileAsync(string key)
        {
            return await m_inner.GetStorageFileAsync(key);
        }
    }
}
