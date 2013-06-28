// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class GoalAssociatedTypeInfo : IHealthVaultTypeSerializable
    {
        public GoalAssociatedTypeInfo()
        {
            ThingTypeVersionId = String.Empty;
            ThingTypeValueXpath = String.Empty;
            ThingTypeDisplayXpath = String.Empty;
        }

        [XmlElement("thing-type-version-id", Order = 1)]
        public string ThingTypeVersionId { get; set; }

        [XmlElement("thing-type-value-xpath", Order = 2)]
        public string ThingTypeValueXpath { get; set; }

        [XmlElement("thing-type-display-xpath", Order = 3)]
        public string ThingTypeDisplayXpath { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            ThingTypeVersionId.ValidateRequired("ThingTypeVersionId");
        }

        #endregion

        public bool ShouldSerializeThingTypeVersionId()
        {
            return !String.IsNullOrEmpty(ThingTypeVersionId);
        }

        public bool ShouldSerializeThingTypeValueXpath()
        {
            return !String.IsNullOrEmpty(ThingTypeValueXpath);
        }

        public bool ShouldSerializeThingTypeDisplayXpath()
        {
            return !String.IsNullOrEmpty(ThingTypeDisplayXpath);
        }

        public override string ToString()
        {
            return ThingTypeVersionId ?? String.Empty;
        }
    }
}
