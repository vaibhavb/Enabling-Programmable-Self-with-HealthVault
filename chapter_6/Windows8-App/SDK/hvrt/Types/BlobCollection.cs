// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public interface IBlobCollection : IList<Blob>
    {
        int IndexOfBlobNamed(string name);
    }

    public sealed class BlobCollection : IBlobCollection
    {
        private LazyList<Blob> m_items;

        public BlobCollection()
        {
            m_items = new LazyList<Blob>();
        }

        #region IBlobCollection Members

        public int Count
        {
            get { return m_items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public Blob this[int index]
        {
            get { return m_items[index]; }
            set
            {
                ValidateItem(value);
                m_items[index] = value;
            }
        }

        public void Add(Blob item)
        {
            ValidateItem(item);
            m_items.Add(item);
        }

        public void Clear()
        {
            m_items.Clear();
        }

        public bool Contains(Blob item)
        {
            return m_items.Contains(item);
        }

        public void CopyTo(Blob[] array, int arrayIndex)
        {
            m_items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(Blob item)
        {
            return m_items.IndexOf(item);
        }

        public int IndexOfBlobNamed(string name)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            for (int i = 0, count = m_items.Count; i < count; ++i)
            {
                string blobName = m_items[i].Name;
                if (blobName == name)
                {
                    return i;
                }
            }

            return -1;
        }

        public void Insert(int index, Blob item)
        {
            ValidateItem(item);
            m_items.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            m_items.RemoveAt(index);
        }

        public bool Remove(Blob item)
        {
            return m_items.Remove(item);
        }

        public IEnumerator<Blob> GetEnumerator()
        {
            return m_items.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        #endregion

        private void ValidateItem(Blob item)
        {
            if (item == null)
            {
                throw new ArgumentNullException("item");
            }
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            m_items.ValidateRequired("Items");
        }
    }
}