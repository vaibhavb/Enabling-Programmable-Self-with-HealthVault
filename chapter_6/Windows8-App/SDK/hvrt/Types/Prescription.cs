// (c) Microsoft. All rights reserved

using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class Prescription : IHealthVaultTypeSerializable
    {
        [XmlElement("prescribed-by", Order = 1)]
        public Person PrescribedBy { get; set; }

        [XmlElement("date-prescribed", Order = 2)]
        public ApproxDateTime DatePrescribed { get; set; }

        [XmlElement("amount-prescribed", Order = 3)]
        public ApproxMeasurement Amount { get; set; }

        [XmlElement("substitution", Order = 4)]
        public CodableValue Substitution { get; set; }

        [XmlElement("refills", Order = 5)]
        public NonNegativeInt Refills { get; set; }

        [XmlElement("days-supply", Order = 6)]
        public PositiveInt DaysSupply { get; set; }

        [XmlElement("prescription-expiration", Order = 7)]
        public Date Expiration { get; set; }

        [XmlElement("instructions", Order = 8)]
        public CodableValue Instructions { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            PrescribedBy.ValidateRequired("PrescribedBy");
            DatePrescribed.ValidateOptional("DatePrescribed");
            Amount.ValidateOptional("Amount");
            Substitution.ValidateOptional("Substitution");
            Refills.ValidateOptional("Refills");
            DaysSupply.ValidateOptional("DaysSupply");
            Expiration.ValidateOptional("Expiration");
            Instructions.ValidateOptional("Instructions");
        }

        #endregion

        public static Person Deserialize(string xml)
        {
            return HealthVaultClient.Serializer.FromXml<Person>(xml);
        }
    }
}