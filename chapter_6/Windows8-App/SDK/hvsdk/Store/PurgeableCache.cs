// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthVault.Store
{
    /// <summary>
    /// A cache that can be reclaimed by the Garbage Collector if under memory pressure
    /// </summary>
    public class PurgeableCache<K, V> : LRUCache<K, WeakReference<V>>, ICache<K, V>
        where V : class
    {
        public PurgeableCache(int maxItems)
            : base(maxItems)
        {
        }

        public PurgeableCache(int maxEntries, IEqualityComparer<K> comparer)
            : base(maxEntries, comparer)
        {
        }

        public bool TryGet(K key, out V value)
        {
            WeakReference<V> valueRef = null;
            value = default(V);
            if (this.TryGet(key, out valueRef))
            {
                lock (valueRef)
                {
                    if (valueRef.TryGetTarget(out value))
                    {
                        return true;
                    }
                }
            }

            return false;
        }

        public void Put(K key, V value)
        {
            base.Put(key, new WeakReference<V>(value));
        }
    }
}
