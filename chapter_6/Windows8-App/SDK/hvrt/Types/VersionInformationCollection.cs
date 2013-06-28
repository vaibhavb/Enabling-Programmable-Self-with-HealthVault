// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public interface IVersionInformationCollection : IHealthVaultTypeSerializable, IList<VersionInformation>
    {
    }

    public sealed class VersionInformationCollection : IVersionInformationCollection
    {
        private LazyList<VersionInformation> m_items;

        public VersionInformationCollection()
        {
            m_items = new LazyList<VersionInformation>();
        }

        #region IVersionInformationCollection Members

        public int Count
        {
            get { return m_items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public VersionInformation this[int index]
        {
            get { return m_items[index]; }
            set { m_items[index] = value; }
        }

        public void Add(VersionInformation item)
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

        public bool Contains(VersionInformation item)
        {
            return m_items.Contains(item);
        }

        public void CopyTo(VersionInformation[] array, int arrayIndex)
        {
            m_items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(VersionInformation item)
        {
            return m_items.IndexOf(item);
        }

        public void Insert(int index, VersionInformation item)
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

        public bool Remove(VersionInformation item)
        {
            return m_items.Remove(item);
        }

        public IEnumerator<VersionInformation> GetEnumerator()
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