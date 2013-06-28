// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HealthVault.Store
{
    public interface ICache<K, V>
    {
        int Count { get;}
        int MaxCount { get; set;}

        bool TryGet(K key, out V value);
        void Put(K key, V value);

        void Remove(K key);
        void Trim();
        void Clear();

        IList<K> GetAllKeys();
    }
}
