// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class VersionInformation : IHealthVaultTypeSerializable
    {
        [XmlAttribute("version-type-id")]
        public string TypeId { get; set; }

        [XmlAttribute("version-name")]
        public string Name { get; set; }

        [XmlAttribute("version-sequence")]
        public int Sequence { get; set; }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            TypeId.ValidateRequired("TypeId");
            Name.ValidateRequired("Name");
            Sequence.ValidateRequired("Sequence");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion
    }
}