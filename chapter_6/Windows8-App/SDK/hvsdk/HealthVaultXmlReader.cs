// (c) Microsoft. All rights reserved
using System;
using System.IO;
using System.Xml;

namespace HealthVault.Foundation
{
    /// <summary>
    /// BodyReader helps supply deserialization Context....
    /// </summary>
    public class HealthVaultXmlReader : XmlReader
    {
        internal static XmlReaderSettings ReaderSettings = new XmlReaderSettings
                                                           {
                                                               ConformanceLevel = ConformanceLevel.Fragment
                                                           };

        private XmlReader m_inner;
        private int m_rootDepth;
        private string m_rootLocalName;
        private string m_rootName;

        public HealthVaultXmlReader(XmlReader inner)
        {
            if (inner == null)
            {
                throw new ArgumentNullException("inner");
            }
            m_inner = inner;
            m_rootDepth = -1;
        }

        public string RootElementName
        {
            set
            {
                m_rootLocalName = null;
                m_rootName = null;
                m_rootDepth = -1;
                if (!string.IsNullOrEmpty(value))
                {
                    m_rootLocalName = m_inner.NameTable.Add(value);
                    m_rootDepth = m_inner.Depth;
                }
            }
        }

        /// <summary>
        /// Associated arbitrary context with this reader...
        /// </summary>
        public object Context { get; set; }

        public override int AttributeCount
        {
            get { return m_inner.AttributeCount; }
        }

        public override string BaseURI
        {
            get { return m_inner.BaseURI; }
        }

        public override bool CanResolveEntity
        {
            get { return m_inner.CanResolveEntity; }
        }

        public override bool CanReadBinaryContent
        {
            get { return m_inner.CanReadBinaryContent; }
        }

        public override bool CanReadValueChunk
        {
            get { return m_inner.CanReadValueChunk; }
        }

        public override int Depth
        {
            get { return (m_inner.Depth); }
        }

        public override bool EOF
        {
            get { return m_inner.EOF; }
        }

        public override bool HasAttributes
        {
            get { return m_inner.HasAttributes; }
        }

        public override bool HasValue
        {
            get { return m_inner.HasValue; }
        }

        public override bool IsEmptyElement
        {
            get { return m_inner.IsEmptyElement; }
        }

        public override bool IsDefault
        {
            get { return m_inner.IsDefault; }
        }

        public override string LocalName
        {
            get
            {
                if (IsAtRoot)
                {
                    return m_rootLocalName;
                }

                return m_inner.LocalName;
            }
        }

        public override string Name
        {
            get
            {
                if (IsAtRoot)
                {
                    if (m_rootName == null)
                    {
                        m_rootName = m_inner.Prefix + ':' + m_rootLocalName;
                    }
                    return m_rootName;
                }

                return m_inner.Name;
            }
        }

        public override XmlNameTable NameTable
        {
            get { return m_inner.NameTable; }
        }

        public override string NamespaceURI
        {
            get
            {
                if (IsAtRoot)
                {
                    return string.Empty;
                }
                return m_inner.NamespaceURI;
            }
        }

        public override XmlNodeType NodeType
        {
            get { return m_inner.NodeType; }
        }

        public override string Prefix
        {
            get
            {
                if (IsAtRoot)
                {
                    return string.Empty;
                }
                return m_inner.Prefix;
            }
        }

        public override ReadState ReadState
        {
            get { return m_inner.ReadState; }
        }

        public override XmlReaderSettings Settings
        {
            get { return m_inner.Settings; }
        }

        public override string Value
        {
            get { return m_inner.Value; }
        }

        public override Type ValueType
        {
            get { return m_inner.ValueType; }
        }

        public override string XmlLang
        {
            get { return m_inner.XmlLang; }
        }

        public override XmlSpace XmlSpace
        {
            get { return m_inner.XmlSpace; }
        }

        private bool IsAtRoot
        {
            get { return (m_inner.Depth == m_rootDepth); }
        }

        public override string GetAttribute(string name, string namespaceUri)
        {
            return m_inner.GetAttribute(name, namespaceUri);
        }

        public override string GetAttribute(string name)
        {
            return m_inner.GetAttribute(name);
        }

        public override string GetAttribute(int i)
        {
            return m_inner.GetAttribute(i);
        }

        public override string LookupNamespace(string prefix)
        {
            return m_inner.LookupNamespace(prefix);
        }

        public override bool MoveToAttribute(string name, string ns)
        {
            return m_inner.MoveToAttribute(name, ns);
        }

        public override bool MoveToAttribute(string name)
        {
            return m_inner.MoveToAttribute(name);
        }

        public override bool MoveToElement()
        {
            return m_inner.MoveToElement();
        }

        public override bool MoveToFirstAttribute()
        {
            return m_inner.MoveToFirstAttribute();
        }

        public override bool MoveToNextAttribute()
        {
            return m_inner.MoveToNextAttribute();
        }

        public override bool Read()
        {
            return m_inner.Read();
        }

        public override bool ReadAttributeValue()
        {
            return m_inner.ReadAttributeValue();
        }

        public override void ResolveEntity()
        {
            m_inner.ResolveEntity();
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            if (disposing)
            {
                if (m_inner != null)
                {
                    m_inner.Dispose();
                    m_inner = null;
                }
            }
        }

        public static XmlReader Create(XmlReader inner)
        {
            return new HealthVaultXmlReader(inner);
        }

        public new static XmlReader Create(TextReader reader)
        {
            XmlReader inner = Create(reader, ReaderSettings);
            return Create(inner);
        }

        public new static XmlReader Create(Stream stream)
        {
            XmlReader inner = Create(stream, ReaderSettings);
            return Create(inner);
        }
    }
}