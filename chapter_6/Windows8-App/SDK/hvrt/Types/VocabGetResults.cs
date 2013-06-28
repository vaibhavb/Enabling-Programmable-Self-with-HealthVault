// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class VocabGetResults : IHealthVaultTypeSerializable
    {
        [XmlElement("vocabulary")]
        public VocabCodeSet[] Vocabs { get; set; }

        [XmlIgnore]
        public bool HasVocabs
        {
            get { return !(Vocabs.IsNullOrEmpty()); }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Vocabs.ValidateOptional("Vocabs");
        }

        #endregion

        public static VocabGetResults Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<VocabGetResults>(xml);
        }
    }
}