// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class NutritionFact : IHealthVaultTypeSerializable
    {
        [XmlElement("name", Order = 1)]
        public CodableValue Name { get; set; }

        [XmlElement("fact", Order = 2)]
        public GeneralMeasurement Fact { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Name.ValidateRequired("Name");
            Fact.ValidateOptional("Fact");
        }

        #endregion
    }
}