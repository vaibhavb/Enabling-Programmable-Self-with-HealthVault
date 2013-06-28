// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public interface ICodedValueCollection : IHealthVaultTypeSerializable, IList<CodedValue>
    {
    }

    public sealed class CodedValueCollection : ICodedValueCollection
    {
        private LazyList<CodedValue> m_items;

        public CodedValueCollection()
        {
            m_items = new LazyList<CodedValue>();
        }

        #region ICodedValueCollection Members

        public int Count
        {
            get { return m_items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public CodedValue this[int index]
        {
            get { return m_items[index]; }
            set
            {
                ValidateItem(value);
                m_items[index] = value;
            }
        }

        public void Add(CodedValue item)
        {
            ValidateItem(item);
            m_items.Add(item);
        }

        public void Clear()
        {
            m_items.Clear();
        }

        public bool Contains(CodedValue item)
        {
            return m_items.Contains(item);
        }

        public void CopyTo(CodedValue[] array, int arrayIndex)
        {
            m_items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(CodedValue item)
        {
            return m_items.IndexOf(item);
        }

        public void Insert(int index, CodedValue item)
        {
            ValidateItem(item);
            m_items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_items.RemoveAt(index);
        }

        public bool Remove(CodedValue item)
        {
            return m_items.Remove(item);
        }

        public IEnumerator<CodedValue> GetEnumerator()
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

        public void AddIfDoesNotExist(CodedValue item)
        {
            ValidateItem(item);
            if (!Contains(item))
            {
                m_items.Add(item);
            }
        }

        private void ValidateItem(CodedValue item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
        }
    }
}