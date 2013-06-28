// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using HealthVault.Foundation;
using HealthVault.Types;

namespace HealthVault.Store
{
    public interface IViewKeyCollection : IList<ViewKey>
    {
        void TrimExcess();
    }

    /// <summary>
    /// Thread safe
    /// Keys are SORTED BY {EffectiveDate, Key}. Same sort order that HealthVault returns results with.
    /// Also contains an reverse lookup: ItemID --> Index
    /// </summary>
    public sealed class ViewKeyCollection : IViewKeyCollection
    {
        static ViewKeyComparer s_keyComparer = new ViewKeyComparer();
        private readonly List<ViewKey> m_keys;
        private readonly Dictionary<string, ViewKey> m_keysByID;
        private bool m_sorted;

        public ViewKeyCollection()
        {
            m_sorted = false;
            m_keys = new List<ViewKey>();
            m_keysByID = new Dictionary<string, ViewKey>();
        }

        public DateTimeOffset MinDate
        {
            get
            {
                lock (m_keys)
                {
                    if (m_keys.IsNullOrEmpty())
                    {
                        return DateTimeOffset.MinValue;
                    }

                    return m_keys[m_keys.Count - 1].EffectiveDate.Value;
                }
            }
        }

        public DateTimeOffset MaxDate
        {
            get
            {
                lock (m_keys)
                {
                    if (m_keys.IsNullOrEmpty())
                    {
                        return DateTimeOffset.MinValue;
                    }

                    return m_keys[0].EffectiveDate.Value;
                }
            }
        }

        #region IViewKeyCollection Members

        public ViewKey this[int index]
        {
            get { return Get(index); }
            set { Insert(index, value); }
        }

        public int Count
        {
            get { return m_keys.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(ViewKey key)
        {
            lock (m_keys)
            {
                AddImpl(key);
                m_sorted = false;
            }
        }

        public void Clear()
        {
            lock (m_keys)
            {
                m_keys.Clear();
                m_keysByID.Clear();
                m_sorted = false;
            }
        }

        public bool Contains(ViewKey key)
        {
            lock (m_keys)
            {
                EnsureOrdered();

                return (IndexOf(key) >= 0);
            }
        }

        public int IndexOf(ViewKey key)
        {
            lock (m_keys)
            {
                EnsureOrdered();

                return m_keys.BinarySearch(key, s_keyComparer);
            }
        }

        public void Insert(int index, ViewKey item)
        {
            ValidateKey(item);
            //
            // Since we don't actually support explicit insertion
            //
            Add(item);
        }

        public bool Remove(ViewKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("item");
            }

            lock (m_keys)
            {
                int index = IndexOf(key);
                if (index < 0)
                {
                    return false;
                }

                RemoveAt(index);
                return true;
            }
        }

        public void RemoveAt(int index)
        {
            lock (m_keys)
            {
                ViewKey item = m_keys[index];
                m_keys.RemoveAt(index);
                m_keysByID.Remove(item.Key.ID);
            }
        }

        public void CopyTo(ViewKey[] array, int arrayIndex)
        {
            lock (m_keys)
            {
                EnsureOrdered();
                m_keys.CopyTo(array, arrayIndex);
            }
        }

        public void TrimExcess()
        {
            lock (m_keys)
            {
                m_keys.TrimExcess();
            }
        }

        public IEnumerator<ViewKey> GetEnumerator()
        {
            int i = 0;
            while (true)
            {
                lock (m_keys)
                {
                    if (i >= Count)
                    {
                        break;
                    }

                    yield return m_keys[i];
                    ++i;
                }
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public void AddRange(IEnumerable<ViewKey> keys)
        {
            lock (m_keys)
            {
                foreach (ViewKey key in keys)
                {
                    AddImpl(key);
                }

                m_sorted = false;
            }
        }

        internal void AddImpl(ViewKey key)
        {
            ValidateKey(key);

            // Add to the index first, to catch duplicates
            m_keysByID.Add(key.ID, key);
            m_keys.Add(key);
        }

        public void AddFromPending(PendingItem item)
        {
            Add(ViewKey.FromPendingItem(item));
        }

        public void AddFromPendingItems(IEnumerable<PendingItem> items)
        {
            if (items == null)
            {
                throw new ArgumentException("items");
            }

            IEnumerable<ViewKey> keys = (
                from item in items
                select ViewKey.FromPendingItem(item)
                );

            AddRange(keys);
        }

        public void AddFromItem(RecordItem item)
        {
            Add(ViewKey.FromItem(item));
        }

        public void AddFromItems(IEnumerable<RecordItem> items)
        {
            if (items == null)
            {
                throw new ArgumentException("items");
            }

            IEnumerable<ViewKey> keys = (
                from item in items
                select ViewKey.FromItem(item)
                );

            AddRange(keys);
        }

        public bool ContainsID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id");
            }

            lock (m_keys)
            {
                return m_keysByID.ContainsKey(id);
            }
        }

        public ViewKey Get(int index)
        {
            lock (m_keys)
            {
                EnsureOrdered();
                return m_keys[index];
            }
        }

        public ViewKey GetByID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id");
            }

            lock (m_keys)
            {
                int index = IndexOfID(id);
                if (index < 0)
                {
                    return null;
                }

                return m_keys[index];
            }
        }

        public ViewKey GetByItemKey(ItemKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            return GetByID(key.ID);
        }

