// (c) Microsoft. All rights reserved
using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public class SerializerFactory
    {
        private readonly Dictionary<Type, XmlSerializer> m_cache;
        private readonly Func<Type, XmlSerializer> m_constructor;

        public SerializerFactory()
            : this((type) => new XmlSerializer(type))
        {
        }

        public SerializerFactory(Func<Type, XmlSerializer> constructor)
        {
            if (constructor == null)
            {
                throw new ArgumentNullException("constructor");
            }
            m_cache = new Dictionary<Type, XmlSerializer>();
            m_constructor = constructor;
        }

        public XmlSerializer this[Type type]
        {
            get { return Ensure(type); }
            set
            {
                lock (m_cache)
                {
                    m_cache[type] = value;
                }
            }
        }

        public XmlSerializer Ensure(Type type)
        {
            lock (m_cache)
            {
                XmlSerializer serializer = null;
                if (!m_cache.TryGetValue(type, out serializer) || serializer == null)
                {
                    serializer = m_constructor(type);
                    m_cache[type] = serializer;
                }

                return serializer;
            }
        }
    }
}