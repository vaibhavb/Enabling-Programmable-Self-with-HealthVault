// (c) Microsoft. All rights reserved

using HealthVault.Foundation;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    [XmlType(TypeName="permission")]
    public sealed class Permission : IHealthVaultTypeSerializable
    {
        [XmlText]
        public string Value { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Value.ValidateRequired("Value");
        }

        #endregion
    }
}