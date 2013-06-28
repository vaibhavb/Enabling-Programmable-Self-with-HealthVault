// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class GoalRange : IHealthVaultTypeSerializable
    {
        public GoalRange()
        {
            Description = String.Empty;
        }

        [XmlElement("name", Order = 1)]
        public CodableValue Name { get; set; }

        [XmlElement("description", Order = 2)]
        public string Description { get; set; }

        [XmlElement("minimum", Order = 3)]
        public GeneralMeasurement Minimum { get; set; }

        [XmlElement("maximum", Order = 4)]
        public GeneralMeasurement Maximum { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Name.ValidateRequired("Name");
            Minimum.ValidateOptional("Minimum");
            Maximum.ValidateOptional("Maximum");
        }

        #endregion

        public bool ShouldSerializeDescription()
        {
            return !String.IsNullOrEmpty(Description);
        }

        public override string ToString()
        {
            if (Minimum != null && Maximum != null)
            {
                return String.Format("{0} - {1}", Minimum, Maximum);
            }
            else if (Minimum != null)
            {
                return Minimum.ToString();
            }
            else if (Maximum != null)
            {
                return Maximum.ToString();
            }
            else if (Name != null)
            {
                return Name.ToString();
            } else
            {
                return String.Empty;
            }
        }
    }
}
