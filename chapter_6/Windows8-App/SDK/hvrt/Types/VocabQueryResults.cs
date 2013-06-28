// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class VocabQueryResults : IHealthVaultTypeSerializable
    {
        [XmlElement("code-set-result")]
        public VocabQueryResult Matches { get; set; }

        [XmlIgnore]
        public bool HasMatches
        {
            get { return Matches != null; }
        }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Matches.ValidateOptional("Matches");
        }

        #endregion
    }
}