// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;

namespace HealthVault.Store
{
    public delegate void ObjectPurgedNotifyDelegate(ObjectCache cache, string key);

    public sealed class ObjectCache
    {
        private readonly ICache<string, object> m_cache;

        public ObjectCache(int maxItems, bool isPurgeable)
        {
            if (isPurgeable)
            {
                m_cache = new PurgeableCache<string, object>(maxItems);
            }
            else
            {
                m_cache = new LRUCache<string, object>(maxItems);
            }
        }

        public int Count
        {
            get { return m_cache.Count; }
        }

        public int MaxCount
        {
            get { return m_cache.MaxCount; }
            set { m_cache.MaxCount = value; }
        }

        public object Get(string key)
        {
            object value = null;
            if (m_cache.TryGet(key, out value))
            {
                return value;
            }

            return null;
        }

        public void Put(string key, object value)
        {
            m_cache.Put(key, new WeakReference<object>(value));
        }

        public void Remove(string key)
        {
            m_cache.Remove(key);
        }

        public void Trim()
        {
            m_cache.Trim();
        }

        public void Clear()
        {
            m_cache.Clear();
        }

        public IList<string> GetAllKeys()
        {
            return m_cache.GetAllKeys();
        }
    }
}