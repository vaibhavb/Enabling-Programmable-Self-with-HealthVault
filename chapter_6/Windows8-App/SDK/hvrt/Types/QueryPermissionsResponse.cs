// (c) Microsoft. All rights reserved

using HealthVault.Foundation;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class QueryPermissionsResponse : IHealthVaultTypeSerializable
    {
        [XmlElement("thing-type-permission", Order = 1)]
        public ThingTypePermission ThingTypePermission { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            ThingTypePermission.ValidateOptional("ThingTypePermission");
        }

        #endregion
    }
}
