// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class VocabCodeSet : IHealthVaultTypeSerializable
    {
        public VocabCodeSet()
        {
            Name = String.Empty;
            Family = String.Empty;
            Version = String.Empty;
        }

        [XmlElement("name", Order = 1)]
        public string Name { get; set; }

        [XmlElement("family", Order = 2)]
        public string Family { get; set; }

        [XmlElement("version", Order = 3)]
        public string Version { get; set; }

        [XmlElement("code-item", Order = 4)]
        public VocabItem[] Items { get; set; }

        [XmlElement("is-vocab-truncated", Order = 5)]
        public BooleanValue IsTruncated { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
        }

        #endregion

        public static VocabCodeSet Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<VocabCodeSet>(xml);
        }

        public bool ShouldSerializeName()
        {
            return !String.IsNullOrEmpty(Name);
        }

        public bool ShouldSerializeFamily()
        {
            return !String.IsNullOrEmpty(Family);
        }

        public bool ShouldSerializeVersion()
        {
            return !String.IsNullOrEmpty(Version);
        }
    }
}