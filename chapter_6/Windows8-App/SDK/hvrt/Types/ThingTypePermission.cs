// (c) Microsoft. All rights reserved

using HealthVault.Foundation;
using System;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class ThingTypePermission : IHealthVaultTypeSerializable
    {
        [XmlElement("thing-type-id", Order = 1)]
        public string ThingTypeId { get; set; }

        [XmlArray("online-access-permissions", Order = 2)]
        public Permission[] OnlineAccessPermissions { get; set; }

        [XmlArray("offline-access-permissions", Order = 3)]
        public Permission[] OfflineAccessPermissions { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            ThingTypeId.ValidateRequired("ThingTypeId");
            OnlineAccessPermissions.ValidateOptional<Permission>("OnlineAccessPermissions");
            OfflineAccessPermissions.ValidateOptional<Permission>("OfflineAccessPermissions");
        }

        #endregion
    }
}