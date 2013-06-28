// (c) Microsoft. All rights reserved

using System;
using System.Xml.Serialization;
using HealthVault.Foundation;

namespace HealthVault.Types
{
    public sealed class StructuredNameValue : IHealthVaultTypeSerializable
    {
        public StructuredNameValue()
        {
        }

        public StructuredNameValue(CodedValue name, StructuredMeasurement measurement)
        {
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }

            if (measurement == null)
            {
                throw new ArgumentNullException("value");
            }

            Name = name;
            Value = measurement;
        }

        [XmlElement("name", Order = 1)]
        public CodedValue Name { get; set; }

        [XmlElement("value", Order = 2)]
        public StructuredMeasurement Value { get; set; }

        #region IHealthVaultTypeSerializable Members

        public string Serialize()
        {
            return this.ToXml();
        }

        public void Validate()
        {
            Name.ValidateRequired("Name");
            Value.ValidateRequired("Value");
        }

        #endregion
    }
}