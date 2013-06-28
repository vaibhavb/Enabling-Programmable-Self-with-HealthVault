// (c) Microsoft. All rights reserved
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public class Optional<T>
        where T : struct
    {
        private T m_value;

        public Optional()
        {
        }

        public Optional(T value)
        {
            m_value = value;
        }

        [XmlText]
        public T Value
        {
            get { return m_value; }
            set { m_value = value; }
        }

        public override bool Equals(object obj)
        {
            return m_value.Equals(obj);
        }

        public override int GetHashCode()
        {
            return m_value.GetHashCode();
        }

        public override string ToString()
        {
            return ((T) this).ToString();
        }

        public static implicit operator T(Optional<T> optional)
        {
            if (optional == null)
            {
                return default(T);
            }
            return optional.Value;
        }

        public static implicit operator Optional<T>(T value)
        {
            return new Optional<T>(value);
        }
    }
}