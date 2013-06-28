// (c) Microsoft. All rights reserved

using System;
using System.Collections.Generic;

namespace HealthVault
{
    internal struct LazyList<T>
    {
        private List<T> m_list;

        public List<T> Value
        {
            get
            {
                if (m_list == null)
                {
                    m_list = new List<T>();
                }

                return m_list;
            }
            set { m_list = value; }
        }

        public bool HasValue
        {
            get { return (m_list != null); }
        }

        public int Count
        {
            get { return HasValue ? m_list.Count : 0; }
        }

        public T this[int index]
        {
            get
            {
                if (!HasValue)
                {
                    throw new IndexOutOfRangeException();
                }
                return m_list[index];
            }
            set { Value[index] = value; }
        }

        public void Add(T item)
        {
            Value.Add(item);
        }

        public void AddRange(IEnumerable<T> items)
        {
            Value.AddRange(items);
        }

        public bool Contains(T item)
        {
            if (!HasValue)
            {
                return false;
            }

            return m_list.Contains(item);
        }

        public void Clear()
        {
            if (HasValue)
            {
                m_list.Clear();
            }
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            if (HasValue)
            {
                m_list.CopyTo(array, arrayIndex);
            }
        }

        public int IndexOf(T item)
        {
            if (!HasValue)
            {
                return -1;
            }

            return m_list.IndexOf(item);
        }

        public void Insert(int index, T item)
        {
            Value.Insert(index, item);
        }

        public void RemoveAt(int index)
        {
            if (HasValue)
            {
                m_list.RemoveAt(index);
            }
        }

        public bool Remove(T item)
        {
            if (!HasValue)
            {
                return false;
            }

            return m_list.Remove(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            if (!HasValue)
            {
                return EmptyEnumerator();
            }

            return Value.GetEnumerator();
        }

        public void ValidateRequired(string arg)
        {
            m_list.ValidateRequired(arg);
        }

        private static IEnumerator<T> EmptyEnumerator()
        {
            yield break;
        }
    }
}