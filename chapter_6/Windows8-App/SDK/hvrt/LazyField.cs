// (c) Microsoft. All rights reserved

namespace HealthVault
{
    //
    // Use this instead of Lazy<T> 
    // -You can SET the value...
    //
    internal struct LazyField<T>
        where T : class, new()
    {
        private T m_field;

        public T Value
        {
            get
            {
                if (m_field == null)
                {
                    m_field = new T();
                }

                return m_field;
            }
            set { m_field = value; }
        }

        public bool HasValue
        {
            get { return (m_field != null); }
        }
    }
}