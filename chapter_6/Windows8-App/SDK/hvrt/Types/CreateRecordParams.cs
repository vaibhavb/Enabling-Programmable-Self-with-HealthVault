// (c) Microsoft. All rights reserved

using HealthVault.Foundation;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    [XmlType("record-info")]
    public sealed class CreateRecordParams : IHealthVaultType
    {
        [XmlElement("name", Order = 1)]
        public string Name { get; set; }

        [XmlElement("rel-id", Order = 2)]
        public int RelationshipId { get; set; }

        [XmlElement("datastore-name", Order = 3)]
        public string DataStoreName { get; set; }

        [XmlElement("display-name", Order = 4)]
        public string DisplayName { get; set; }

        [XmlElement("location", Order = 5)]
        public Location Location { get; set; }

        public void Validate()
        {
            Name.ValidateRequired("Name");
            Location.ValidateOptional("Location");
        }
    }
}
