// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public interface IStructuredNameValueCollection : IHealthVaultTypeSerializable, IList<StructuredNameValue>
    {
    }

    public sealed class StructuredNameValueCollection : IStructuredNameValueCollection
    {
        private LazyList<StructuredNameValue> m_items;

        public StructuredNameValueCollection()
        {
            m_items = new LazyList<StructuredNameValue>();
        }

        #region IStructuredNameValueCollection Members

        public int Count
        {
            get { return m_items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public StructuredNameValue this[int index]
        {
            get { return m_items[index]; }
            set { m_items[index] = value; }
        }

        public void Add(StructuredNameValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            m_items.Add(item);
        }

        public void Clear()
        {
            m_items.Clear();
        }

        public bool Contains(StructuredNameValue item)
        {
            return m_items.Contains(item);
        }

        public void CopyTo(StructuredNameValue[] array, int arrayIndex)
        {
            m_items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(StructuredNameValue item)
        {
            return m_items.IndexOf(item);
        }

        public void Insert(int index, StructuredNameValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }

            m_items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_items.RemoveAt(index);
        }

        public bool Remove(StructuredNameValue item)
        {
            return m_items.Remove(item);
        }

        public IEnumerator<StructuredNameValue> GetEnumerator()
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
    }
}