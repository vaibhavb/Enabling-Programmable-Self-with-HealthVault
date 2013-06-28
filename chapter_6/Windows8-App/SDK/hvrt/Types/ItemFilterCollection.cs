// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public interface IItemFilterCollection : IHealthVaultTypeSerializable, IList<ItemFilter>
    {
    }


    public sealed class ItemFilterCollection : IItemFilterCollection
    {
        private LazyList<ItemFilter> m_items;

        public ItemFilterCollection()
        {
            m_items = new LazyList<ItemFilter>();
        }

        #region IItemFilterCollection Members

        public int Count
        {
            get { return m_items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ItemFilter this[int index]
        {
            get { return m_items[index]; }
            set
            {
                ValidateItem(value);
                m_items[index] = value;
            }
        }

        public void Add(ItemFilter item)
        {
            ValidateItem(item);
            m_items.Add(item);
        }

        public void Clear()
        {
            m_items.Clear();
        }

        public bool Contains(ItemFilter item)
        {
            return m_items.Contains(item);
        }

        public void CopyTo(ItemFilter[] array, int arrayIndex)
        {
            m_items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(ItemFilter item)
        {
            return m_items.IndexOf(item);
        }

        public void Insert(int index, ItemFilter item)
        {
            ValidateItem(item);
            m_items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_items.RemoveAt(index);
        }

        public bool Remove(ItemFilter item)
        {
            return m_items.Remove(item);
        }

        public IEnumerator<ItemFilter> GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            m_items.ValidateRequired("Items");
        }

        #endregion

        public void AddRange(IEnumerable<ItemFilter> items)
        {
            m_items.AddRange(items);
        }

        private void ValidateItem(ItemFilter item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
        }
    }
}