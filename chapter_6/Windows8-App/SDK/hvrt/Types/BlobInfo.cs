// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class BlobInfo : IHealthVaultType
    {
        private StringZ1024 m_contentType;
        private StringZ255 m_name;

        public BlobInfo()
        {
        }

        public BlobInfo(string contentType)
            : this(contentType, string.Empty) // Default blob
        {
        }

        public BlobInfo(string contentType, string name)
        {
            Name = name;
            ContentType = contentType;
        }

        [XmlElement("name", Order = 1)]
        public string Name
        {
            get { return (m_name != null) ? m_name.Value : String.Empty; }
            set
            {
                m_name = value != null
                    ? new StringZ255(value)
                    : null;
            }
        }

        [XmlElement("content-type", Order = 2)]
        public string ContentType
        {
            get { return (m_contentType != null) ? m_contentType.Value : String.Empty; }
            set { m_contentType = value != null ? new StringZ1024(value) : null; }
        }

        #region IHealthVaultType Members

        public void Validate()
        {
            m_name.ValidateRequired("Name");
            m_contentType.ValidateRequired("ContentType");
        }

        #endregion

        public bool ShouldSerializeName()
        {
            return m_name != null;
        }

        public bool ShouldSerializeContentType()
        {
            return m_contentType != null;
        }
    }
}