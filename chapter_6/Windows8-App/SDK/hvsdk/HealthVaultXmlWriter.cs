// (c) Microsoft. All rights reserved
using System;
using System.IO;
using System.Text;
using System.Xml;

namespace HealthVault.Foundation
{
    /// <summary>
    /// The fragment writer works with the Xml Serializer to emit Xml that HealthVault accepts. 
    ///  - Namespaces ignored
    ///  - Serialization of XElements ("Any" element) fixed up
    ///  - Fragment conformance
    /// </summary>
    public class HealthVaultXmlWriter : XmlWriter
    {
        private const string XElementName = "XElement";

        internal static XmlWriterSettings WriterSettings = new XmlWriterSettings
                                                             {
                                                                 OmitXmlDeclaration = true,
                                                                 Indent = false,
                                                                 CloseOutput = false,
                                                                 ConformanceLevel = ConformanceLevel.Fragment
                                                             };

        internal static XmlWriterSettings WriterSettingsIndent = new XmlWriterSettings
                                                                   {
                                                                       OmitXmlDeclaration = true,
                                                                       Indent = true,
                                                                       CloseOutput = false,
                                                                       ConformanceLevel = ConformanceLevel.Fragment
                                                                   };

        private int m_depth;
        private bool m_ignoreAttribute;
        private XmlWriter m_inner;
        private int m_skipElementAtDepth;

        public HealthVaultXmlWriter(XmlWriter inner)
        {
            Inner = inner;
        }

        internal HealthVaultXmlWriter()
        {
        }

        public XmlWriter Inner
        {
            get { return m_inner; }
            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException();
                }

                Init(value);
            }
        }

        public bool IgnoreType { get; set; }

        public override string XmlLang
        {
            get { return m_inner.XmlLang; }
        }

        public override XmlSpace XmlSpace
        {
            get { return m_inner.XmlSpace; }
        }

        public override XmlWriterSettings Settings
        {
            get { return m_inner.Settings; }
        }

        public bool AllowRootPrefix { get; set; }

        /// <summary>
        /// Arbitrary context
        /// </summary>
        public object Context { get; set; }

        public override WriteState WriteState
        {
            get { return m_inner.WriteState; }
        }

        public void Clear()
        {
            Init(null);
        }

        private void Init(XmlWriter inner)
        {
            m_inner = inner;
            m_ignoreAttribute = false;
            m_depth = 0;
            m_skipElementAtDepth = -1;
        }

        public override void Flush()
        {
            m_inner.Flush();
        }

        public override string LookupPrefix(string ns)
        {
            return m_inner.LookupPrefix(ns);
        }

        public override void WriteQualifiedName(string localName, string ns)
        {
            m_inner.WriteName(localName);
        }

        public override void WriteBase64(byte[] buffer, int index, int count)
        {
            m_inner.WriteBase64(buffer, index, count);
        }

        public override void WriteCData(string text)
        {
            m_inner.WriteCData(text);
        }

        public override void WriteCharEntity(char ch)
        {
            m_inner.WriteCharEntity(ch);
        }

        public override void WriteChars(char[] buffer, int index, int count)
        {
            m_inner.WriteChars(buffer, index, count);
        }

        public override void WriteComment(string text)
        {
        }

        public override void WriteDocType(string name, string pubid, string sysid, string subset)
        {
        }

        public override void WriteEndAttribute()
        {
            if (m_ignoreAttribute)
            {
                m_ignoreAttribute = false;
                return;
            }

            m_inner.WriteEndAttribute();
        }

        public override void WriteEndDocument()
        {
            // Ignore
        }

        public override void WriteEndElement()
        {
            if (m_depth == m_skipElementAtDepth)
            {
                m_skipElementAtDepth = -1;
            }
            else
            {
                m_inner.WriteEndElement();
            }
            --m_depth;
        }

        public override void WriteEntityRef(string name)
        {
            m_inner.WriteEntityRef(name);
        }

        public override void WriteFullEndElement()
        {
            if (m_depth == m_skipElementAtDepth)
            {
                m_skipElementAtDepth = -1;
            }
            else
            {
                m_inner.WriteFullEndElement();
            }
            --m_depth;
        }

        public override void WriteProcessingInstruction(string name, string text)
        {
            // Ignore
        }

        public override void WriteRaw(string data)
        {
            m_inner.WriteRaw(data);
        }

        public override void WriteRaw(char[] buffer, int index, int count)
        {
            m_inner.WriteRaw(buffer, index, count);
        }

        public override void WriteStartAttribute(string prefix, string localName, string ns)
        {
            if (localName == "xsd" || localName == "xsi" || localName == "type")
            {
                m_ignoreAttribute = true;
                return;
            }
            m_inner.WriteStartAttribute(localName);
        }

        public override void WriteStartDocument(bool standalone)
        {
            // Ignore
        }

        public override void WriteStartDocument()
        {
            // Ignore
        }

        public override void WriteStartElement(string prefix, string localName, string ns)
        {
            ++m_depth;
            if (localName.Length == XElementName.Length && localName == XElementName)
            {
                m_skipElementAtDepth = m_depth;
            }
            else
            {
                if (m_depth == 1 && AllowRootPrefix)
                {
                    m_inner.WriteStartElement(prefix, localName, ns);
                }
                else
                {
                    m_inner.WriteStartElement(localName);
                }
            }
        }

        public override void WriteString(string text)
        {
            if (m_ignoreAttribute)
            {
                return;
            }
            m_inner.WriteString(text);
        }

        public override void WriteSurrogateCharEntity(char lowChar, char highChar)
        {
            m_inner.WriteSurrogateCharEntity(lowChar, highChar);
        }

        public override void WriteWhitespace(string ws)
        {
            m_inner.WriteWhitespace(ws);
        }

        public override void WriteName(string name)
        {
            m_inner.WriteName(name);
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

        public new static XmlWriter Create(XmlWriter inner)
        {
            //inner.Settings.OmitXmlDeclaration = true;
            return new HealthVaultXmlWriter(inner);
        }

        public new static XmlWriter Create(TextWriter textWriter)
        {
            return Create(textWriter, false);
        }

        public static XmlWriter Create(TextWriter textWriter, bool indent)
        {
            XmlWriter inner = Create(textWriter, indent ? WriterSettingsIndent : WriterSettings);
            return new HealthVaultXmlWriter(inner);
        }

        public new static XmlWriter Create(StringBuilder builder)
        {
            return Create(builder, false);
        }

        public static XmlWriter Create(StringBuilder builder, bool indent)
        {
            return new HealthVaultXmlWriter(Create(builder, indent ? WriterSettingsIndent : WriterSettings));
        }

        public new static XmlWriter Create(Stream stream)
        {
            XmlWriter inner = Create(stream, WriterSettings);
            return new HealthVaultXmlWriter(inner);
        }
    }
}