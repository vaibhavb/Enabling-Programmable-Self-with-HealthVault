// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;

namespace HealthVault.Types
{
    public sealed class Collection : IList<object>
    {
        private LazyList<object> m_items;

        public Collection()
        {
            m_items = new LazyList<object>();
        }

        #region IList<object> Members

        public object this[int index]
        {
            get { return m_items[index]; }
            set
            {
                ValidateItem(value);
                m_items[index] = value;
            }
        }

        public int Count
        {
            get { return m_items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public void Add(object item)
        {
            ValidateItem(item);
            m_items.Add(item);
        }

        public void Clear()
        {
            m_items.Clear();
        }

        public int IndexOf(object item)
        {
            return m_items.IndexOf(item);
        }

        public void Insert(int index, object item)
        {
            ValidateItem(item);
            m_items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_items.RemoveAt(index);
        }

        public bool Contains(object item)
        {
            return m_items.Contains(item);
        }

        public void CopyTo(object[] array, int arrayIndex)
        {
            m_items.CopyTo(array, arrayIndex);
        }

        public bool Remove(object item)
        {
            return m_items.Remove(item);
        }

        public IEnumerator<object> GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private void ValidateItem(object item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
        }
    }
}