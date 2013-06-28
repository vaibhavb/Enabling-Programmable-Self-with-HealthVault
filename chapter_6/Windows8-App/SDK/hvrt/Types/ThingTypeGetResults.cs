// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class ThingTypeGetResults : IHealthVaultTypeSerializable
    {
        [XmlElement("thing-type")]
        public ItemTypeDefinition[] ItemTypeDefinitions { get; set; }

        #region IHealthVaultTypeSerializable Members

        public void Validate()
        {
            ItemTypeDefinitions.ValidateOptional("ItemTypeDefinitions");
        }

        public string Serialize()
        {
            return this.ToXml();
        }

        #endregion

        public static ThingTypeGetResults Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<ThingTypeGetResults>(xml);
        }
    }
}