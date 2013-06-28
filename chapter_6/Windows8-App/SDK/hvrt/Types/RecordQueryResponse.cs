// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class RecordQueryResponse : IHealthVaultTypeSerializable
    {
        [XmlElement("group")]
        public ItemQueryResult[] Results { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Results.ValidateOptional("Results");
        }

        #endregion
    }
}