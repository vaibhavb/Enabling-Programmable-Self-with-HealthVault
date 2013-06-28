// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using HealthVault.Foundation;
using HealthVault.Types;
using Windows.Foundation;

namespace HealthVault.ItemTypes
{
    /// <summary>
    /// This class will automatically resolve Pending (Partial) items
    /// </summary>
    public sealed class ItemDataTypedList : IReadOnlyList<IItemDataTyped>
    {
        private readonly List<KeyValuePair<ItemKey, IItemDataTyped>> m_items;
        private readonly IRecord m_record;
        private readonly ItemView m_view;
        private List<ItemKey> m_keyCollector;
        private int m_pendingCount;

        public ItemDataTypedList(IRecord record, IEnumerable<ItemKey> keys)
            : this(record, new ItemView(ItemSectionType.Standard), keys)
        {
        }

        public ItemDataTypedList(IRecord record, ItemView view, IEnumerable<ItemKey> keys)
            : this(record, view, null, keys)
        {
        }

        public ItemDataTypedList(
            IRecord record, ItemView view, IEnumerable<RecordItem> items, IEnumerable<ItemKey> pendingItems)
        {
            if (record == null)
            {
                throw new ArgumentNullException("record");
            }
            if (view == null)
            {
                throw new ArgumentNullException("view");
            }
            m_record = record;
            m_view = view;

            m_items = new List<KeyValuePair<ItemKey, IItemDataTyped>>();
            AddItems(items);
            AddItems(pendingItems);
            m_items.TrimExcess();
        }

        public IEnumerable<ItemKey> Keys
        {
            get
            {
                foreach (var kvPair in m_items)
                {
                    yield return kvPair.Key;
                }
            }
        }

        public int PendingCount
        {
            get { return m_pendingCount; }
        }

        #region IReadOnlyList<IItemDataTyped> Members

        public int Count
        {
            get { return m_items.Count; }
        }

        /// <summary>
        /// CAN RETURN NULL if:
        /// 1. The item is not available because it hasn't been downloaded yet
        /// 2. Any attempt was made to download the item, but it had been deleted from the user's record in meanwhile
        /// </summary>
        /// <param name="index"></param>
        /// <returns></returns>
        public IItemDataTyped this[int index]
        {
            get { return m_items[index].Value; }
        }

        public IEnumerator<IItemDataTyped> GetEnumerator()
        {
            foreach (var kvPair in m_items)
            {
                yield return kvPair.Value;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        public bool AreItemsAvailable(int startAt, int count)
        {
            for (int i = startAt, max = Math.Min(startAt + count, Count); i < max; ++i)
            {
                if (m_items[i].Value == null)
                {
                    return false;
                }
            }

            return true;
        }

        public IAsyncAction EnsureAvailableAsync()
        {
            return EnsureAvailableAsync(0, m_items.Count);
        }

        public IAsyncAction EnsureAvailableAsync(int startAt, int count)
        {
            return AsyncInfo.Run(cancelToken => EnsureLoadedAsync(cancelToken, startAt, count));
        }

        private async Task EnsureLoadedAsync(CancellationToken cancelToken, int startAt, int count)
        {
            List<ItemKey> pendingKeys = CollectPendingKeys(startAt, count);
            if (pendingKeys.Count == 0)
            {
                return;
            }

            ItemQuery query = ItemQuery.QueryForKeys(pendingKeys);
            query.View = m_view;

            IList<RecordItem> items = await m_record.GetAllItemsAsync(query).AsTask(cancelToken);
            StoreLoadedItems(startAt, count, items);
        }

        private void AddItems(IEnumerable<ItemKey> keys)
        {
            if (keys == null)
            {
                return;
            }

            foreach (ItemKey key in keys)
            {
                m_items.Add(new KeyValuePair<ItemKey, IItemDataTyped>(key, null));
                ++m_pendingCount;
            }
        }

        private void AddItems(IEnumerable<RecordItem> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (RecordItem item in items)
            {
                m_items.Add(item.GetTypedDataWithKey());
            }
        }

        private void AddItems(IEnumerable<PendingItem> items)
        {
            if (items == null)
            {
                return;
            }

            foreach (PendingItem item in items)
            {
                m_items.Add(new KeyValuePair<ItemKey, IItemDataTyped>(item.Key, null));
            }
        }

        private List<ItemKey> CollectPendingKeys(int startAt, int count)
        {
            if (m_keyCollector == null)
            {
                m_keyCollector = new List<ItemKey>();
            }
            else
            {
                m_keyCollector.Clear();
            }
            for (int i = startAt, max = Math.Min(startAt + count, Count); i < max; ++i)
            {
                if (m_items[i].Value == null)
                {
                    m_keyCollector.Add(m_items[i].Key);
                }
            }
            return m_keyCollector;
        }

        private void StoreLoadedItems(int startAt, int count, IList<RecordItem> newItems)
        {
            if (newItems == null || newItems.Count == 0)
            {
                return;
            }

            int max = Math.Min(startAt + count, Count);
            //
            // Platform returns matched items in the order they were requested
            //
            for (int i = startAt, iNewItem = 0; i < max; ++i)
            {
                KeyValuePair<ItemKey, IItemDataTyped> pair = m_items[i];
                if (pair.Key.EqualsKey(newItems[iNewItem].Key))
                {
                    m_items[i] = newItems[iNewItem++].GetTypedDataWithKey();
                    --m_pendingCount;
                }
            }
        }
    }
}