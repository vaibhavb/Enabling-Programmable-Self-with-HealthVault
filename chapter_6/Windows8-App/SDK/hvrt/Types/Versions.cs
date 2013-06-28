// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Versions : IHealthVaultTypeSerializable
    {
        [XmlAttribute("thing-type-id")]
        public string TypeId { get; set; }

        [XmlElement("version-info")]
        public VersionInformationCollection VersionInformation { get; set; }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            VersionInformation.ValidateRequired<VersionInformation>("VersionInformation");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion
    }
}