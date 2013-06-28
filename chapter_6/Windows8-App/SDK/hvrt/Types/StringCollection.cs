// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public interface IStringCollection : IHealthVaultTypeSerializable, IList<string>
    {
    }

    /// <summary>
    /// String collections that are also XmlSerializable
    /// No inheritance in WINRT...(and none of the standard collections work)
    /// </summary>
    public sealed class StringCollection : IStringCollection
    {
        private LazyList<string> m_items;

        public StringCollection()
        {
        }

        public StringCollection(IEnumerable<string> items)
        {
            if (items != null)
            {
                AddRange(items);
            }
        }

        #region IStringCollection Members

        public string this[int index]
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

        public void Add(string item)
        {
            ValidateItem(item);
            m_items.Add(item);
        }

        public void Clear()
        {
            m_items.Clear();
        }

        public int IndexOf(string item)
        {
            return m_items.IndexOf(item);
        }

        public void Insert(int index, string item)
        {
            ValidateItem(item);
            m_items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_items.RemoveAt(index);
        }

        public bool Contains(string item)
        {
            return m_items.Contains(item);
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            m_items.CopyTo(array, arrayIndex);
        }

        public bool Remove(string item)
        {
            return m_items.Remove(item);
        }

        public IEnumerator<string> GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return m_items.GetEnumerator();
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

        public void AddRange(IEnumerable<string> items)
        {
            m_items.AddRange(items);
        }

        private void ValidateItem(string item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
        }
    }
}