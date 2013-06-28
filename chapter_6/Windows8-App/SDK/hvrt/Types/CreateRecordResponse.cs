// (c) Microsoft. All rights reserved

using HealthVault.Foundation;
using System.Xml.Serialization;

namespace HealthVault.Types
{
    public sealed class CreateRecordResponse : IHealthVaultType
    {
        public CreateRecordResponse()
        {
        }

        [XmlElement("record-id", Order = 1)]
        public string RecordId
        {
            get;
            set;
        }

        public void Validate()
        {
            RecordId.ValidateRequired("RecordId");
        }
    }
}
