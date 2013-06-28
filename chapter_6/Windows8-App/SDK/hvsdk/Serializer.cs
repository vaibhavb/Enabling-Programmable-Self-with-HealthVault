// (c) Microsoft. All rights reserved
using System;
using System.IO;
using System.Threading;
using System.Xml;
using System.Xml.Serialization;

namespace HealthVault.Foundation
{
    public class Serializer : ISerializer
    {
        //private static readonly ThreadLocal<XmlReader> s_currentReader = new ThreadLocal<XmlReader>();

        private readonly SerializerFactory m_factory;
        private readonly XmlSerializerNamespaces m_requestNamespaces;
        private readonly XmlSerializer m_requestSerializer;

        public Serializer()
        {
            m_requestNamespaces = new XmlSerializerNamespaces();
            m_requestNamespaces.Add("wc-request", Request.Namespace);

            m_factory = new SerializerFactory();
            m_requestSerializer = m_factory[typeof (Request)];
        }

        /*
        public static XmlReader CurrentReader
        {
            get { return s_currentReader.Value; }
        }
        */

        #region ISerializer Members

        public XmlSerializer SerializerForType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            return m_factory[type];
        }

        public void SetSerializerForType(Type type, XmlSerializer serializer)
        {
            if (type == null)
            {
                throw new ArgumentNullException("type");
            }

            m_factory[type] = serializer;
        }

        public void Serialize(TextWriter writer, object obj, object context)
        {
            if (obj == null)
            {
                return;
            }
            if (writer == null)
            {
                throw new ArgumentNullException("writer");
            }

            var request = obj as Request;
            if (request != null)
            {
                SerializeRequest(writer, request, context);
            }
            else
            {
                SerializeObject(writer, obj, context);
            }
        }

        public object Deserialize(TextReader reader, Type type, object context)
        {
            return DeserializeObject(reader, type, context);
        }

        #endregion

        private void SerializeRequest(TextWriter textWriter, Request request, object context)
        {
            // Delegate error checking to serializer
            using (var xmlWriter = (HealthVaultXmlWriter) HealthVaultXmlWriter.Create(textWriter))
            {
                xmlWriter.AllowRootPrefix = true;
                xmlWriter.Context = context;
                m_requestSerializer.Serialize(xmlWriter, request, m_requestNamespaces);
            }
        }

        private void SerializeObject(TextWriter textWriter, object obj, object context)
        {
            XmlSerializer serializer = m_factory[obj.GetType()];
            using (var xmlWriter = (HealthVaultXmlWriter) HealthVaultXmlWriter.Create(textWriter))
            {
                xmlWriter.Context = context;
                serializer.Serialize(xmlWriter, obj);
            }
        }

        private object DeserializeObject(TextReader reader, Type type, object context)
        {
            using (var xmlReader = (HealthVaultXmlReader) HealthVaultXmlReader.Create(reader))
            {
                xmlReader.Context = context;
                try
                {
                    //s_currentReader.Value = xmlReader;
                    return SerializerForType(type).Deserialize(xmlReader);
                }
                finally
                {
                    //s_currentReader.Value = null;
                }
            }
        }
    }
}