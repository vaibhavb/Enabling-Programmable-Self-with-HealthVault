// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthVault.Store
{
    /// <summary>
    /// Thread safe LRU cache. 
    /// Have to write our own because WinRT does not have one!
    /// </summary>
    /// <typeparam name="K"></typeparam>
    /// <typeparam name="V"></typeparam>
    public class LRUCache<K, V> : ICache<K, V>
    {
        Dictionary<K, LinkedListNode<KeyValuePair<K, V>>> m_index;
        LinkedList<KeyValuePair<K, V>> m_itemList;
        int m_highWatermark;
        int m_lowWatermark;

        long m_hitCount;
        long m_missCount;

        public LRUCache(int maxEntries)
            : this(maxEntries, maxEntries, null)
        {
        }

        public LRUCache(int maxEntries, IEqualityComparer<K> comparer)
            : this(maxEntries, maxEntries, comparer)
        {
        }

        public LRUCache(int lowWatermark, int highWatermark)
            : this(lowWatermark, highWatermark, null)
        {
        }

        public LRUCache(int lowWatermark, int highWatermark, IEqualityComparer<K> comparer)
        {
            if (comparer != null)
            {
                m_index = new Dictionary<K, LinkedListNode<KeyValuePair<K, V>>>(comparer);
            }
            else
            {
                m_index = new Dictionary<K, LinkedListNode<KeyValuePair<K, V>>>();
            }

            m_itemList = new LinkedList<KeyValuePair<K, V>>();

            this.HighWatermark = highWatermark;
            this.LowWatermark = lowWatermark;
        }

        public int Count
        {
            get { return m_itemList.Count; }
        }

        public int MaxCount
        {
            get { return this.HighWatermark;}
            set
            {
                this.HighWatermark = value;
                this.LowWatermark = value;
                this.Trim();
            }
        }

        public long HitCount
        {
            get { return m_hitCount;}
        }

        public long MissCount
        {
            get { return m_missCount;}
        }

        public int HighWatermark
        {
            get
            {
                return m_highWatermark;
            }
            set
            {
                if (value < 0)
                {
                    throw new ArgumentException("HighWatermark");
                }
                m_highWatermark = value;
            }
        }

        public int LowWatermark
        {
            get
            {
                return m_lowWatermark;
            }
            set
            {
                if (value < 0 || value > m_highWatermark)
                {
                    throw new ArgumentException("LowWatermark");
                }
                m_lowWatermark = value;
            }
        }

        internal bool ShouldCache
        {
            get { return (m_highWatermark > 0);}
        }

        public event Action<KeyValuePair<K, V>> Purged;

        public V this[K key]
        {
            get
            {
                V value;
                if (this.TryGet(key, out value))
                {
                    return value;
                }

                throw new KeyNotFoundException();
            }
            set
            {
                this.Put(new KeyValuePair<K,V>(key, value));
            }
        }

        public bool TryGet(K key, out V value)
        {
            value = default(V);
            if (!this.ShouldCache)
            {
                return false;
            }

            lock (m_itemList)
            {
                LinkedListNode<KeyValuePair<K, V>> item = null;
                if (m_index.TryGetValue(key, out item))
                {
                    ++m_hitCount;
                    this.MakeMRU(item);
                    value = item.Value.Value;
                    return true;
                }

                ++m_missCount;
                return false;
            }
        }

        public bool Contains(K key)
        {
            V value;
            return this.TryGet(key, out value);
        }

        public void Put(K key, V value)
        {
            this.Put(new KeyValuePair<K, V>(key, value));
        }

        void Put(KeyValuePair<K, V> kvPair)
        {
            if (!this.ShouldCache)
            {
                return;  // Don't cache
            }

            lock (m_itemList)
            {
                //
                // Update existing, if it exists
                //
                LinkedListNode<KeyValuePair<K, V>> newNode = this.UpdateExisting(kvPair);
                if (newNode != null)
                {
                    this.MakeMRU(newNode);
                    return;
                }
                //
                // New Entry
                //
                if (m_itemList.Count == m_highWatermark)
                {
                    // Remove old items from the cache. 
                    // Reuse the last node... keep GC happier
                    newNode = this.Shrink();
                }

                if (newNode == null)
                {
                    newNode = new LinkedListNode<KeyValuePair<K, V>>(kvPair);
                }
                else
                {
                    newNode.Value = kvPair;
                }

                m_index[kvPair.Key] = newNode;
                m_itemList.AddFirst(newNode);
            }
        }
        
        public void Remove(K key)
        {
            if (!this.ShouldCache)
            {
                return;
            }

            lock (m_itemList)
            {
                LinkedListNode<KeyValuePair<K, V>> node = null;
                if (m_index.TryGetValue(key, out node) && node != null)
                {
                    m_index.Remove(key);
                    m_itemList.Remove(node);
                }
            }
        }

        public void Clear()
        {
            lock(m_itemList)
            {
                m_index.Clear();
                m_itemList.Clear();
            }
        }

        public void Trim()
        {
            lock (m_itemList)
            {
                if (this.MaxCount <= 0)
                {
                    this.Clear();
                }
                else
                {
                    this.Shrink();
                }
            }
        }

        public IList<K> GetAllKeys()
        {
            lock(m_itemList)
            {
                return m_index.Keys.ToArray();
            }
        }

        void MakeMRU(LinkedListNode<KeyValuePair<K, V>> node)
        {
            m_itemList.Remove(node);
            m_itemList.AddFirst(node);
        }

        LinkedListNode<KeyValuePair<K, V>> Shrink()
        {
            LinkedListNode<KeyValuePair<K, V>> lastNode = null;
            while (m_itemList.Count >= m_lowWatermark)
            {
                lastNode = m_itemList.Last;

                KeyValuePair<K, V> last = lastNode.Value;

                m_itemList.RemoveLast();
                m_index.Remove(last.Key);

                if (this.Purged != null)
                {
                    this.Purged(last);
                }
            }

            return lastNode;
        }

        LinkedListNode<KeyValuePair<K, V>> UpdateExisting(KeyValuePair<K, V> kvPair)
        {
            LinkedListNode<KeyValuePair<K, V>> item = null;
            if (m_index.TryGetValue(kvPair.Key, out item))
            {
                // Exists...
                item.Value = kvPair;
            }
            return item;
        }

        public static LRUCache<K, V> Create(int maxCachedItems)
        {
            return (maxCachedItems > 0) ? new LRUCache<K, V>(maxCachedItems) : null;
        }
    }
}
