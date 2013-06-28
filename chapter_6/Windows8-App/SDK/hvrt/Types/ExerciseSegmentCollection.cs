// (c) Microsoft. All rights reserved

using System;
using System.Collections;
using System.Collections.Generic;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public interface IExerciseSegmentCollection : IHealthVaultTypeSerializable, IList<ExerciseSegment>
    {
    }

    public sealed class ExerciseSegmentCollection : IExerciseSegmentCollection
    {
        private LazyList<ExerciseSegment> m_items;

        public ExerciseSegmentCollection()
        {
            m_items = new LazyList<ExerciseSegment>();
        }

        #region IExerciseSegmentCollection Members

        public int Count
        {
            get { return m_items.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public ExerciseSegment this[int index]
        {
            get { return m_items[index]; }
            set { m_items[index] = value; }
        }

        public void Add(ExerciseSegment item)
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

        public bool Contains(ExerciseSegment item)
        {
            return m_items.Contains(item);
        }

        public void CopyTo(ExerciseSegment[] array, int arrayIndex)
        {
            m_items.CopyTo(array, arrayIndex);
        }

        public int IndexOf(ExerciseSegment item)
        {
            return m_items.IndexOf(item);
        }

        public void Insert(int index, ExerciseSegment item)
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

        public bool Remove(ExerciseSegment item)
        {
            return m_items.Remove(item);
        }

        public IEnumerator<ExerciseSegment> GetEnumerator()
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