        public int IndexOfID(string id)
        {
            if (string.IsNullOrEmpty(id))
            {
                throw new ArgumentException("id");
            }

            lock (m_keys)
            {
                ViewKey item = null;
                if (!m_keysByID.TryGetValue(id, out item) || item == null)
                {
                    return -1;
                }

                return IndexOf(item);
            }
        }

        public int IndexOfItemKey(ItemKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            return IndexOfID(key.ID);
        }

        public int InsertInOrder(ViewKey key)
        {
            ValidateKey(key);

            lock (m_keys)
            {
                int index = IndexOf(key);
                if (index < 0)
                {
                    index = ~index;
                }
                m_keys.Insert(index, key);
                m_keysByID[key.Key.ID] = key;

                return index;
            }
        }

        public int RemoveByID(string id)
        {
            lock (m_keys)
            {
                int index = IndexOfID(id);
                if (index >= 0)
                {
                    RemoveAt(index);
                }

                return index;
            }
        }

        public int RemoveByItemKey(ItemKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            return RemoveByID(key.ID);
        }

        public void UpdateKey(ViewKey updatedKey)
        {
            if (updatedKey == null)
            {
                throw new ArgumentNullException("updatedKey");
            }

            this.UpdateKey(updatedKey.ID, updatedKey);
        }

        public void UpdateKey(string itemID, ViewKey updatedKey)
        {
            if (updatedKey == null)
            {
                throw new ArgumentNullException("updatedKey");
            }

            lock(m_keys)
            {
                this.RemoveByID(itemID);
                this.Add(updatedKey);
            }
        }

        public IList<ViewKey> SelectKeys(int startAt, int count)
        {
            ValidateIndex(startAt);

            lock (m_keys)
            {
                this.EnsureOrdered();

                count = CorrectCount(startAt, count);
                var keys = new ViewKey[count];
                m_keys.CopyTo(keys, startAt);

                return keys;
            }
        }

        public IList<ItemKey> SelectItemKeys(int startAt, int count)
        {
            ValidateIndex(startAt);

            lock (m_keys)
            {
                this.EnsureOrdered();

                count = CorrectCount(startAt, count);
                var keys = new ItemKey[count];
                for (int i = startAt, max = i + count; i < max; ++i)
                {
                    keys[i] = m_keys[i].Key;
                }

                return keys;
            }
        }

        public string Serialize()
        {
            lock (m_keys)
            {
                return this.ToXml();
            }
        }

        public static ViewKeyCollection Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<ViewKeyCollection>(xml);
        }

        public void Validate()
        {
            lock (m_keys)
            {
                m_keys.ValidateRequired("Items");
            }
        }

        public static ViewKeyCollection FromQueryResult(ItemQueryResult queryResult)
        {
            ViewKeyCollection keys = new ViewKeyCollection();
            if (queryResult != null)
            {
                if (queryResult.HasItems)
                {
                    keys.AddFromItems(queryResult.Items);
                }
                if (queryResult.HasPending)
                {
                    keys.AddFromPendingItems(queryResult.PendingItems);
                }
            }

            return keys;
        }

        internal IList<ItemKey> CollectKeysNeedingDownload(int startAt, int count)
        {
            lock(m_keys)
            {
                this.EnsureOrdered();

                count = CorrectCount(startAt, count);

                var keys = new LazyList<ItemKey>();
                for (int i = startAt, max = i + count; i < max; ++i)
                {
                    ViewKey key = m_keys[i];
                    if (!key.IsLoadPending && !key.Key.IsLocal)
                    {
                        keys.Add(key.Key);
                        key.IsLoadPending = true;
                    }
                }

                return keys.HasValue ? keys.Value : null;
            }
        }

        internal IList<ViewKey> CollectViewKeysNeedingDownload(int startAt, int count)
        {
            lock (m_keys)
            {
                this.EnsureOrdered();

                count = CorrectCount(startAt, count);

                List<ViewKey> keys = new List<ViewKey>();
                for (int i = startAt, max = i + count; i < max; ++i)
                {
                    ViewKey key = m_keys[i];
                    if (!key.IsLoadPending && !key.Key.IsLocal)
                    {
                        keys.Add(key);
                        key.IsLoadPending = true;
                    }
                }

                return keys;
            }
        }

        internal void SetLoadingStateForKeys(IList<ItemKey> keys, bool isLoading)
        {
            lock(m_keys)
            {
                this.EnsureOrdered();

                for (int i = 0, count = keys.Count; i < count; ++i)
                {
                    ViewKey viewKey = this.GetByItemKey(keys[i]);
                    if (viewKey != null)
                    {
                        viewKey.IsLoadPending = isLoading;
                    }
                }
            }
        }
       
        internal void ValidateIndex(int index)
        {
            if (index < 0 || index >= Count)
            {
                throw new IndexOutOfRangeException();
            }
        }

        internal void ValidateKey(ViewKey key)
        {
            if (key == null)
            {
                throw new ArgumentNullException("key");
            }

            key.Validate();
        }

        internal int GetCorrectedCount(int startAt, int count)
        {
            lock (m_keys)
            {
                return CorrectCount(startAt, count);
            }
        }

        internal void EnsureReady()
        {
            lock(m_keys)
            { 
                this.EnsureOrdered();
            }
        }

        private void EnsureOrdered()
        {
            if (m_sorted)
            {
                return;
            }

            m_keys.Sort(s_keyComparer);
            m_sorted = true;
        }

        private int CorrectCount(int startAt, int count)
        {
            EnsureOrdered();

            int max = startAt + count;
            if (max > Count)
            {
                return Count - startAt;
            }

            return count;
        }
    }
}