// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ItemDataCommon : IHealthVaultTypeSerializable
    {
        private String255 m_clientId;

        public ItemDataCommon()
        {
            Source = String.Empty;
            Note = String.Empty;
            Tags = String.Empty;
        }

        [XmlElement("source")]
        public string Source { get; set; }

        [XmlElement("note")]
        public string Note { get; set; }

        [XmlElement("tags")]
        public string Tags { get; set; }

        [XmlElement("extension")]
        public ItemExtension[] Extensions { get; set; }

        [XmlElement("related-thing")]
        public RelatedItem[] RelatedItems { get; set; }

        [XmlElement("client-thing-id")]
        public string ClientId
        {
            get { return m_clientId != null ? m_clientId.Value : String.Empty; }
            set { m_clientId = !String.IsNullOrEmpty(value) ? new String255(value) : null; }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            m_clientId.ValidateOptional("ClientId");
        }

        #endregion

        public bool ShouldSerializeClientId()
        {
            return !String.IsNullOrEmpty(ClientId);
        }

        public bool ShouldSerializeSource()
        {
            return !String.IsNullOrEmpty(Source);
        }

        public bool ShouldSerializeNote()
        {
            return !String.IsNullOrEmpty(Note);
        }

        public bool ShouldSerializeTags()
        {
            return !String.IsNullOrEmpty(Tags);
        }
    }
}