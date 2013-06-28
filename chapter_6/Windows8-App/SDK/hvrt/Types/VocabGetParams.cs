// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    [XmlType("vocabulary-parameters")]
    public sealed class VocabGetParams : IHealthVaultTypeSerializable
    {
        [XmlElement("vocabulary-key", Order = 1)]
        public VocabIdentifier[] VocabIDs { get; set; }

        [XmlElement("fixed-culture", Order = 2)]
        public bool FixedCulture { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            VocabIDs.ValidateRequired("VocabIDs");
        }

        #endregion
    }
}